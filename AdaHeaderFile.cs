using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GFXFontEditor
{
	public class AdaHeaderFile
	{
		public static IEnumerable<(string ext, string title)> GetExtensions()
		{
			yield return (".h", "Font H");
		}

		/// <summary>
		/// Load an Adafruit Header (.h) font file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont Load(string fileName)
		{
			//
			// The possible range of C language syntax in a header file representing the construction of a valid
			// GFXfont is far too complex to parse here.
			// Parsing done is sufficient to handle the representative samples that are available at the time.
			//

			// the basic strategy is to read a line at a time and parse it into tokens as the typical 'words' and symbols found
			using var file = File.OpenText(fileName);
			var tokens = new List<string>();
			// set up a Regex for...
			//		words-or-signed-numbers: -?\w+
			//		OR block-comment-start:	 /*
			//		OR block-comment-end:	 */
			//		OR eol-comment:			 //
			//		OR non-word-single-chars [\W_-[\s]]
			//		OR whitespace			 \s*
			// as tokens, we're only interested in words-or-signed-numbers OR non-word-single-chars and filter out the others
			var regx = new Regex(@"-?\w+|/\*|\*/|//|[\W_-[\s]]|\s*");
			bool inComment = false;
			string line = null;
			int lineNumber = 0;

			// parse a token from the file
			string Parse()
			{
				while (true)
				{
					while (!tokens.Any())
					{
						// need more tokens
						line = file.ReadLine().Trim();
						lineNumber++;
						if (line == null)
						{
							MessageBox.Show($"Unexpected end of file");
							return null;
						}
						// ignore empty lines or those with #pragma, #include, etc.
						if (string.IsNullOrWhiteSpace(line) || line[0] == '#')
							continue;
						// generate tokens from the line
						var m = regx.Matches(line);
						// filter out the whitespace
						tokens.AddRange(m.Select(x => x.Value).Where(s => !string.IsNullOrWhiteSpace(s)));
					}
					// get a token
					var tok = tokens.First();
					tokens.RemoveAt(0);
					// filter out the comments
					if (tok == "//")
					{
						// end-of-line comment - clear tokens for this line
						tokens.Clear();
						continue;
					}
					if (tok == "/*")
					{
						// block comment start
						inComment = true;
						continue;
					}
					if (tok == "*/" && inComment)
					{
						// block comment end
						inComment = false;
						continue;
					}
					if (inComment)  // in block comment
						continue;
					return tok;
				}
			}

			// put a token back
			void RestoreToken(string tok)
			{
				tokens.Insert(0, tok);
			}

			// require a given string as the next token
			bool Accept(string tgt)
			{
				while (true)
				{
					string tok = Parse();
					if (tok == null)
						return false;
					if (tok == tgt)
						return true;
					if (MessageBox.Show($"Expected '{tgt}', found '{tok}'.\r\n\r\n[{lineNumber}] {line}\r\n\r\nContinue?", "Load Font", MessageBoxButtons.YesNo) == DialogResult.No)
						return false;
				}
			}

			// require a sequence of strings as the next tokens
			bool AcceptAll(params string[] tgts)
			{
				foreach (var tgt in tgts)
				{
					if (!Accept(tgt))
						return false;
				}
				return true;
			}

			// return true if the next token matches the expected string
			// does not consume the token
			bool Peek(string tgt)
			{
				var tok = Parse();
				var ret = tok == tgt;
				RestoreToken(tok);
				return ret;
			}

			// return true if the next token matches the expected string
			// consume the token only if it matches
			bool Optional(string tgt)
			{
				var tok = Parse();
				if (tok == tgt)
					return true;
				RestoreToken(tok);
				return false;
			}

			// parse the next token as a number
			bool Number(out int val)
			{
				val = 0;
				// allow the user to ignore reported errors and keep trying
				while (true)
				{
					var tok = Parse();
					if (tok == null)
						return false;
					var ns = NumberStyles.Integer;
					if (tok.StartsWith("0x"))
					{
						// hex number
						tok = tok[2..];
						ns = NumberStyles.HexNumber;
					}
					if (int.TryParse(tok, ns, NumberFormatInfo.CurrentInfo, out val))
						return true;
					if (MessageBox.Show($"Expected number, found {tok}. Continue?", "Load Font", MessageBoxButtons.YesNo) == DialogResult.No)
						return false;
				}
			}

			// parse a comma-separated number list
			bool NumberList(out List<int> vals)
			{
				vals = new();
				// allow the user to ignore reported errors and keep trying
				while (true)
				{
					// end of list?
					if (Peek("}"))
						return true;
					if (!Number(out int val))
						return false;
					vals.Add(val);
					// technically, we should see a comma between numbers, but...
					// we won't care if it works!
					// exit if end of list
					if (!Optional(","))
						return true;
				}
			}

			// parse the initialization for a GFXglyph structure, e.g.
			// {0, 3, 11, 11, 4, -10}
			bool ParseGlyph(out List<int> glyph)
			{
				glyph = new();
				if (!Accept("{"))
					return false;
				if (!NumberList(out glyph))
					return false;
				return Accept("}");
			}

			// parse a list of initializations for GFXglyph structures
			bool GlyphList(out List<List<int>> glyphs)
			{
				glyphs = new();
				while (true)
				{
					// end of list?
					if (Peek("}"))
						return true;
					if (!ParseGlyph(out List<int> glyph))
						return false;
					glyphs.Add(glyph);
					// technically, we should see a comma between glyphs, but...
					// we won't care if it works!
					// exit if end of list
					if (!Optional(","))
						return true;
				}
			}

			//
			// parsing glyph bitmap array:
			// const uint8_t <>Bitmaps[] PROGMEM = { <comma-separated-numbers> };
			//

			if (!AcceptAll("const", "uint8_t"))
				return null;

			var bmpArrayName = Parse();
			if (bmpArrayName == null)
				return null;

			if (!AcceptAll("[", "]"))
				return null;

			Optional("PROGMEM");

			if (!AcceptAll("=", "{"))
				return null;

			if (!NumberList(out List<int> bmpValues))
				return null;

			if (!AcceptAll("}", ";"))
				return null;

			//
			// parsing glyph array:
			// const GFXglyph <>Glyphs[] PROGMEM = { <comma-separated-glyphs> };
			// where glyph is:
			// { <number>, <number>, <number>, <number>, <number>, <number> }
			//

			if (!AcceptAll("const", "GFXglyph"))
				return null;

			var glyphArrayName = Parse();
			if (glyphArrayName == null)
				return null;

			if (!AcceptAll("[", "]"))
				return null;

			Optional("PROGMEM");

			if (!AcceptAll("=", "{"))
				return null;

			if (!GlyphList(out List<List<int>> glyphs))
				return null;

			if (!AcceptAll("}", ";"))
				return null;

			//
			// parsing font structure:
			// const GFXfont <> PROGMEM = {
			// (uint8_t*)<>Bitmaps,
			// (GFXglyph*)<>Glyphs,
			// <number>, <number>, <number>};
			//

			if (!AcceptAll("const", "GFXfont"))
				return null;

			var fontName = Parse();
			if (fontName == null)
				return null;

			Optional("PROGMEM");

			if (!AcceptAll("=", "{"))
				return null;

			// this cast always seems to be there, but may actually not be required?
			if (Optional("("))
			{
				if (!AcceptAll("uint8_t", "*", ")"))
					return null;
			}

			// note: we don't really care if this name matches, but...
			if (!Accept(bmpArrayName) || !Accept(","))
				return null;

			// this cast always seems to be there, but may actually not be required?
			if (Optional("("))
			{
				if (!AcceptAll("GFXglyph", "*", ")"))
					return null;
			}

			// note: we don't really care if this name matches, but...
			if (!AcceptAll(glyphArrayName, ","))
				return null;

			if (!NumberList(out List<int> fontVals))
				return null;

			// I guess we could actually pass on this too!
			if (!AcceptAll("}", ";"))
				return null;

			if (fontVals.Count < 3)
			{
				MessageBox.Show($"Incomplete GFXFont strtucture");
				return null;
			}

			// build our font structure from the harvested glyph and font data
			GfxFont font = new()
			{
				FirstCode = (ushort)fontVals[0],
				yAdvance = fontVals[2],
			};

			for (int i = 0; i < glyphs.Count; i++)
			{
				var gl = glyphs[i];
				if (gl.Count < 6)
				{
					MessageBox.Show($"Incomplete GFXGlyph structure");
					return null;
				}
				var offset1 = gl[0];
				byte[] data = null;
				if (gl != glyphs.Last())
				{
					var offset2 = glyphs[i + 1][0];
					data = bmpValues.Select(v => (byte)v).Skip(offset1).Take(offset2 - offset1).ToArray();
				}
				else
				{
					data = bmpValues.Select(v => (byte)v).Skip(offset1).ToArray();
				}

				var glyph = new Glyph(data, gl[1], gl[2], gl[4], gl[5], gl[3]);
				font.Add(glyph);
			}

			font.ResetCodes();
			file.Close();
			return font;
		}

		/// <summary>
		/// Save the font to a C header (.h) file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		public static bool Save(GfxFont font, string fileName)
		{
			string fontName = Path.GetFileNameWithoutExtension(fileName);
			StringBuilder sb = new();
			// write bitmap data array bytes
			sb.AppendLine($"const uint8_t {fontName}Bitmaps[] PROGMEM = " + "{");
			foreach (var gg in font.Glyphs)
			{
				sb.Append($"/* '{(char)gg.Code}' 0x{gg.Code:X2} */ ");
				foreach (var bb in gg.GetData())
				{
					sb.Append($"0x{bb:X2}, ");
				}
				sb.AppendLine();
			}
			sb.AppendLine("};");
			sb.AppendLine();

			// write GFXglyph array initialization
			sb.AppendLine($"const GFXglyph {fontName}Glyphs[] PROGMEM = " + "{");
			int offset = 0;
			foreach (var item in font.Glyphs)
			{
				sb.AppendLine($"/* '{(char)item.Code}' 0x{item.Code:X2} */ {{{offset,6}, {item.Width,4},{item.Height,4},{item.xAdvance,4},{item.xOffset,4},{item.yOffset,5} }},");
				offset += item.GetData().Count;
			}
			sb.AppendLine("};");
			sb.AppendLine();

			// write GFXfont struct initialization
			sb.AppendLine($"const GFXfont {fontName} PROGMEM = " + "{");
			sb.AppendLine($"(uint8_t*){fontName}Bitmaps,");
			sb.AppendLine($"(GFXglyph*){fontName}_Glyphs,");
			sb.AppendLine($"0x{font.FirstCode:X2}, 0x{font.FirstCode + font.Glyphs.Count - 1:X2}, {font.yAdvance} ");
			sb.AppendLine("};");

			File.WriteAllText(fileName, sb.ToString());
			return true;
		}
	}
}
