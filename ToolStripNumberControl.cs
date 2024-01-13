using System.Windows.Forms.Design;

/*
 https://glenviewsoftware.com/projects/products/adafonteditor/adafruit-gfx-font-format/

 TODO:
	adjustable pixelsPerDot in Viewer (zoom)
	y/xAdvance can go beyond current picture size
	copy/paste glyph
	modify font.first/last char code
		??
		change glyph symbol/code
	open/import font
		binary?
		.xml?
		.bdf??
		load/paste BMP/PNG, e.g from Material Design Icons
		other??
	save(as)/export font
		header file
		binary?
			format to load at runtime rather than include as header
		.xml?
		.bdf??
*/
namespace GFXFontEditor
{
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
	public class ToolStripNumberControl : ToolStripControlHost
	{
		public ToolStripNumberControl()
			: base(new NumericUpDown())
		{

		}

		protected override void OnSubscribeControlEvents(Control control)
		{
			base.OnSubscribeControlEvents(control);
			((NumericUpDown)control).ValueChanged += new EventHandler(OnValueChanged);
		}

		protected override void OnUnsubscribeControlEvents(Control control)
		{
			base.OnUnsubscribeControlEvents(control);
			((NumericUpDown)control).ValueChanged -= new EventHandler(OnValueChanged);
		}

		public event EventHandler ValueChanged;

		public Control NumericUpDownControl
		{
			get { return Control as NumericUpDown; }
		}

		public void OnValueChanged(object sender, EventArgs e)
		{
			ValueChanged?.Invoke(this, e);
		}
	}
}
