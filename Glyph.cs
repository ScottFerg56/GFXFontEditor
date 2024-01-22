using System.Diagnostics;
using System.Xml.Linq;

namespace GFXFontEditor
{
	/// <summary>
	/// A class representing the Glyph data represented by the GFXglyph structure
	/// in the Adafruit GFX Library.
	/// </summary>
	[DebuggerDisplay("Glyph Code: {Code}")]
	public class Glyph : SparseMap
	{
		/*
			https://github.com/adafruit/Adafruit-GFX-Library/blob/master/gfxfont.h

			typedef struct { // Data stored PER GLYPH
				uint16_t bitmapOffset;     // Pointer into GFXfont->bitmap
				uint8_t  width, height;    // Bitmap dimensions in pixels
				uint8_t  xAdvance;         // Distance to advance cursor (x axis)
				int8_t   xOffset, yOffset; // Dist from cursor pos to UL corner
			} GFXglyph;
		*/

		/// <summary>
		/// The number of pixels to be advanced to the right after the glyph is drawn.
		/// </summary>
		public int xAdvance;
		/// <summary>
		/// The width of the minimal bounds of the pixels drawn by the glyph.
		/// </summary>
        public int Width => Bounds.Width;
		/// <summary>
		/// The height of the minimal bounds of the pixels drawn by the glyph.
		/// </summary>
		public int Height => Bounds.Height;
		/// <summary>
		/// The offset in the x axis for drawing the character cell.
		/// </summary>
		public int xOffset => Bounds.X;
		/// <summary>
		/// The offset in the y axis for drawing the character cell.
		/// </summary>
		public int yOffset => Bounds.Y;

		public enum States { Normal, Inserted, Error};

		public States Status = States.Normal;

		/// <summary>
		/// Glyph constructor.
		/// </summary>
		/// <param name="data">Bitmap data representing just the bouds of the drawn pixels</param>
		/// <param name="width">Width of the bitmap data</param>
		/// <param name="height">Height of the bitmap data</param>
		/// <param name="xoffset">Offset in the x axis for drawing the character cell</param>
		/// <param name="yoffset">Offset in the y axis for drawing the character cell</param>
		/// <param name="xadvance">Number of pixels to be advanced to the right after the glyph is drawn</param>
		public Glyph(
		    byte[] data,
		    int width,
		    int height,
		    int xoffset,
		    int yoffset,
		    int xadvance
            ) : base(data, width, height)	// bitmap data handled by the SparseMap class
        {
			// it would be nice to check the data array we produce against the data coming in
			// unfortunately, many font files contain glyphs with extraneous blank rows at the beginning or end
			// and also glyphs (such as space) with width=0 and height=1, which makes no sense
            // fortunately that means this code produces more efficient font data!!

			// offset the data and SparseMap tracks the actual points to be set
			Offset(xoffset, yoffset);
            xAdvance = xadvance;
        }

		public Glyph(
			SparseMap map,
			int xoffset,
			int yoffset,
			int xadvance
			) : base(map.GetData(), map.Bounds.Width, map.Bounds.Height)   // bitmap data handled by the SparseMap class
		{
			// it would be nice to check the data array we produce against the data coming in
			// unfortunately, many font files contain glyphs with extraneous blank rows at the beginning or end
			// and also glyphs (such as space) with width=0 and height=1, which makes no sense
			// fortunately that means this code produces more efficient font data!!

			// offset the data and SparseMap tracks the actual points to be set
			Offset(xoffset, yoffset);
			xAdvance = xadvance;
		}

		/// <summary>
		/// The chacter code this glyph represents
		/// </summary>
		/// <remarks>
		/// This value is maintained by the font only as a convenient cache since the actual value
		/// depends on the font's FirstCode and the glyph's position in the list.
		/// </remarks>
		public ushort Code { get; internal set; }

		/// <summary>
		/// Print the glyph onto a Graphics canvas.
		/// </summary>
		/// <param name="g">Graphics canvas for printing</param>
		/// <param name="pixelsPerDot">Pixels to print for each glyph 'dot'; a zoom factor</param>
		/// <param name="textColor">The color for glyph printing</param>
		/// <param name="x">The x location for the upper left of the character cell</param>
		/// <param name="y">The y location for the upper left of the character cell</param>
		public void Print(Graphics g, int pixelsPerDot, Color textColor, ref int x, int y)
        {
            var rcBounds = Bounds;
			if (rcBounds.IsEmpty)
            {
				// nothing to print
                x += xAdvance * pixelsPerDot;
                return;
            }

            using var brushText = new SolidBrush(textColor);
			// enumerate the minimal bounds for the glyphs set pixels
			// and draw them relative to the specified x,y location
			for (int row = rcBounds.Top; row < rcBounds.Bottom; row++)
            {
                for (int col = rcBounds.Left; col < rcBounds.Right; col++)
                {
                    if (Get(col,row))
                    {
                        Rectangle rcCell = new(x + col * pixelsPerDot, y + row * pixelsPerDot, pixelsPerDot + 1, pixelsPerDot + 1);
                        g.FillRectangle(brushText, rcCell);
                    }
				}
			}
			// advance to the next character cell
			x += xAdvance * pixelsPerDot;
		}

