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
			/*
				**** The C structures defining the glyphs and font are:

				/// Font data stored PER GLYPH
				typedef struct {
				  uint16_t bitmapOffset; ///< Pointer into GFXfont->bitmap
				  uint8_t width;         ///< Bitmap dimensions in pixels
				  uint8_t height;        ///< Bitmap dimensions in pixels
				  uint8_t xAdvance;      ///< Distance to advance cursor (x axis)
				  int8_t xOffset;        ///< X dist from cursor pos to UL corner
				  int8_t yOffset;        ///< Y dist from cursor pos to UL corner
				} GFXglyph;

				/// Data stored for FONT AS A WHOLE
				typedef struct {
				  uint8_t *bitmap;  ///< Glyph bitmaps, concatenated
				  GFXglyph *glyph;  ///< Glyph array
				  uint16_t first;   ///< ASCII extents (first char)
				  uint16_t last;    ///< ASCII extents (last char)
				  uint8_t yAdvance; ///< Newline distance (y axis)
				} GFXfont;

				**** The typical C header defining a font looks like:

				const uint8_t font_name_Bitmaps[] PROGMEM = {
					0xFF, 0xFF...
					0xFF, 0xFF};

				const GFXglyph font_name_Glyphs[] PROGMEM = {
					{   0,  0, 0, 10, 0,   1},		// 0x20 ' '
					...
					{4491, 15, 6, 18, 1, -10}};		// 0x7E '~'

				const GFXfont font_name_ PROGMEM = {
					(uint8_t *)font_name_Bitmaps, (GFXglyph *)font_name_Glyphs,
					0x20, 0x7E, 42};
			*/

			//
			// The possible range of C language syntax in a header file representing the construction of a valid
			// GFXfont is far too complex to parse here.
			// Parsing done is sufficient to handle the representative samples that Adafruit has published at the time.
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

			// throw an error exception
			void Error(string message)
			{
				throw new Exception($"[{Path.GetFileName(fileName)}][line:{lineNumber}] {message}");
			}

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
							Error($"Unexpected end of file");
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
			void Accept(string tgt)
			{
				string tok = Parse();
				if (tok != tgt)
					Error($"Expected '{tgt}', found '{tok}'");
			}

			// require a sequence of strings as the next tokens
			void AcceptAll(params string[] tgts)
			{
				foreach (var tgt in tgts)
					Accept(tgt);
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
			void Number(out int val)
			{
				val = 0;
				// allow the user to ignore reported errors and keep trying
				var tok = Parse();
				var ns = NumberStyles.Integer;
				if (tok.StartsWith("0x"))
				{
					// hex number
					tok = tok[2..];
					ns = NumberStyles.HexNumber;
				}
				if (!int.TryParse(tok, ns, NumberFormatInfo.CurrentInfo, out val))
					Error($"Expected number, found {tok}");
			}

			// parse a comma-separated number list
			void NumberList(out List<int> vals)
			{
				vals = new();
				// allow the user to ignore reported errors and keep trying
				while (true)
				{
					// end of list?
					if (Peek("}"))
						return;
					Number(out int val);
					vals.Add(val);
					// technically, we should see a comma between numbers, but...
					if (!Optional(","))
						return;
				}
			}

			// parse the initialization for a GFXglyph structure, e.g.
			// {0, 3, 11, 11, 4, -10}
			void ParseGlyph(out List<int> glyph)
			{
				glyph = new();
				Accept("{");
				NumberList(out glyph);
				Accept("}");
			}

			// parse a list of initializations for GFXglyph structures
			void GlyphList(out List<List<int>> glyphs)
			{
				glyphs = new();
				while (true)
				{
					// end of list?
					if (Peek("}"))
						return;
					ParseGlyph(out List<int> glyph);
					glyphs.Add(glyph);
					// technically, we should see a comma between glyphs, but...
					// we won't care, if it works!
					// exit if end of list
					if (!Optional(","))
						return;
				}
			}

			//
			// parsing glyph bitmap array:
			// const uint8_t <>Bitmaps[] PROGMEM = { <comma-separated-numbers> };
			//

			AcceptAll("const", "uint8_t");

			var bmpArrayName = Parse();

			AcceptAll("[", "]");

			Optional("PROGMEM");

			AcceptAll("=", "{");

			NumberList(out List<int> bmpValues);

			AcceptAll("}", ";");

			//
			// parsing glyph array:
			// const GFXglyph <>Glyphs[] PROGMEM = { <comma-separated-glyphs> };
			// where glyph is:
			// { <number>, <number>, <number>, <number>, <number>, <number> }
			//

			AcceptAll("const", "GFXglyph");

			var glyphArrayName = Parse();

			AcceptAll("[", "]");

			Optional("PROGMEM");

			AcceptAll("=", "{");

			GlyphList(out List<List<int>> glyphs);

			AcceptAll("}", ";");

			//
			// parsing font structure:
			// const GFXfont <> PROGMEM = {
			// (uint8_t*)<>Bitmaps,
			// (GFXglyph*)<>Glyphs,
			// <number>, <number>, <number>};
			//

			AcceptAll("const", "GFXfont");

			var fontName = Parse();

			Optional("PROGMEM");

			AcceptAll("=", "{");

			// this cast always seems to be there, but may actually not be required?
			if (Optional("("))
				AcceptAll("uint8_t", "*", ")");

			// note: we don't really care if this name matches, but...
			AcceptAll(bmpArrayName, ",");

			// this cast always seems to be there, but may actually not be required?
			if (Optional("("))
				AcceptAll("GFXglyph", "*", ")");

			// note: we don't really care if this name matches, but...
			AcceptAll(glyphArrayName, ",");

			NumberList(out List<int> fontVals);

			// I guess we could actually pass on this too!
			AcceptAll("}", ";");

			if (fontVals.Count < 3)
				Error($"Incomplete GFXfont strtucture");

			// build our font structure from the harvested glyph and font data
			GfxFont font = new()
			{
				yAdvance = fontVals[2],
			};

			var firstCode = (ushort)fontVals[0];
			for (int i = 0; i < glyphs.Count; i++)
			{
				var gl = glyphs[i];
				if (gl.Count < 6)
					Error($"Incomplete GFXGlyph structure");
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

				var glyph = new Glyph(data, gl[1], gl[2], gl[4], gl[5], gl[3]) { Code = firstCode++ };
				font.Add(glyph);
			}

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
			string CodeToChar(ushort code) => (code < 0x20 || code >= 0x7F) ? "" : $"'{(char)code}' ";

			if (!GfxFont.CheckFlatness(font.Glyphs))
			{
				MessageBox.Show($"Font must be flattened before saving in header format!", "Header File Save");
				return false;
			}
			var fn = new StringBuilder(Path.GetFileNameWithoutExtension(fileName));
			for (int i = 0; i < fn.Length; i++)
			{
				var c = fn[i];
				if (char.IsLetter(c) || c == '_')
					continue;
				if (char.IsDigit(c) && i > 0)
					continue;
				fn[i] = '_';
			}
			var fontName = fn.ToString();
			StringBuilder sb = new();
			// write bitmap data array bytes
			sb.AppendLine($"const uint8_t {fontName}Bitmaps[] PROGMEM = " + "{");
			foreach (var gg in font.Glyphs)
			{
				sb.Append($"/* {CodeToChar(gg.Code)}0x{gg.Code:X2} */ ");
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
				sb.AppendLine($"/* {CodeToChar(item.Code)}0x{item.Code:X2} */ {{{offset,6}, {item.Width,4},{item.Height,4},{item.xAdvance,4},{item.xOffset,4},{item.yOffset,5} }},");
				offset += item.GetData().Count;
			}
			sb.AppendLine("};");
			sb.AppendLine();

			// write GFXfont struct initialization
			sb.AppendLine($"const GFXfont {fontName} PROGMEM = " + "{");
			sb.AppendLine($"(uint8_t*){fontName}Bitmaps,");
			sb.AppendLine($"(GFXglyph*){fontName}_Glyphs,");
			sb.AppendLine($"0x{font.StartCode:X2}, 0x{font.EndCode:X2}, {font.yAdvance} ");
			sb.AppendLine("};");

			File.WriteAllText(fileName, sb.ToString());
			return true;
		}
	}
}
