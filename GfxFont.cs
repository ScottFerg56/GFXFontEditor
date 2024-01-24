using System.Collections.ObjectModel;
using System.Diagnostics;
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

		/// <summary>
		/// The number of y pixels advanced from the start of one line of charaters to the next.
		/// </summary>
		public int yAdvance;

		public ushort StartCode => _Glyphs.FirstOrDefault()?.Code ?? 0;

		public ushort EndCode => _Glyphs.LastOrDefault()?.Code ?? 0;

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
				// Rectangle.Empty contains the point (0,0) which would show up in the union
				// so we need to seed from the first non-empty glyph bounds
				// before doing any union operation with other glyphs
				var bounds = Rectangle.Empty;
				foreach (var g in _Glyphs)
				{
					var rc = g.Bounds;
					if (rc.IsEmpty)
						continue;
                    if (bounds.IsEmpty)
						bounds = rc;
					else
						bounds = Rectangle.Union(bounds, g.Bounds);
				}
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
		/// Add a glyph to the font, maintaining the sort order by glyph Code.
		/// </summary>
		/// <param name="glyph">The glyph to add</param>
		public int Add(Glyph glyph)
		{
			if (glyph.Code >= EndCode)
			{
				// shortcut to the end
				_Glyphs.Add(glyph);
				return _Glyphs.Count - 1;
			}
			// find insertion point to keep the list sorted
			var next = _Glyphs.FirstOrDefault(g => g.Code > glyph.Code);
			if (next is null)
			{
				// shouldn't happen with the above shortcut!
				Debug.Fail("shouldn't happen");
				_Glyphs.Add(glyph);
				return _Glyphs.Count - 1;
			}
			var inx = _Glyphs.IndexOf(next);
			_Glyphs.Insert(inx, glyph);
			return inx;
		}

		/// <summary>
		/// Remove a glyph from the font.
		/// </summary>
		/// <param name="glyph">The glyph to remove</param>
		public void Remove(Glyph glyph)
		{
			_Glyphs.Remove(glyph);
		}

		/// <summary>
		/// Add a collection of glyphs to the font.
		/// </summary>
		/// <param name="glyphs">The glyphs to add</param>
		public void AddGlyphs(IEnumerable<Glyph> glyphs)
		{
			foreach (var glyph in glyphs)
				Add(glyph);
		}

		/// <summary>
		/// Truncate glyph list to a specified maximum.
		/// </summary>
		/// <param name="length">Maximum length to allow</param>
		public void Truncate(int length)
		{
			if (_Glyphs.Count > length)
				_Glyphs.RemoveRange(length, _Glyphs.Count - length);
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
			return
				AdaHeaderFile.GetExtensions()
				.Concat(BdfParser.GetExtensions())
				.Concat(GfxBinaryFile.GetExtensions())
				.Concat(GfxXmlFile.GetExtensions());
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
			return
				AdaHeaderFile.GetExtensions()
				.Concat(GfxBinaryFile.GetExtensions())
				.Concat(GfxXmlFile.GetExtensions());
		}

		/// <summary>
		/// Dummy fill Code gaps where possible in a fonts glyph list,
		/// and assign those with duplicate or missing Codes to free values.
		/// </summary>
		/// <param name="glyphsIn">The glyph list to fix</param>
		/// <returns>The flattened glyph list</returns>
		public static List<Glyph> FlattenGlyphList(IEnumerable<Glyph> glyphsIn)
		{
			if (!glyphsIn.Any())
				return new List<Glyph>();
			List<Glyph> glyphs = new();
			List<Glyph> glyphsFill = new();
			var groups = glyphsIn.GroupBy(g => g.Code);
			foreach (var group in groups)
			{
				// move ALL 'bad' codes to the 'fill' list
				int skip = group.Key == 0xFFFF ? 0 : 1;
				// and any duplicate codes
				foreach (var glyph in group.Skip(skip))
				{
					glyph.Status = Glyph.States.Error;
					glyphsFill.Add(glyph);
				}
			}
			ushort code = groups.First().Key;
			if (glyphsFill.Any())
			{
				// allow fill before the first glyph
				code = (ushort)Math.Max(0, code - glyphsFill.Count);
			}
			bool badGap = false;
			foreach (var group in groups)
			{
				if (group.Key == 0xFFFF)	// already processed
					break;
				var first = group.First();
				var gap = first.Code - code;
				if (!badGap && gap > 128 && gap > glyphsFill.Count)
				{
					badGap = true;
					Debug.WriteLine($"bad gap {gap} at {first.Code}");
				}
				if (badGap)
				{
					// we've encountered a 'too big' gap
					// just compress codes to the end
					first.Code = code++;
					first.Status = Glyph.States.Error;
					glyphs.Add(first);
				}
				else
				{
					// fill the gap
					for (ushort i = code; i < first.Code; i++)
					{
						Glyph fill;
						if (glyphsFill.Any())
						{
							// fill from the 'bad' list
							fill = glyphsFill.First();
							fill.Code = i;
							fill.Status = Glyph.States.Error;
							glyphsFill.RemoveAt(0);
						}
						else
						{
							// fill with a blank (some xAdvance just for presence in the bitmap font view)
							fill = new Glyph(null, 0, 0, 0, 0, 8) { Code = i, Status = Glyph.States.Inserted };
						}
						glyphs.Add(fill);
					}
					// add the good one
					glyphs.Add(first);
					code = (ushort)(first.Code + 1);
				}
			}
			// fill in the rest
			foreach (var glyph in glyphsFill)
			{
				glyph.Code = code++;
				glyph.Status = Glyph.States.Error;
				glyphs.Add(glyph);
			}
			return glyphs;
		}

		/// <summary>
		/// Check the glyph list for flatness,
		/// e.g. no duplicates, no gaps and no unspecified codes (== 0xFFFF).
		/// </summary>
		/// <param name="glyphsIn">The glyph list to check</param>
		/// <returns>True if the list is flat</returns>
		public static bool CheckFlatness(IEnumerable<Glyph> glyphsIn)
		{
			if (!glyphsIn.Any())
				return true;
			var groups = glyphsIn.GroupBy(g => g.Code);
			ushort code = groups.First().Key;
			foreach (var group in groups)
			{
				if (group.Key == 0xFFFF)
					return false;
				if (group.Count() > 1)
					return false;
				if (group.Key - code > 1)
					return false;
				code = group.Key;
			}
			return true;
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
			font.AddGlyphs(glyphs);
			return font;
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
						font = AdaHeaderFile.Load(fileName);
						break;
					case ".bdf":
						font = new BdfParser(fileName).Load();
						break;
					case ".gfxfntx":
						font = GfxXmlFile.Load(fileName);
						break;
					case ".gfxfntb":
						font = GfxBinaryFile.Load(fileName);
						break;
					default:
						MessageBox.Show("Unsupported file extension for open");
						return null;
				}
				if (font is null)
				{
					MessageBox.Show(fileName, "Load File Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		/// Save the font to a file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		public bool SaveFile(string fileName)
		{
			if (_Glyphs.Count == 0)
			{
				MessageBox.Show("Font is empty!", "Error saving file");
				return false;
			}
			try
			{
				var ext = Path.GetExtension(fileName);
				switch (ext.ToLower())
				{
					case ".h":
						return AdaHeaderFile.Save(this, fileName);
					case ".gfxfntx":
						return GfxXmlFile.Save(this, fileName);
					case ".gfxfntb":
						return GfxBinaryFile.Save(this, fileName);
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
