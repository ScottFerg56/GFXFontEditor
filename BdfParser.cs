using System.Diagnostics;

namespace GFXFontEditor
{
	public class BdfParser
	{
		readonly StreamReader file;
		readonly string fileName;
		public List<Glyph> glyphs = new();
		public List<Glyph> glyphsErr = new();
		private int lineNumber = -1;
		string line;
		List<string> tokens;
		string keyWord = "";

		public BdfParser(string fileName)
		{
			this.fileName = fileName;
			file = File.OpenText(fileName);
		}

		public static IEnumerable<(string ext, string title)> GetExtensions()
		{
			yield return (".bdf", "BDF");
		}

		private void Error(string message)
		{
			throw new Exception($"[{Path.GetFileName(fileName)}][line:{lineNumber}] {message}");
		}

		/// <summary>
		/// Read a line from the file, absorbing comment lines.
		/// </summary>
		/// <returns>The line read</returns>
		public void ReadLine()
		{
			line = file.ReadLine();
			lineNumber++;
			if (line is null)
				Error("unexpected end of file");
		}

		void ParseLine()
		{
			ReadLine();
			tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			keyWord = tokens.FirstOrDefault() ?? "";
		}

		List<int> Values(int cnt, int optionalCount = 0)
		{
			List<int> values = new();
			for (int i = 1; i <= cnt; i++)
			{
				if (i >= tokens.Count)
				{
					if (i - (tokens.Count - 1) <= optionalCount)
						values.Add(int.MinValue);
					else
						Error("not enough parameters supplied");
				}
				else if (int.TryParse(tokens[i], out int v))
				{
					values.Add(v);
				}
				else
				{
					Error($"cannot parse integer parameter {tokens[i]}");
				}
			}
			return values;
		}

		//int Values1()
		//{
		//	return Values(1)[0];
		//}

		(int, int) Values2(int optionalCount = 0)
		{
			var values = Values(2, optionalCount);
			return (values[0], values[1]);
		}

		//(int, int, int) Values3()
		//{
		//	var values = Values(3);
		//	return (values[0], values[1], values[2]);
		//}

		(int, int, int, int) Values4()
		{
			var values = Values(4);
			return (values[0], values[1], values[2], values[3]);
		}

		/// <summary>
		/// Output a Debug message, with filename and linenumber.
		/// </summary>
		/// <param name="msg">The message to output</param>
		public void Trace(string msg)
		{
			Debug.WriteLine($"[{fileName}][{lineNumber}] {msg}");
		}

		/// <summary>
		/// Output a Debug message, with filename and linenumber, if a condition is false.
		/// </summary>
		/// <param name="cond">The test condition</param>
		/// <param name="msg">The message to output</param>
		public void TraceAssert(bool cond, string msg)
		{
			if (!cond)
				Trace(msg);
		}

		public GfxFont Load()
		{
			// TODO: any use for BOUNDINGBOX??
			(int x, int y) globaldwidth = (0, 0);
			Rectangle fontBoundingBox = new();
			int maxHeight = int.MinValue;
			int minHeight = int.MaxValue;

			void ParseChar()
			{
				Rectangle bbx = Rectangle.Empty;
				(int x, int y) dwidth = (0, 0);
				SparseMap map = new();
				int code_point = -1;

				void ParseBitmap()
				{
					int y = 0;
					while (true)
					{
						ParseLine();
						if (keyWord == "ENDCHAR")
							return;
						int x = 0;
						foreach (var nibble in keyWord)
						{
							if (!int.TryParse("" + nibble, System.Globalization.NumberStyles.HexNumber, null, out int v))
								Error("error parsing bitmap");
							if (v != 0)
							{
								for (int msk = 0x08; msk != 0; msk >>= 1)
								{
									if ((v & msk) != 0)
										map.Set(x, y);
									if (++x >= bbx.Width)
										break;
								}
							}
							else
							{
								x += 4;
							}
							if (x >= bbx.Width)
								break;
						}
						if (++y >= bbx.Height)
							return;
					}
				}

				void CreateGlyph()
				{
					var dx = dwidth.x == 0 ? globaldwidth.x : dwidth.x;
					var dy = dwidth.y == 0 ? globaldwidth.y : dwidth.y;
					var code = (ushort)(code_point == -1 ? 0xFFFF : code_point);
					if (glyphs.Any(g => g.Code == code))
						code = 0xFFFF;
					var glyph = new Glyph(map, bbx.X, -bbx.Y - bbx.Height, dx)
					{
						Code = code,
						Status = code == 0xFFFF ? Glyph.States.Error : Glyph.States.Normal
					};
					glyphs.Add(glyph);
					maxHeight = Math.Max(maxHeight, bbx.Height);
					minHeight = Math.Min(minHeight, bbx.Height);
				}

				ParseLine();
				if (keyWord != "STARTCHAR")
					return;
				while (true)
				{
					ParseLine();
					switch (keyWord)
					{
						case "ENCODING":
							(code_point, int cp2) = Values2(1);
							if (code_point == -1 && cp2 != int.MinValue)
								code_point = cp2;
							break;
						case "SWIDTH":
						case "SWIDTH1":
						case "DWIDTH1":
						case "VVECTOR":
							break;
						case "DWIDTH":
							dwidth = Values2();
							break;
						case "BBX":
							{
								(int w, int h, int xOff, int yOff) = Values4();
								bbx = new(xOff, yOff, w, h);
							}
							break;
						case "BITMAP":
							ParseBitmap();
							CreateGlyph();
							return;
						case "ENDCHAR":
							CreateGlyph();
							return;
					}
				}
			}

			void ParseChars()
			{
				while (true)
				{
					ParseChar();
					if (keyWord == "ENDFONT")
						return;
				}
			}

			GfxFont CreateFont()
			{
				if (maxHeight == int.MinValue)
					maxHeight = 0;
				if (minHeight == int.MaxValue)
					minHeight = 0;
				TraceAssert(minHeight == maxHeight, $"map height varies from {minHeight} to {maxHeight}");
				GfxFont.FixGaps(glyphs);
				var avgAdv = glyphs.Average(g => g.xAdvance);
				foreach (var glyph in glyphs)
				{
					//glyph.Offset(0, -maxHeight);
					if (glyph.Status == Glyph.States.Inserted)
						glyph.xAdvance = (int)Math.Round(avgAdv);
				}
				GfxFont font = new()
				{
					FirstCode = glyphs[0].Code,
					yAdvance = fontBoundingBox.Height
				};
				font.AddGlyphs(glyphs);
				return font;
			}

			ParseLine();
			if (!line.StartsWith("STARTFONT 2.1"))
				throw new Exception("Unsupported file version");

			while (true)
			{
				ParseLine();

				switch (keyWord)
				{
					case "FONTBOUNDINGBOX":
						{
							(int w, int h, int xOff, int yOff) = Values4();
							fontBoundingBox = new(xOff, yOff, w, h);
						}
						break;
					case "SWIDTH":
					case "SWIDTH1":
					case "DWIDTH1":
					case "VVECTOR":
						break;
					case "DWIDTH":
						globaldwidth = Values2();
						break;
					case "CHARS":
						ParseChars();
						return CreateFont();
					case "ENDFONT":
						return CreateFont();
					case "COMMENT":
					default:
						break;
				}
			}
		}
	}
}
