using System.Diagnostics;

namespace GFXFontEditor
{
	/// <summary>
	/// Load 'Glyph Bitmap Distribution Format' (BDF) files
	/// </summary>
	public class BdfParser
	{
		// https://adobe-type-tools.github.io/font-tech-notes/pdfs/5005.BDF_Spec.pdf
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

		void Error(string message)
		{
			throw new Exception($"[{Path.GetFileName(fileName)}][line:{lineNumber}] {message}");
		}

		/// <summary>
		/// Read a line from the file
		/// </summary>
		void ReadLine()
		{
			line = file.ReadLine();
			lineNumber++;
			if (line is null)
				Error("unexpected end of file");
		}

		/// <summary>
		/// Read an input line, parse into space-separated tokens and set the first as a keyWord.
		/// </summary>
		void ParseLine()
		{
			ReadLine();
			tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			keyWord = tokens.FirstOrDefault() ?? "";
		}

		/// <summary>
		/// Create a list of integer value parameters from the parsed tokens following the 1st keyWord.
		/// </summary>
		/// <param name="cnt">Number of values</param>
		/// <param name="optionalCount">Number of optional values</param>
		/// <returns>List of integer parameter values</returns>
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

		/// <summary>
		/// Return a two-tuple of parameter values
		/// </summary>
		/// <param name="optionalCount">Number of optional values</param>
		/// <returns>Tuple of values</returns>
		(int, int) Values2(int optionalCount = 0)
		{
			var values = Values(2, optionalCount);
			return (values[0], values[1]);
		}

		/// <summary>
		/// Return a four-tuple of parameter values
		/// </summary>
		/// <returns>Tuple of values</returns>
		(int, int, int, int) Values4()
		{
			var values = Values(4);
			return (values[0], values[1], values[2], values[3]);
		}

#if true
		/// <summary>
		/// Output a Debug message, with filename and linenumber.
		/// </summary>
		/// <param name="msg">The message to output</param>
		void Trace(string msg)
		{
			Debug.WriteLine($"[{fileName}][{lineNumber}] {msg}");
		}

		/// <summary>
		/// Output a Debug message, with filename and linenumber, if a condition is false.
		/// </summary>
		/// <param name="cond">The test condition</param>
		/// <param name="msg">The message to output</param>
		void TraceAssert(bool cond, string msg)
		{
			if (!cond)
				Trace(msg);
		}
#endif

		/// <summary>
		/// Load a BDF file.
		/// </summary>
		/// <returns>The font</returns>
		/// <exception cref="Exception">Errors while parsing the file</exception>
		public GfxFont Load()
		{
			(int x, int y) globaldwidth = (0, 0);
			Rectangle fontBoundingBox = new();

			void ParseChar()
			{
				Rectangle bbx = Rectangle.Empty;
				(int x, int y) dwidth = (0, 0);
				SparseMap map = new();
				int code_point = -1;

				void ParseBitmap()
				{
					// BITMAP consists of BBX.Height lines of hex digits, one per Y row
					// each line has an even number of hex digits
					// enough to hold BBX.Width bits (X columns) starting with the most significant bit
					int y = 0;
					while (true)
					{
						ParseLine();
						if (keyWord == "ENDCHAR")
							return;
						int x = 0;
						// take one hex digit at a time
						foreach (var digit in keyWord)
						{
							if (!int.TryParse("" + digit, System.Globalization.NumberStyles.HexNumber, null, out int v))
								Error("error parsing bitmap");
							if (v != 0)
							{
								// four bits in the nibble
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
								// quickly skip a 0 nibble
								x += 4;
							}
							if (x >= bbx.Width)
								break;
						}
						++y;
					}
				}

				void CreateGlyph()
				{
					// default to global DWIDTH, if none for glyph
					var dx = dwidth.x == 0 ? globaldwidth.x : dwidth.x;
					//var dy = dwidth.y == 0 ? globaldwidth.y : dwidth.y;
					var code = (ushort)((code_point == -1 || code_point > 0xFFFF) ? 0xFFFF : code_point);
					var dup = code != 0xFFFF && glyphs.Any(g => g.Code == code);
					TraceAssert(!dup, $"glyph with duplicate code: {code}");
					// Y offset for glyph:
					//		move reference from glyph top to origin (bottom)
					//		then add BBX.Y offset (negative is up!)
					var glyph = new Glyph(map, dx)
					{
						Code = code,
						Status = (code == 0xFFFF || dup) ? Glyph.States.Error : Glyph.States.Normal
					};
					glyph.Offset(bbx.X, -bbx.Y - bbx.Height);
					glyphs.Add(glyph);
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
							// ENCODING
							//		code point
							//		[non-standard encoding] if code point is -1
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
							// DWIDTH
							//		x advance to next glyph
							//		y advance (not useful here)
							dwidth = Values2();
							break;
						case "BBX":
							{
								// BBX
								//		width of bitmap pixels
								//		height of bitmap pixels
								//		x offset for rendering from origin
								//		y offset for rendering from origin
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
				var avgAdv = glyphs.Average(g => g.xAdvance);
				foreach (var glyph in glyphs)
				{
					if (glyph.Status == Glyph.States.Inserted)
						glyph.xAdvance = (int)Math.Round(avgAdv);
				}
				GfxFont font = new()
				{
					yAdvance = fontBoundingBox.Height
				};
				font.AddGlyphs(glyphs);
				return font;
			}

			ParseLine();
			if (!line.StartsWith("STARTFONT 2.1"))
				Error("Unsupported file or version");

			while (true)
			{
				ParseLine();

				switch (keyWord)
				{
					case "FONTBOUNDINGBOX":
						{
							// FONTBOUNDINGBOX
							//		here only using the height as the (yAdvance) line height
							//		(as Adafruit does in its Python libraries)
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
						// font global DWIDTH
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
