using System.Xml.Linq;

namespace GFXFontEditor
{
	public class GfxXmlFile
	{
		public static IEnumerable<(string ext, string title)> GetExtensions()
		{
			yield return (".gfxfntx", "XML Font");
		}

		/// <summary>
		/// Load a font from an XML file.
		/// </summary>
		/// <param name="fileName">Name of the file to load.</param>
		/// <returns>GfxFont loaded, or null if the load fails.</returns>
		public static GfxFont Load(string fileName)
		{
			return GfxFont.FromXml(XElement.Load(fileName));
		}

		/// <summary>
		/// Save the font to an XML file.
		/// </summary>
		/// <param name="fileName">Name of the file to save</param>
		/// <returns>True if the save was successful</returns>
		public static bool Save(GfxFont font, string fileName)
		{
			File.WriteAllText(fileName, font.ToXmlString());
			return true;
		}
	}
}
