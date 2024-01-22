using System.Diagnostics;

namespace GFXFontEditor
{
	public class GfxBinaryFile
	{
		public static IEnumerable<(string ext, string title)> GetExtensions()
		{
			yield return (".gfxfntb", "Binary Font");
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
		public static GfxFont Load(string fileName)
		{
			using BinaryReader reader = new(File.Open(fileName, FileMode.Open));

			// read the GFXfont structure datta
			var bitmapOffset = reader.ReadInt32();      //   uint8_t* bitmap;
			var glyphOffset = reader.ReadInt32();       //   GFXglyph* glyph;
			var first = reader.ReadUInt16();            //   uint16_t first;
			var last = reader.ReadUInt16();             //   uint16_t last;
			var yAdvance = reader.ReadByte();           //   uint8_t yAdvance;

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
				reader.ReadUInt16(),    // uint16_t bitmapOffset;
				reader.ReadByte(),    // uint8_t width;
				reader.ReadByte(),    // uint8_t height;
				reader.ReadByte(),    // uint8_t xAdvance;
				reader.ReadSByte(),    // int8_t xOffset;
				reader.ReadSByte()     // int8_t yOffset;
				));
				reader.ReadByte();      // extra 8-byte alignment byte 
			}

			// build our Glyphs and the font from the loaded data
			for (int i = 0; i < gliphs.Count; i++)
			{
				var (offset, Width, Height, xAdvance, xOffset, yOffset) = gliphs[i];
				int end = i == gliphs.Count - 1 ? bitmap.Length : gliphs[i + 1].offset;
				var glyph = new Glyph(bitmap[offset..end], Width, Height, xOffset, yOffset, xAdvance);
				font.Add(glyph);
			}
			return font;
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
		public static bool Save(GfxFont font, string fileName)
		{
			List<byte> bitmap = new(font.Glyphs.SelectMany(g => g.GetData()));
			BinaryWriter writer = new(File.Open(fileName, FileMode.Create));

			// typedef struct {
			writer.Write((Int32)GFXFontSize);                       //   uint8_t* bitmap;
			writer.Write((Int32)GFXFontSize + bitmap.Count);        //   GFXglyph* glyph;
			writer.Write((ushort)font.FirstCode);                        //   uint16_t first;
			writer.Write((ushort)(font.FirstCode + font.Glyphs.Count - 1));  //   uint16_t last;
			writer.Write((byte)font.yAdvance);                           //   uint8_t yAdvance;
																	// } GFXfont;

			writer.Write(bitmap.ToArray());                         // const uint8_t <>Bitmaps[] PROGMEM = { };

			int offset = 0;
			foreach (var glyph in font.Glyphs)
			{
				// typedef struct {
				writer.Write((UInt16)offset);                       //   uint16_t bitmapOffset;
				writer.Write((byte)glyph.Width);                    //   uint8_t width;
				writer.Write((byte)glyph.Height);                   //   uint8_t height;
				writer.Write((byte)glyph.xAdvance);                 //   uint8_t xAdvance;
				writer.Write((sbyte)glyph.xOffset);                 //   int8_t xOffset;
				writer.Write((sbyte)glyph.yOffset);                 //   int8_t yOffset;
																	// } GFXglyph;
				writer.Write((byte)0);                              // alignment to 8-byte records
				offset += glyph.GetData().Count;
			}
			writer.Close();
			return true;
		}

	}
}
