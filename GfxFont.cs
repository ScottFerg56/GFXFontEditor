using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GFXFontEditor
{
	/// <summary>
	/// A class representing the font data represented by the GFXfont structure
	/// in the Adafruit GFX Library.
	/// </summary>
	public class GfxFont
    {
		/*
			https://github.com/adafruit/Adafruit-GFX-Library/blob/master/gfxfont.h

			typedef struct { // Data stored for FONT AS A WHOLE:
				uint8_t  *bitmap;      // Glyph bitmaps, concatenated
				GFXglyph *glyph;       // Glyph array
				uint8_t   first, last; // ASCII extents
				uint8_t   yAdvance;    // Newline distance (y axis)
			} GFXfont;
		*/

		/// <summary>
		/// Full path name to the file from which the font was loaded.
		/// </summary>
		public string FullPathName;

        protected ushort _FirstCode;
		/// <summary>
		/// The character code for the font's first glyph.
		/// </summary>
		/// <remarks>
		/// Each Glyph.Code is based on its position in the font's list relative to the FirstCode.
		/// The font must maintain these values as Glyphs are added or removed or FirstCode changes.
		/// </remarks>
        public ushort FirstCode
		{
			get => _FirstCode;
			set
			{
				if (_FirstCode == value)
					return;
				_FirstCode = value;
				ResetCodes();
			}
		}
        
		/// <summary>
		/// The number of y pixels advanced from the start of one line of charaters to the next.
		/// </summary>
		public int yAdvance;
        
        protected List<Glyph> _Glyphs = new();
		/// <summary>
		/// The font's list of Glyphs
		/// </summary>
        public ReadOnlyCollection<Glyph> Glyphs => _Glyphs.AsReadOnly();

		/// <summary>
		/// Gets the rectangular bounds that tightly contains the Bounds of all the Glyphs.
		/// </summary>
		public Rectangle BmpBounds
		{
			get
			{
				var bounds = Rectangle.Empty;
				foreach (var g in _Glyphs)
					bounds = Rectangle.Union(bounds, g.Bounds);
				return bounds;
			}
		}

		/// <summary>
		/// The maximum xAdvance of all the Glyphs.
		/// </summary>
		public int MaxAdvance => _Glyphs.Any() ? _Glyphs.Max(x => x.xAdvance) : 0;

		/// <summary>
		/// The font character cell, with the full yAdvance and maximum xAdvance,
		/// expanded by the bounds of all Glyphs, to include any that extend beyond the cell.
		/// </summary>
		public Rectangle FullBounds => Rectangle.Union(BmpBounds, new Rectangle(0, -yAdvance, MaxAdvance, yAdvance));

		/// <summary>
		/// Add a glyph to the end of the font.
		/// </summary>
		/// <param name="glyph">The glyph to add</param>
		public void Add(Glyph glyph)
		{
			glyph.Code = (ushort)(FirstCode + _Glyphs.Count);
			_Glyphs.Add(glyph);
		}

		/// <summary>
		/// Remove a glyph from the font.
		/// </summary>
		/// <param name="glyph">The glyph to remove</param>
		/// <remarks>
		/// Removing a glyph causes all of the glyph character codes to be
		/// reset from the font's FirstCode.
		/// An exception is made when removing the first glyph, where the
		/// font's FirstCode is set to the code of the next glyph in the list,
		/// if such exists.
		/// </remarks>
		public void Remove(Glyph glyph)
		{
			if (glyph == _Glyphs.FirstOrDefault())
			{
				_Glyphs.Remove(glyph);
				var next = _Glyphs.FirstOrDefault();
				if (next != null)
					_FirstCode = next.Code;
			}
			else if (glyph == _Glyphs.LastOrDefault())
			{
				// opyimize with no need to reset codes
				_Glyphs.Remove(glyph);
			}
			else
			{
				_Glyphs.Remove(glyph);
				ResetCodes();
			}
		}

		/// <summary>
		/// Add a collection of glyphs to the font.
		/// </summary>
		/// <param name="glyphs">The glyphs to add</param>
		private void AddGlyphs(IEnumerable<Glyph> glyphs)
		{
			_Glyphs.AddRange(glyphs);
			ResetCodes();
		}

		/// <summary>
		/// Insert a glyph into the list of glyphs.
		/// </summary>
		/// <param name="inx">The location to insert the glyph.</param>
		/// <param name="glyph"></param>
		/// <remarks>
		/// Inserting a glyph causes all of the glyph character codes to be
		/// reset from the font's FirstCode.
		/// An exception is made when inserting before the first glyph, where the
		/// font's FirstCode reduced by one and assigned to the new first glyph.
		/// </remarks>
		public void InsertAt(int inx, Glyph glyph)
		{
			_Glyphs.Insert(inx, glyph);
			if (inx == 0 && FirstCode > 0 && _Glyphs.Any())
				glyph.Code = --_FirstCode;
			else
				ResetCodes();
		}

		/// <summary>
		/// Reset each Glyph.Code based on the FirstCode.
		/// </summary>
		public void ResetCodes()
		{
			var c = FirstCode;
			foreach (var g in _Glyphs)
				g.Code = c++;
		}

		/// <summary>
		/// Build a display bitmap visual representation of a text string, or the entire Glyph set.
		/// </summary>
		/// <param name="text">The text to display, or null/blank to display the entire Glyph set</param>
		/// <param name="pixelsPerDot">Pixels to print for each glyph 'dot'; a zoom factor</param>
		/// <param name="backColor">The background color</param>
		/// <param name="textColor">The color for glyph printing</param>
		/// <param name="bounds">Returns the left and right extent along the text line for each printed Glyph</param>
		/// <returns>The bitmap of the printed Glyphs</returns>
		public Bitmap ToBitmap(string text, int pixelsPerDot, Color backColor, Color textColor, out List<(int left, int right)> bounds)
		{
			IEnumerable<Glyph> glyphs;
			if (string.IsNullOrWhiteSpace(text))
				glyphs = _Glyphs;
			else
				glyphs = text.Select(c => _Glyphs.FirstOrDefault(gl => gl.Code == c)).ToList();
			return ToBitmap(glyphs, pixelsPerDot, backColor, textColor, out bounds);
		}

		/// <summary>
		/// Build a display bitmap visual representation of a set of Glyphs.
		/// </summary>
		/// <param name="glyphs">The Glyphs to print</param>
		/// <param name="pixelsPerDot">Pixels to print for each glyph 'dot'; a zoom factor</param>
		/// <param name="backColor">The background color</param>
		/// <param name="textColor">The color for glyph printing</param>
		/// <param name="bounds">Returns the left and right extent along the text line for each printed Glyph</param>
		/// <returns>The bitmap of the printed Glyphs</returns>
		public Bitmap ToBitmap(IEnumerable<Glyph> glyphs, int pixelsPerDot, Color backColor, Color textColor, out List<(int left, int right)> bounds)
		{
			bounds = new();
			if (!_Glyphs.Any())
				return null;
			var rcBounds = FullBounds;
			var top = Math.Min(-yAdvance, rcBounds.Top);
			var bottom = Math.Max(0, rcBounds.Bottom);
			int width = glyphs.Sum(g => g is null ? 1 : g.xAdvance) * pixelsPerDot;
			int height = rcBounds.Height * pixelsPerDot;
			if (width * height == 0)
				return null;
			var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(backColor);
				int x = 0;
				int y = Math.Max(-rcBounds.Top, yAdvance) * pixelsPerDot;
				using var pen = new Pen(Color.Blue, 1);
				using var penRed = new Pen(Color.Red, pixelsPerDot);
				// draw baseline and top line in blue
				g.DrawLine(pen, 0, y - pixelsPerDot / 2, width, y - pixelsPerDot / 2);
				g.DrawLine(pen, 0, y - yAdvance * pixelsPerDot + pixelsPerDot / 2, width, y - yAdvance * pixelsPerDot + pixelsPerDot / 2);
				foreach (var glyph in glyphs)
				{
					var left = x;
					if (glyph is null)
					{
						// a red mark represents any null Glyphs (text characters not found in the text to be displayed)
						g.DrawLine(penRed, x + pixelsPerDot / 2, 0, x + pixelsPerDot / 2, height);
						x += pixelsPerDot;
					}
					else
					{
						// print the Glyph
						glyph.Print(g, pixelsPerDot, textColor, ref x, y);
					}
					// add the left & right bounds for this Glyph
					bounds.Add((left, x - 1));
				}
			}
			return bmp;
		}

		/// <summary>
		/// Get a collection of extensions and their titles for valid file extensions
		/// for loading fonts.
		/// </summary>
		/// <returns>
		/// For each supported file format, the extension (with '.') and a title
		/// string suitable for use in file dialog boxes.
		/// </returns>
		public static IEnumerable<(string ext, string title)> GetLoadFileExtensions()
		{
			return new (string ext, string title)[]
			{
				(".h", "Font H"),
				(".yaff", "YAFF"),
				(".gfxfntx", "XML Font"),
				(".gfxfntb", "Binary Font"),
			};
		}

		/// <summary>
		/// Get a collection of extensions and their titles for valid file extensions
		/// for saving fonts.
		/// </summary>
		/// <returns>
		/// For each supported file format, the extension (with '.') and a title
		/// string suitable for use in file dialog boxes.
		/// </returns>
		public static IEnumerable<(string ext, string title)> GetSaveFileExtensions()
		{
			return new (string ext, string title)[]
			{
				(".h", "Font H"),
				(".gfxfntx", "XML Font"),
				(".gfxfntb", "Binary Font")
			};
		}

		/// <summary>
		/// Load an Adafruit Header (.h) font file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont LoadHFile(string fileName)
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
					if (inComment)	// in block comment
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
					MessageBox.Show($"Incomplete GFXGlyph strtucture");
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
				font._Glyphs.Add(glyph);
			}

			font.ResetCodes();
			file.Close();
			return font;
		}

		/// <summary>
		/// Load a YAFF font file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont LoadYaffFile(string fileName)
		{
			List<Glyph> glyphs = new();
			using var file = File.OpenText(fileName);
			bool keepLine = false;
			string line = "";
			int lineNumber = 0;
			bool seenGlyphs = false;
			bool inMap = false;
			List<string> mapLines = null;
			int mapHeight = -1;
			byte[] data = null;
			ushort cCode = 0xFFFF;	// char code (decimal or hex)
			ushort aCode = 0xFFFF;	// ascii code ('x')
			ushort uCode = 0xFFFF;  // u code (unicode)
			ushort xCode = 0x8000;	// code not found, increment
			Dictionary<string, string> fontDict = new();
			Dictionary<string, string> glyphDict = null;

			void Trace(string msg)
			{
				Debug.WriteLine($"[{fileName}][{lineNumber}] {msg}");
			}

			void TraceAssert(bool cond, string msg)
			{
				if (!cond)
					Trace(msg);
			}

			void OpenMap()
			{
				if (inMap)
					return;
				mapLines = new();
				inMap = true;
			}

			void CloseMap()
			{
				inMap = false;
				if (mapLines is null || mapLines.Count == 0)
				{
					data = null;
					return;
				}
				if (mapLines.Count == 1 && mapLines[0] == "-")
				{
					data = Array.Empty<byte>();
					return;
				}
				var width = mapLines.First().Length;
				var height = mapLines.Count;
				if (mapHeight == -1)
				{
					mapHeight = height;
				}
				else
				{
					TraceAssert(height == mapHeight, "map height mismatch for glyph");
				}
				data = new byte[(int)Math.Ceiling((width * height) / 8.0)];
				int inx = 0;
				byte mask = 0x80;
				foreach (var l in mapLines)
				{
					TraceAssert(l.Length == width, $"map width mismatch for glyph");
					for (int x = 0; x < width; x++)
					{
						if (x < l.Length && l[x] == '@')
							data[inx] |= mask;
						mask >>= 1;
						if (mask == 0)
						{
							inx++;
							mask = 0x80;
						}
					}
				}
			}

			void FinishGlyph()
			{
				CloseMap();
				if (data is null)
					return;
				if (glyphDict is not null)
				{
					foreach (var item in glyphDict)
					{
						Trace($"glyph property: {item.Key} = {item.Value}");
					}
				}
				glyphDict = null;
				ushort code = 0;
				if (cCode != 0xFFFF)
				{
					code = cCode;
				}
				else if (aCode != 0xFFFF)
				{
					Trace("using ascii glyph code label");
					code = aCode;
				}
				else if (uCode != 0xFFFF)
				{
					Trace("using unicode glyph code label");
					code = uCode;
				}
				else
				{ 
					Trace("no glyph code label");
					code = xCode++;
				}
				cCode = 0xFFFF;
				aCode = 0xFFFF;
				uCode = 0xFFFF;
				Glyph glyph;
				if (data.Length == 0)
					glyph = new(null, 0, 0, 0, 0, 0);
				else
					glyph = new Glyph(data.ToArray(), mapLines.First().Length, mapLines.Count, 0, 0, mapLines.First().Length);
				mapLines = null;
				glyph.Code = code;
				glyphs.Add(glyph);
			}

			while (true)
			{
				if (!keepLine)
				{
					line = file.ReadLine();
					lineNumber++;
				}
				keepLine = false;
				if (line == null)
					break;
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
					continue;
				if (line.Contains(':'))
				{
					// property
					if (line.StartsWith("':':"))
					{
						// glyph label of ':' confuses the key-value split!
						seenGlyphs = true;
						FinishGlyph();
						aCode = ':';
						continue;
					}
					string key = "";
					string value = "";
					var propParts = line.Split(':');
					if (propParts.Length > 0)
					{
						key = propParts[0].TrimEnd().ToLower();
						if (string.IsNullOrWhiteSpace(key))
							continue;
						if (propParts.Length > 1)
							value = propParts[1].Trim();
					}
					if (key.Contains(','))
					{
						seenGlyphs = true;
						FinishGlyph();
						Trace($"glyph label contains comma: [{key}]");
						continue;
					}
					if (key.StartsWith("u+"))
					{
						seenGlyphs = true;
						FinishGlyph();
						uCode = (ushort)int.Parse(key[2..], NumberStyles.HexNumber);
					}
					else if (key.StartsWith("0x"))
					{
						seenGlyphs = true;
						FinishGlyph();
						cCode = (ushort)int.Parse(key[2..], NumberStyles.HexNumber);
					}
					else if (char.IsDigit(key[0]))
					{
						seenGlyphs = true;
						FinishGlyph();
						cCode = (ushort)int.Parse(key);
					}
					else if (key.StartsWith("0o"))
					{
						seenGlyphs = true;
						FinishGlyph();
						cCode = (ushort)Convert.ToInt16(key[2..], 8);
						Trace($"glyph label in octal: [{key}] -> {cCode}");
					}
					else if (key[0] == '\'')
					{
						seenGlyphs = true;
						FinishGlyph();
						aCode = key[1];
					}
					else if (string.IsNullOrWhiteSpace(key))
					{
						Trace("Multiline property!");
						while (true)
						{
							line = file.ReadLine();
							lineNumber++;
							if (line.Contains(':'))
							{
								keepLine = true;
								break;
							}
							value += line.Trim();
						}
					}
					if (!seenGlyphs)
					{
						// glyph property
						fontDict.Add(key, value);
					}
					else
					{
						// glyph property or label
						if (!string.IsNullOrWhiteSpace(value))
						{
							// glyph property
							glyphDict ??= new();
							glyphDict.Add(key, value);
						}
						else
						{
							// glyph label
						}
					}
				}
				else
				{
					// not property or label
					if (line.Trim() == "-")
					{
						TraceAssert(!inMap, "encountered blank map indicator inside map");
						OpenMap();
						mapLines.Add("-");
					}
					else if (line.Contains('.') || line.Contains('@'))
					{
						OpenMap();
						mapLines.Add(line.Trim());
					}
				}
			}
			FinishGlyph();
			//if (fontDict is not null)
			//{
			//	foreach (var item in fontDict)
			//	{
			//		Trace($"font property: {item.Key} = {item.Value}");
			//	}
			//}
			int yAdvance = 0;
			if (fontDict.TryGetValue("shift-up", out string s))
			{
				int shiftUp;
				shiftUp = int.Parse(s);
				yAdvance = mapHeight + shiftUp;
			}
			if (yAdvance == 0)
				yAdvance = mapHeight;
			glyphs.Sort((a,b) => a.Code.CompareTo(b.Code));
			GfxFont font = new()
			{
				FirstCode = glyphs[0].Code,
				yAdvance = yAdvance
			};
			ushort code = glyphs[0].Code;
			List<int> missingCodes = new();
			int missCnt = (glyphs.Last().Code - code + 1) - glyphs.Count;
			foreach (var glyph in glyphs)
			{
				glyph.Offset(0, -yAdvance);
				if (glyph.Code != code)
				{
					while (glyph.Code != code)
					{
						missingCodes.Add(glyphs.IndexOf(glyph));
						code++;
					}
				}
				code++;
			}
			TraceAssert(missingCodes.Count == missCnt, "missing glyph count mismatch");
			font.AddGlyphs(glyphs);
			int maxAdv = font.MaxAdvance;
			missingCodes.Reverse();
			foreach (var inx in missingCodes)
			{
				var glyph = new Glyph(null, 0, 0, 0, 0, maxAdv);
				glyph.SetRect(maxAdv, yAdvance);
				font.InsertAt(inx, glyph);
			}
			return font;
		}

		/// <summary>
		/// Load a font from it's XML representation.
		/// </summary>
		/// <param name="node">The XElement node representing the font</param>
		/// <returns>A font</returns>
		public static GfxFont FromXml(XElement node)
		{
			var font = new GfxFont() { yAdvance = int.Parse(node.Element("yAdvance").Value) };
			var glyphs = Glyph.GlyphsFromXml(node.Element("glyphs"));
			font.FirstCode = glyphs.First().Code;
			font.AddGlyphs(glyphs);
			return font;
		}

		/// <summary>
		/// Load a font from an XML file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont LoadXmlFile(string fileName)
		{
			return FromXml(XElement.Load(fileName));
		}

		const Int32 GFXFontSize = 13;       // (assuming 32-bit pointers)

		/// <summary>
		/// Load a font from a binary file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		/// <remarks>
		/// The binary file contains an in-memory image of the C language
		/// GFXfont structure, bitmap byte array and GFXglyph array
		/// representing what would be in memory when a font defined in
		/// a header file is compiled.
		/// </remarks>
		public static GfxFont LoadBinaryFile(string fileName)
		{
			using BinaryReader reader = new(File.Open(fileName, FileMode.Open));

			// read the GFXfont structure datta
			var bitmapOffset = reader.ReadInt32();      //   uint8_t* bitmap;
			var glyphOffset = reader.ReadInt32();		//   GFXglyph* glyph;
			var first = reader.ReadUInt16();			//   uint16_t first;
			var last = reader.ReadUInt16();				//   uint16_t last;
			var yAdvance = reader.ReadByte();			//   uint8_t yAdvance;

			GfxFont font = new()
			{
				FirstCode = first,
				yAdvance = yAdvance,
			};

			// read the bitmap byte array
			Debug.Assert(bitmapOffset == GFXFontSize);
			var bitmap = reader.ReadBytes(glyphOffset - bitmapOffset);

			// read all the GFXglyph structure data
			List<(UInt16 offset, byte Width, byte Height, byte xAdvance, sbyte xOffset, sbyte yOffset)> gliphs = new();
			for (int i = first; i <= last; i++)
			{
				gliphs.Add(
				(
				reader.ReadUInt16(),	// uint16_t bitmapOffset;
				reader.ReadByte  (),    // uint8_t width;
				reader.ReadByte  (),    // uint8_t height;
				reader.ReadByte  (),    // uint8_t xAdvance;
				reader.ReadSByte (),    // int8_t xOffset;
				reader.ReadSByte ()     // int8_t yOffset;
				));
				reader.ReadByte();		// extra 8-byte alignment byte 
			}

			// build our Glyphs and the font from the loaded data
			for (int i = 0; i < gliphs.Count; i++)
			{
				var (offset, Width, Height, xAdvance, xOffset, yOffset) = gliphs[i];
				int end = i == gliphs.Count - 1 ? bitmap.Length : gliphs[i+1].offset;
				var glyph = new Glyph(bitmap[offset..end], Width, Height, xOffset, yOffset, xAdvance);
				font.Add(glyph);
			}
			return font;
		}

		/// <summary>
		/// Load a font file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont LoadFile(string fileName)
		{
			try
			{
				GfxFont font = null;
				var ext = Path.GetExtension(fileName);
				switch (ext.ToLower())
				{
					case ".h":
						font = LoadHFile(fileName);
						break;
					case ".yaff":
						font = LoadYaffFile(fileName);
						break;
					case ".gfxfntx":
						font = LoadXmlFile(fileName);
						break;
					case ".gfxfntb":
						font = LoadBinaryFile(fileName);
						break;
					default:
						MessageBox.Show("Unsupported file extension for open");
						return null;
				}
				font.FullPathName = fileName;
				return font;
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, "Error loading file");
				return null;
			}
		}

		/// <summary>
		/// Save the font to a C header (.h) file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		bool SaveHFile(string fileName)
		{
			string fontName = Path.GetFileNameWithoutExtension(fileName);
			StringBuilder sb = new();
			// write bitmap data array bytes
            sb.AppendLine($"const uint8_t {fontName}Bitmaps[] PROGMEM = " + "{");
            foreach (var gg in _Glyphs)
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
			foreach (var item in _Glyphs)
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
            sb.AppendLine($"0x{FirstCode:X2}, 0x{FirstCode + _Glyphs.Count - 1:X2}, {yAdvance} ");
            sb.AppendLine("};");

            File.WriteAllText(fileName, sb.ToString());
			return true;
        }

		/// <summary>
		/// Create an XML representation of the font.
		/// </summary>
		/// <returns>An XElement node</returns>
		public XElement ToXml()
		{
			var node = new XElement("GfxFont");
			node.Add(new XElement("yAdvance", $"{yAdvance}"));
			node.Add(Glyph.GlyphsToXml(_Glyphs));
			return node;
		}

		/// <summary>
		/// Create an XML string representation of the font.
		/// </summary>
		/// <returns>The XML string</returns>
		public string ToXmlString()
		{
			return ToXml().ToString(); // SaveOptions.DisableFormatting);
		}

		/// <summary>
		/// Save the font to an XML file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		bool SaveXmlFile(string fileName)
		{
			File.WriteAllText(fileName, ToXmlString());
			return true;
		}

		/// <summary>
		/// Save the font to a binary file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		/// <remarks>
		/// The binary file contains an in-memory image of the C language
		/// GFXfont structure, bitmap byte array and GFXglyph array
		/// representing what would be in memory when a font defined in
		/// a header file is compiled.
		/// </remarks>
		bool SaveBinaryFile(string fileName)
		{
			List<byte> bitmap = new(_Glyphs.SelectMany(g => g.GetData()));
			BinaryWriter writer = new(File.Open(fileName, FileMode.Create));

																	// typedef struct {
			writer.Write((Int32)GFXFontSize);						//   uint8_t* bitmap;
			writer.Write((Int32)GFXFontSize + bitmap.Count);		//   GFXglyph* glyph;
			writer.Write((ushort)FirstCode);						//   uint16_t first;
			writer.Write((ushort)(FirstCode + _Glyphs.Count - 1));	//   uint16_t last;
			writer.Write((byte)yAdvance);                           //   uint8_t yAdvance;
																	// } GFXfont;

			writer.Write(bitmap.ToArray());                         // const uint8_t <>Bitmaps[] PROGMEM = { };

			int offset = 0;
			foreach (var glyph in _Glyphs)
			{
																	// typedef struct {
				writer.Write((UInt16)offset);						//   uint16_t bitmapOffset;
				writer.Write((byte)glyph.Width);					//   uint8_t width;
				writer.Write((byte)glyph.Height);					//   uint8_t height;
				writer.Write((byte)glyph.xAdvance);					//   uint8_t xAdvance;
				writer.Write((sbyte)glyph.xOffset);					//   int8_t xOffset;
				writer.Write((sbyte)glyph.yOffset);                 //   int8_t yOffset;
																	// } GFXglyph;
				writer.Write((byte)0);                              // alignment to 8-byte records
				offset += glyph.GetData().Count;
			}
			writer.Close();
			return true;
		}

		/// <summary>
		/// Save the font to a file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		public bool SaveFile(string fileName)
		{
			try
			{
				var ext = Path.GetExtension(fileName);
				switch (ext.ToLower())
				{
					case ".h":
						return SaveHFile(fileName);
					case ".gfxfntx":
						return SaveXmlFile(fileName);
					case ".gfxfntb":
						return SaveBinaryFile(fileName);
					default:
						MessageBox.Show("Unsupported file extension for save", "Error saving file");
						return false;
				}

			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, "Error saving file");
				return false;
			}
		}
	}
}
