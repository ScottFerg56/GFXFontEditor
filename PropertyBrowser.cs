namespace GFXFontEditor
{
	/// <summary>
	/// Simple form with a full-sized PropertyGris control.
	/// </summary>
	public partial class PropertyBrowser : Form
	{
		public PropertyBrowser()
		{
			InitializeComponent();
			Icon = Application.OpenForms[0]?.Icon;
		}

		private void buttonCancel_Click(object sender, EventArgs e) => Close();
	}
}