		/// <summary>
		/// Create a glyph from it's XML representation.
		/// </summary>
		/// <param name="node">The XElement node representing the glyph</param>
		/// <returns>A Glyph</returns>
		public static Glyph FromXml(XElement node)
		{
			var glyph = new Glyph(
				Convert.FromBase64String(node.Element("Data").Value),
				int.Parse(node.Element("Width").Value),
				int.Parse(node.Element("Height").Value),
				int.Parse(node.Element("xOffset").Value),
				int.Parse(node.Element("yOffset").Value),
				int.Parse(node.Element("xAdvance").Value)
				)
			{
				Code = ushort.Parse(node.Element("Code").Value),
			};
			return glyph;
		}

		/// <summary>
		/// Create a glyph from it's XML string representation.
		/// </summary>
		/// <param name="xml">The XML string</param>
		/// <returns>A Glyph.</returns>
		public static Glyph FromXmlString(string xml)
        {
            return FromXml(XElement.Parse(xml));
        }

		/// <summary>
		/// Create an XML representation of the glyph.
		/// </summary>
		/// <returns>An XElement node</returns>
		public XElement ToXml()
		{
			var node = new XElement("Glyph");
			node.Add(new XElement("Code", $"{Code}"));
			node.Add(new XElement("Width", $"{Width}"));
			node.Add(new XElement("Height", $"{Height}"));
			node.Add(new XElement("xAdvance", $"{xAdvance}"));
			node.Add(new XElement("xOffset", $"{xOffset}"));
			node.Add(new XElement("yOffset", $"{yOffset}"));
			node.Add(new XElement("Data", Convert.ToBase64String(GetData().ToArray())));
			return node;
		}

		/// <summary>
		/// Create an XML string representation of the glyph.
		/// </summary>
		/// <returns>The XML string</returns>
		public string ToXmlString()
        {
			return ToXml().ToString(SaveOptions.DisableFormatting);
        }

		/// <summary>
		/// Create an XML representation of a collection of glyphs.
		/// </summary>
		/// <param name="glyphs">The glyphs to represent</param>
		/// <returns>The XElement</returns>
		public static XElement GlyphsToXml(IEnumerable<Glyph> glyphs)
		{
			var node = new XElement("glyphs");
			foreach (var glyph in glyphs)
			{
				node.Add(glyph.ToXml());
			}
			return node;
		}

		/// <summary>
		/// Create an XML string representation of a collection of glyphs.
		/// </summary>
		/// <param name="glyphs">The glyphs to represent</param>
		/// <returns>The XML string</returns>
		public static string GlyphsToXmlString(IEnumerable<Glyph> glyphs)
		{
			return GlyphsToXml(glyphs).ToString(SaveOptions.DisableFormatting);
		}

		/// <summary>
		/// Create a List of glyphs from their XML representation.
		/// </summary>
		/// <param name="node">The XElement representing the glyphs</param>
		/// <returns>A List of glyphs.</returns>
		public static List<Glyph> GlyphsFromXml(XElement node)
		{
			List<Glyph> glyphs = new();
			foreach (var gnode in node.Elements("Glyph"))
			{
				glyphs.Add(FromXml(gnode));
			}
			return glyphs;
		}

		/// <summary>
		/// Create a List of glyphs from their XML string representation.
		/// </summary>
		/// <param name="xml">The XML string</param>
		/// <returns>A List of glyphs.</returns>
		public static List<Glyph> GlyphsFromXmlString(string xml)
		{
			return GlyphsFromXml(XElement.Parse(xml));
		}

		/// <summary>
		/// Copy data from another glyph
		/// </summary>
		/// <param name="glyph">The glyph to copy from</param>
		public void CopyFrom(Glyph glyph)
		{
            Points = glyph.Points;
			ClearCache();
			xAdvance = glyph.xAdvance;
		}

		/// <summary>
		/// Set the glyph representation tto that of a hollow rectangle.
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		public void SetRect(int width, int height)
		{
			Clear();
			// build the left and right sides
			for (int x = 0; x < width; x++)
			{
				Set(x, -1);
				Set(x, -height);
			}
			// build the top and bottom sides
			for (int y = 1; y <= height; y++)
			{
				Set(0, -y);
				Set(width - 1, -y);
			}
		}
	}
}
