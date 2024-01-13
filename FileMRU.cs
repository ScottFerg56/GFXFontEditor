namespace GFXFontEditor
{
	/// <summary>
	/// Support for 'Recently Used Files' menu.
	/// </summary>
	public class FileMRU
	{
		private List<string> _FileNames = new();
		public List<string> FileNames
		{
			get => _FileNames ??= new();
			set
			{
				_FileNames = value;
				UpdateMenu();
			}
		}

		public string this[int index]
		{
			get => (index >= 0 && index < FileNames.Count) ? _FileNames[index] : null;
		}

		public event EventHandler Changed;

		private readonly int NumEntries;
		private readonly ToolStripDropDownItem tsParent;

		private readonly EventHandler Handler;

		public class MruClickEventArgs : System.EventArgs
		{
			public string name { get; protected set; }
			public MruClickEventArgs(string name) { this.name = name; }
		}

		public event EventHandler MruClick;

		public FileMRU(int numEntries, ToolStripDropDownItem parent)
		{
			NumEntries = numEntries;
			tsParent = parent;
			Handler = new EventHandler(menuItemMRUFile_Click);
			UpdateMenu(tsParent);
		}

		private void menuItemMRUFile_Click(object sender, System.EventArgs e)
		{
			if (MruClick != null)
			{
				int inx = (int)(sender as ToolStripMenuItem).Tag;
				MruClick(this, new MruClickEventArgs(FileNames[inx]));
			}
		}

		private void InsertEntry(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))		// weed out improper names and duplicates
				return;
			RemoveFilename(fileName);
			FileNames.Insert(0, fileName);					// insert at the head
			while (FileNames.Count > NumEntries)			// remove trailing names overflowing the desired count
				FileNames.RemoveAt(NumEntries);
		}

		private void RemoveFilename(string fileName)
		{
			// do a case-insensitive comparison in the collection
			FileNames.RemoveAll(n => string.Compare(n, fileName, true) == 0);
		}

		public void AddEntry(string fileName)
		{
			if (fileName == this[0])
				return;
			InsertEntry(fileName);
			UpdateMenu();
		}

		public void RemoveEntry(string fileName)
		{
			RemoveFilename(fileName);
			UpdateMenu();
		}

		private void UpdateMenu()
		{
			UpdateMenu(tsParent);
			Changed?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateMenu(ToolStripDropDownItem menu)
		{
			menu.DropDownItems.Clear();
			if (FileNames != null)
			{
				for (int index = 1; index <= FileNames.Count; index++)
					menu.DropDownItems.Add(((index < 10) ? "&" : "") + index.ToString() + " " + FileNames[index - 1], null, Handler).Tag = index - 1;
			}
			menu.Enabled = menu.DropDownItems.Count > 0;
		}
	}
}
