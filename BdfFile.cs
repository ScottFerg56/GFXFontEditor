﻿using System.Diagnostics;

namespace GFXFontEditor
{
	/// <summary>
	/// Load 'Glyph Bitmap Distribution Format' (BDF) files
	/// </summary>
	/// <exception cref="Exception">Errors while parsing the file</exception>
	public static class BdfFile
	{
		public static IEnumerable<(string ext, string title)> GetExtensions()
		{
			yield return (".bdf", "BDF");
		}

		/// <summary>
		/// Load a BDF file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont Load(string fileName)
		{
			// https://adobe-type-tools.github.io/font-tech-notes/pdfs/5005.BDF_Spec.pdf
			StreamReader file = File.OpenText(fileName);
			List<Glyph> glyphs = new();
			List<Glyph> glyphsErr = new();
			int lineNumber = -1;
			string line;
			List<string> tokens;
			string keyWord = "";

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
			/// Return a single value.
			/// </summary>
			/// <returns>int value</returns>
			int Values1() => Values(1)[0];

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

			(int x, int y) globaldwidth = (0, 0);
			Rectangle fontBoundingBox = new();
			FontProperties fontProperties = new();

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
						Status = dup ? Glyph.States.Duplicate : code == 0xFFFF ? Glyph.States.NoCode : Glyph.States.Normal
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
					yAdvance = fontBoundingBox.Height,
					Properties = fontProperties
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
					case "FONT_NAME":
						if (tokens.Count > 1)
							fontProperties.FontName = tokens[1];
						break;
					case "PIXEL_SIZE":
						fontProperties.PixelSize = Values1();
						break;
					case "FONT_ASCENT":
						fontProperties.Ascent = Values1();
						break;
					case "FONT_DESCENT":
						fontProperties.Descent = Values1();
						break;
					case "COMMENT":
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Save the font to a BDF file.
		/// </summary>
		/// <param name="font">The font to save</param>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		public static bool Save(GfxFont font, string fileName)
		{
			if (!FontProperties.EditIncomplete(ref font.Properties))
			{
				MessageBox.Show("Operation cancelled.", "Save as BDF");
				return false;
			}
			StreamWriter sw = new(fileName);
			sw.WriteLine("STARTFONT 2.1");
			sw.WriteLine($"FONTBOUNDINGBOX {(int)font.Glyphs.Average(g => g.xAdvance)} {font.yAdvance} 0 0");
			sw.WriteLine($"COMMENT \"Generated by GFX Font Editor\"");
			sw.WriteLine("STARTPROPERTIES 4");
			sw.WriteLine($"FONT_NAME {font.FontNameDefault}");
			sw.WriteLine($"PIXEL_SIZE {font.PixelSizeDefault}");  // fontforge asks for PIXEL_SIZE
			sw.WriteLine($"FONT_ASCENT {font.AscentDefault}");
			sw.WriteLine($"FONT_DESCENT {font.DescentDefault}");
			sw.WriteLine("ENDPROPERTIES");
			sw.WriteLine($"CHARS {font.Glyphs.Count}");
			foreach (var glyph in font.Glyphs)
			{
				sw.WriteLine($"STARTCHAR {glyph.Code}");
				sw.WriteLine($"ENCODING {glyph.Code}");
				sw.WriteLine($"DWIDTH {glyph.xAdvance} 0");
				var rc = glyph.Bounds;
				sw.WriteLine($"BBX {rc.Width} {rc.Height} {rc.X} {-rc.Y - rc.Height}");
				sw.WriteLine("BITMAP");
				for (int y = rc.Top;  y < rc.Bottom; y++)
				{
					string s = "";
					for (int x = rc.Left; x < rc.Right; )
					{
						int v = 0;
						for (int msk = 0x08; msk != 0; msk >>= 1)
						{
							if (x >= rc.Right)
								break;
							if (glyph.Get(x, y))
								v |= msk;
							x++;
						}
						s += $"{v:X1}";
					}
					if ((s.Length & 1) != 0)
						s += "0";
					sw.WriteLine(s);
				}
				sw.WriteLine("ENDCHAR");
			}
			sw.WriteLine("ENDFONT");
			sw.Close();
			return true;
		}
	}
}
