using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;

namespace GFXFontEditor
{
	/// <summary>
	/// An editor for bitmap fonts for use with the Adafruit Arduino GFX Library.
	/// </summary>
	public partial class Editor : Form
	{
		/// <summary>
		/// Editor constructor.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		public Editor(string[] args)
		{
			InitializeComponent();
			// setup the PixelsPerDot updown control
			UpDownPixelsPerDot.Minimum = 1;
			UpDownPixelsPerDot.Maximum = 10;
			UpDownPixelsPerDot.Value = PixelsPerDot;
			UpDownPixelsPerDot.ValueChanged += UpDownPPD_ValueChanged;

			// timer for auto sequencing thru drag-dropped files
			OpenTimer.Tick += OpenTimer_Tick;
			// recently used files UI
			MRUNames = new FileMRU(10, recentFilesToolStripMenuItem);
			MRUNames.MruClick += (object sender, EventArgs e) => LoadFile(((FileMRU.MruClickEventArgs)e).name);
			MRUNames.FileNames = Properties.Settings.Default.RecentFiles.Split(',').ToList();
			MRUNames.Changed += MRUNames_Changed;
			// open file from command line?
			if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
				startupFileName = args[0];
		}

		readonly string startupFileName = null;

		private void Editor_Shown(object sender, EventArgs e)
		{
			listViewGlyphs.Focus();
			// defer any font related UI interaction until after the form is shown
			// and the layout is settled
			// otherwise problems ensue!
			if (startupFileName is not null)
			{
				if (LoadFile(startupFileName))
					return;
			}
			// open most recently used file?
			while (MRUNames[0] != null)
			{
				if (File.Exists(MRUNames[0]))
				{
					if (LoadFile(MRUNames[0]))
						return;
				}
				MRUNames.RemoveEntry(MRUNames[0]);
			}
			NewFile();
		}

		private void Editor_FormClosing(object sender, FormClosingEventArgs e)
		{
			// check for unsaved changes before closing
			if (!CheckChanges(e.CloseReason))
				e.Cancel = true;
		}

		private void Editor_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		/// <summary>
		/// The current font being edited.
		/// </summary>
		GfxFont CurrentFont;

		/// <summary>
		/// The width of grid cells for the pictureBoxGlyph rendering,
		/// calculated based on the window size and needed font pixel bounds.
		/// </summary>
		int cellWidth = 30;

		// offsets to center the glyph display
		int cellXoff = 0;
		int cellYoff = 0;

		/// <summary>
		/// Pixels to print for each glyph 'dot' in the pictureBoxFontView; a zoom factor.
		/// </summary>
		int PixelsPerDot = 3;

		/// <summary>
		///  Set the title Text of the form based on the current font filename and changed state.
		/// </summary>
		void SetTitle()
		{
			var chg = DocChanged ? "*" : "";
			var fn = Path.GetFileName(CurrentFont.FullPathName);
			if (string.IsNullOrWhiteSpace(fn))
				fn = "<new>";
			Text = $"GFX Font Editor - {fn} {chg}";
		}

		bool _DocChanged;
		/// <summary>
		/// Track changes in the font.
		/// </summary>
		bool DocChanged
		{
			get => _DocChanged;
			set
			{
				if (_DocChanged == value)
					return;
				_DocChanged = value;
				SetTitle();
			}
		}

		/// <summary>
		/// Note a change in the font.
		/// </summary>
		/// <param name="bmpChange">True if a glyph bitmap change is involved</param>
		public void OnChange(bool bmpChange = true)
		{
			UpdateGlyphItem(CurrentGlyph);
			toolStripLabelNumGlyphs.Text = $"   # Glyphs: {CurrentFont.Glyphs.Count}";
			if (bmpChange)
			{
				pictureBoxGlyph.Invalidate();
				ShowFontView();
			}
			DocChanged = true;
		}

		/// <summary>
		/// Get the Glyph associated with a ListViewItem.
		/// </summary>
		/// <param name="item">The ListViewItem</param>
		/// <returns>The Glyph</returns>
		static Glyph GlyphOfItem(ListViewItem item) => item?.Tag as Glyph;

		/// <summary>
		/// Get the ListViewItem associated with a Glyph.
		/// </summary>
		/// <param name="glyph">The Glyph</param>
		/// <returns>The ListViewItem</returns>
		public ListViewItem ItemOfGlyph(Glyph glyph)
		{
			return listViewGlyphs.Items.OfType<ListViewItem>().FirstOrDefault(i => i.Tag == glyph);
		}

		/// <summary>
		/// Gets the current Glyph being edited, as selected bu the listViewGlyphs
		/// </summary>
		Glyph CurrentGlyph => GlyphOfItem(listViewGlyphs.SelectedItems.OfType<ListViewItem>().FirstOrDefault());

		/// <summary>
		/// Get the index of a Glyph within the listViewGlyphs.
		/// </summary>
		/// <param name="glyph">The Glyph</param>
		/// <returns>The item index</returns>
		public int IndexOfGlyph(Glyph glyph)
		{
			return ItemOfGlyph(glyph).Index;
		}

		/// <summary>
		/// Select an item index in the listViewGlyphs.
		/// </summary>
		/// <param name="inx">The index to add</param>
		/// <param name="clear">True to make it the only selected item</param>
		void SelectIndex(int inx, bool clear = true)
		{
			if (listViewGlyphs.Items.Count == 0)
				return;
			if (inx < 0 || inx >= listViewGlyphs.Items.Count)
			{
				listViewGlyphs.SelectedItems.Clear();
				return;
			}
			if (clear)
				listViewGlyphs.SelectedItems.Clear();
			listViewGlyphs.SelectedIndices.Add(inx);
			listViewGlyphs.EnsureVisible(inx);
			listViewGlyphs.FocusedItem = listViewGlyphs.Items[inx];
			listViewGlyphs.Focus();
		}

		/// <summary>
		/// Select a glyph in the listViewGlyphs.
		/// </summary>
		/// <param name="glyph">The glyph to add</param>
		/// <param name="clear">True to make it the only selected item</param>
		void SelectGlyph(Glyph glyph, bool clear = true)
		{
			SelectIndex(IndexOfGlyph(glyph), clear);
		}

		/// <summary>
		/// Get the PixelsPerDot from the menu updown control.
		/// </summary>
		NumericUpDown UpDownPixelsPerDot => toolStripNumberControlPPD.Control as NumericUpDown;

		/// <summary>
		/// Set PixelsPerDot scale for the pictureBoxFontView with changes from the UI.
		/// </summary>
		private void UpDownPPD_ValueChanged(object sender, EventArgs e)
		{
			PixelsPerDot = (int)UpDownPixelsPerDot.Value;
			ShowFontView();
			listViewGlyphs.Focus();
		}

		/// <summary>
		/// Update the pictureBoxFontView when the requested display text changes from the UI.
		/// </summary>
		private void toolStripTextBoxFVText_TextChanged(object sender, EventArgs e)
		{
			ShowFontView();
		}

		/// <summary>
		/// Set the glyph Code with changes from the UI.
		/// </summary>
		private void numericUpDownGlyphCode_ValueChanged(object sender, EventArgs e)
		{
			if (CurrentGlyph is not null)
			{
				if (CurrentGlyph.Code == (ushort)numericUpDownGlyphCode.Value)
					return;
				var code = (ushort)numericUpDownGlyphCode.Value;
				var glyph = CurrentGlyph;
				// sorting order may have changed
				var item = ItemOfGlyph(glyph);
				var inx = item.Index;
				if (code < glyph.Code && (glyph == CurrentFont.Glyphs.First() || CurrentFont.Glyphs.ElementAt(inx - 1).Code <= code) ||
					code > glyph.Code && (glyph == CurrentFont.Glyphs.Last() || CurrentFont.Glyphs.ElementAt(inx + 1).Code >= code))
				{
					// changing without bumping past the next glyph
					CurrentGlyph.Code = code;
					UpdateGlyphItem(glyph);
					OnChange(false);
				}
				else
				{
					// remove and add back into proper place
					CurrentGlyph.Code = code;
					listViewGlyphs.Items.RemoveAt(inx);
					CurrentFont.Remove(glyph);
					inx = CurrentFont.Add(glyph);
					listViewGlyphs.Items.Insert(inx, item);
					SelectGlyph(glyph);
					OnChange(true);
				}
			}
		}

		/// <summary>
		/// Handle ENTER key pressed in numericUpDownGlyphCode
		/// </summary>
		private void buttonDummy_Click(object sender, EventArgs e)
		{
			//
			// That fuckin' DING!
			// It happens when you press ENTER in any text control on a form without an 'AcceptButton'.
			// We'd like ENTER to have it's intended effect of reporting a vlaue change
			// but without the DING.
			// So we have to put a dummy button on the form as the 'AcceptButton' to silence the DING.
			// It has to be Visible and Enabled.
			// And we need to set TabStop to false and hide it below another control
			// so the user cannot get to it accidentally.
			// Fortunately the button doesn't steal focus and we can use that to test which 
			// control on the form the ENTER belongs to.
			//
			if (numericUpDownGlyphCode.Focused)
				numericUpDownGlyphCode_ValueChanged(null, EventArgs.Empty);
			else if (UpDownPixelsPerDot.Focused)
				UpDownPPD_ValueChanged(null, EventArgs.Empty);
			else if (toolStripTextBoxFVText.Focused)
				listViewGlyphs.Focus();
		}

		/// <summary>
		/// List of most recently used files.
		/// </summary>
		private FileMRU MRUNames { get; set; }

		/// <summary>
		/// Update saved app settings when MRU filenames change.
		/// </summary>
		private void MRUNames_Changed(object sender, EventArgs e)
		{
			Properties.Settings.Default.RecentFiles = string.Join(',', MRUNames.FileNames);
			Properties.Settings.Default.Save();
		}

		// extra cells along the left and top for pictureBoxGlyph mouse interaction
		const int gridMargin = 1;

		/// <summary>
		/// Recalc the cellWidth for the pictureBoxGlyph to reflect changes in the font or window size.
		/// </summary>
		void RecalcDesignSpace()
		{
			// don't resize the UI while we're dragging the mouse around!
			if (CurrentFont is null || pictureBoxGlyph.Capture)
				return;
			cellWidth = Math.Min(60, Math.Max(2, pictureBoxGlyph.ClientSize.Width / rcFull.Width));
			int cellHeight = Math.Min(60, Math.Max(2, pictureBoxGlyph.ClientSize.Height / rcFull.Height));
			cellWidth = Math.Min(cellWidth, cellHeight);
			// center the space within the picture box
			cellXoff = (pictureBoxGlyph.ClientSize.Width - cellWidth * rcFull.Width) / 2;
			cellYoff = (pictureBoxGlyph.ClientSize.Height - cellWidth * rcFull.Height) / 2;
			pictureBoxGlyph.Invalidate();
		}

		/// <summary>
		/// Redo layout with window size change.
		/// </summary>
		private void pictureBoxGlyph_Resize(object sender, EventArgs e)
		{
			RecalcDesignSpace();
		}

		Rectangle rcFullCache = Rectangle.Empty;

		/// <summary>
		/// The full rectangle bounds for the pictureBoxGlyph rendering.
		/// </summary>
		Rectangle rcFull
		{
			get
			{
				// cache the full bounds for use while a mouse
				// operation is in progress to stabilize the view
				if (CurrentFont is null || pictureBoxGlyph.Capture)
					return rcFullCache;
				var rc = CurrentFont.FullBounds;
				rcFullCache = new Rectangle(rc.Left - gridMargin, rc.Top - gridMargin, rc.Width + 2 * gridMargin, rc.Height + 2 * gridMargin);
				return rcFullCache;
			}
		}

		/// <summary>
		/// The full rectangle bounds for the current Glyph's character cell in the pictureBoxGlyph rendering.
		/// </summary>
		Rectangle rcBox
		{
			get
			{
				(int xul, int yul) = CellToMouse(0, -CurrentFont.yAdvance);
				(int xlr, int ylr) = CellToMouse(CurrentGlyph.xAdvance, 0);

				return Rectangle.FromLTRB(xul, yul, xlr, ylr);
			}
		}

		/// <summary>
		/// Convert the x,y mouse coordinates in the pictureBoxGlyph
		/// to the Glyph cell coordinates in the rendering.
		/// </summary>
		(int col, int row) MouseToCell(int x, int y)
		{
			//Rectangle rcFull = Rectangle.Union(GfxFont.Bounds(), new Rectangle(0, -GfxFont.yAdvance, GfxFont.MaxAdvance(), GfxFont.yAdvance));
			return ((x - cellXoff) / cellWidth + rcFull.Left, (y - cellYoff) / cellWidth + rcFull.Top);
		}

		/// <summary>
		/// Convert the Glyph cell coordinates in the rendering
		/// to x,y mouse coordinates in the pictureBoxGlyph.
		/// </summary>
		(int x, int y) CellToMouse(int col, int row)
		{
			return ((col - rcFull.Left) * cellWidth + cellXoff, (row - rcFull.Top) * cellWidth + cellYoff);
		}

		/// <summary>
		/// Draw a Glyph in a Graphics canvas.
		/// </summary>
		/// <param name="gr">The Graphics canvas</param>
		/// <param name="glyph">Glyph to render</param>
		public void DrawGlyph(Graphics gr, Glyph glyph)
		{
			//gr.Clear(Color.LightGray);
			if (glyph is null)
				return;
			// the bounds of the glyph bitmap, ignoring the Width and Height when there's no bitmap data
			var rcBmp = glyph.Bounds;

			var rcGlyph = new Rectangle(0, -CurrentFont.yAdvance, glyph.xAdvance, CurrentFont.yAdvance);
			// scan the full grid of possible pixels/dots to be rendered
			for (int row = rcFull.Top; row < rcFull.Bottom; row++)
			{
				for (int col = rcFull.Left; col < rcFull.Right; col++)
				{
					(int left, int top) = CellToMouse(col, row);
					Rectangle rcCell = new(left, top, cellWidth, cellWidth);
					// draw an extra cell boundary, when cell size permits
					if (cellWidth >= 10)
					{
						gr.DrawRectangle(Pens.Gray, rcCell);
						rcCell.Inflate(-1, -1);
					}
					if (cellWidth < 10)
						rcCell = new(rcCell.Left, rcCell.Top, rcCell.Width + 1, rcCell.Height + 1);
					if (rcBmp.Contains(col, row))
					{
						// fill in set dots from the Glyph in black
						// clear dots in light gray
						if (glyph.Get(col, row))
							gr.FillRectangle(Brushes.Black, rcCell);
						else
							gr.FillRectangle(Brushes.LightGray, rcCell);
					}
					else if (rcGlyph.Contains(col, row))
					{
						// dots inside the Glyph bounds but not within the tight Glyph bmp bounds
						// are light slate gray
						gr.FillRectangle(Brushes.LightSlateGray, rcCell);
					}
					else if (CurrentFont.FullBounds.Contains(col, row))
					{
						// dots outside the Glyph bounds in dim gray
						gr.FillRectangle(Brushes.DimGray, rcCell);
					}
					else
					{
						// dots outside the editing area bounds in gray
						gr.FillRectangle(Brushes.Gray, rcCell);
					}
				}
			}
			gr.DrawRectangle(new Pen(Color.Blue, 3), rcBox);
		}

		/// <summary>
		/// Paint the pictureBoxGlyph with a cell grid rendering of the current Glyph.
		/// </summary>
		private void pictureBoxGlyph_Paint(object sender, PaintEventArgs e)
		{
			DrawGlyph(e.Graphics, CurrentGlyph);
		}

		bool DragBmp;           // dragging the Glyph's bmp dots
		bool Draw;              // drawing dots
		bool DrawState;			// bit state to draw
		int ColStart;           // grid column where mouse operation started
		int RowStart;           // grid row where mouse operation started
		int ColLast;            // column of most recent mouse operation
		int RowLast;            // row of most recent mouse operation

		bool DragYAdvance;      // dragging the yAdvance value (bottom in the character box)
		bool DragXAdvance;      // dragging the xAdvance value (right line in the character box)
		int xLast;              // x xoordinate of most recent mouse operation
		int yLast;              // y coordinate of most recent mouse operation

		/// <summary>
		/// Start a mouse operation in the pictureBoxGlyph.
		/// </summary>
		private void pictureBoxGlyph_MouseDown(object sender, MouseEventArgs e)
		{
			var glyph = CurrentGlyph;
			if (glyph is null)
				return;
			(ColStart, RowStart) = MouseToCell(e.X, e.Y);
			if (e.Button == MouseButtons.Left)
			{
				if (pictureBoxGlyph.Cursor == Cursors.VSplit)
				{
					// dragging xAdvance
					pictureBoxGlyph.Capture = true;
					DragXAdvance = true;
					xLast = e.X;
					yLast = e.Y;
				}
				else if (pictureBoxGlyph.Cursor == Cursors.HSplit)
				{
					// dragging yAdvance
					pictureBoxGlyph.Capture = true;
					DragYAdvance = true;
					xLast = e.X;
					yLast = e.Y;
				}
				else if (rcFull.Contains(ColStart, RowStart))
				{
					// drawing glyph dots
					pictureBoxGlyph.Capture = true;
					pictureBoxGlyph.Cursor = Cursors.Hand;
					Draw = true;
					// start drawing with tthe opposite state of the current bit
					DrawState = !CurrentGlyph.Get(ColStart, RowStart);
					DrawBit(ColStart, RowStart);
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				//Debug.WriteLine($"col: {ColStart}  row: {RowStart}");
				if (glyph.Bounds.Contains(ColStart, RowStart))
				{
					// dragging glyph dots
					pictureBoxGlyph.Capture = true;
					pictureBoxGlyph.Cursor = Cursors.Hand;
					DragBmp = true;
					ColLast = ColStart;
					RowLast = RowStart;
				}
			}
		}

		/// <summary>
		/// Handling mouse operations in the pictureBoxGlyph.
		/// </summary>
		private void pictureBoxGlyph_MouseMove(object sender, MouseEventArgs e)
		{
			if (CurrentGlyph is null)
				return;
			if (!pictureBoxGlyph.Capture)
			{
				// no capture - no mouse down
				// change cursor over the right and bottom lines of the blue character box
				// as a visual cue to dragging the xAdvance and yAdvance values
				var rc = rcBox;
				if (Math.Abs(e.X - rc.Right) < 3 && e.Y > rc.Top && rc.Y < rc.Bottom)
					pictureBoxGlyph.Cursor = Cursors.VSplit;
				else if (Math.Abs(e.Y - rc.Bottom) < 3 && e.X > rc.Left && rc.X < rc.Right)
					pictureBoxGlyph.Cursor = Cursors.HSplit;
				else
					pictureBoxGlyph.Cursor = Cursors.Default;
				return;
			}
			if (Draw)
			{
				// keep drawing grid cell dots for the glyph
				// as the mouse moves from cell to cell
				(int col, int row) = MouseToCell(e.X, e.Y);
				if (col != ColLast || row != RowLast && rcFull.Contains(col, row))
					DrawBit(col, row);
			}
			else if (DragYAdvance)
			{
				// only drag YAdvance in Y
				int rows = (e.Y - yLast) / cellWidth;
				if (rows != 0)
				{
					if (CurrentFont.yAdvance + rows > 0)
					{
						// set the font yAdvance
						yLast = e.Y;
						CurrentFont.yAdvance += rows;
						toolStripLabelHeight.Text = $"   Line Height: {CurrentFont.yAdvance}";
						// with SHIFT, offset ALL the glyphs the same amount
						// to maintain their relationships with the baseline
						if (ModifierKeys == Keys.Shift)
						{
							foreach (var glyph in CurrentFont.Glyphs)
							{
								glyph.Offset(0, -rows);
								UpdateGlyphItem(glyph);
							}
						}
						//Debug.WriteLine($"yAdvance adjust by {rows} to {GfxFont.yAdvance}");
						OnChange();
					}
				}
			}
			else if (DragXAdvance)
			{
				// only drag XAdvance in X
				int cols = (e.X - xLast) / cellWidth;
				if (cols != 0)
				{
					if (CurrentGlyph.xAdvance + cols >= 0)
					{
						// set the glyph xAdvance
						xLast = e.X;
						// with SHIFT, set xAdvance for ALL glyphs
						// as a shorcut for creating a FIXED font
						if (ModifierKeys == Keys.Shift)
						{
							foreach (var glyph in CurrentFont.Glyphs)
							{
								glyph.xAdvance = CurrentGlyph.xAdvance + cols;
								UpdateGlyphItem(glyph);
							}
						}
						else
						{
							CurrentGlyph.xAdvance += cols;
						}
						//Debug.WriteLine($"xAdvance adjust by {cols} to {CurrentGlyph.xAdvance}");
						OnChange();
					}
				}
			}
			else if (DragBmp)
			{
				(int col, int row) = MouseToCell(e.X, e.Y);
				if (col != ColLast || row != RowLast)
				{
					// offset all the glyph's dots
					//Debug.WriteLine($"DRAG  col: {col}  row: {row}");
					var glyph = CurrentGlyph;
					glyph.Offset(col - ColLast, row - RowLast);
					ColLast = col;
					RowLast = row;
					OnChange();
				}
			}
		}

		/// <summary>
		/// Set the state of a pixel dot for the glyph.
		/// </summary>
		/// <param name="col">Column of the pixel to set</param>
		/// <param name="row">Row of the pixel to set</param>
		private void DrawBit(int col, int row)
		{
			// limit changes to the margin around the font bounds
			// of the design area, otherwise it can get hard to manage
			if (!rcFull.Contains(col, row))
				return;
			if (CurrentGlyph.Get(col, row) != DrawState)
				CurrentGlyph.Toggle(col, row);
			ColLast = col;
			RowLast = row;
			OnChange();
		}

		/// <summary>
		/// Finish a mouse operation in the pictureBoxGlyph.
		/// </summary>
		private void pictureBoxGlyph_MouseUp(object sender, MouseEventArgs e)
		{
			pictureBoxGlyph.Cursor = Cursors.Default;
			if (!pictureBoxGlyph.Capture)
				return;
			pictureBoxGlyph.Capture = false;
			DragBmp = false;
			DragYAdvance = false;
			DragXAdvance = false;
			Draw = false;
			// just in case a change effects the layout and was deferred during Capture
			RecalcDesignSpace();
		}

		/// <summary>
		/// The left and right extent along the text line for each printed Glyph in the pictureBoxFontView.
		/// </summary>
		List<Rectangle> boundsFontView = new();

		/// <summary>
		/// Get the sample text, decoded from unicode hex escape sequences.
		/// </summary>
		string SampleText
		{
			get
			{
				try
				{
					return System.Text.RegularExpressions.Regex.Unescape(toolStripTextBoxFVText.Text);
				}
				catch (Exception)
				{
					// this will fail as the user types incomplete escape sequences
					return toolStripTextBoxFVText.Text;
				}
			}
		}

		/// <summary>
		/// Prepare the bitmap display image of sample text for the pictureBoxFontView.
		/// </summary>
		void ShowFontView(bool center = false)
		{
			pictureBoxFontView.Image = CurrentFont?.ToBitmap(SampleText, PixelsPerDot, splitContainer2.Panel2.ClientRectangle.Width, Color.Black, Color.White, out boundsFontView);
			SelectFontViewGlyph(center);
		}

		/// <summary>
		/// Get the left and right extent of the current glyph in the pictureBoxFontView.
		/// </summary>
		Rectangle BoundsOfSelectedGlyph()
		{
			if (CurrentGlyph is not null)
			{
				int inx;
				var s = SampleText;
				if (!string.IsNullOrWhiteSpace(s))
					inx = s.IndexOf((char)CurrentGlyph.Code);
				else
					inx = CurrentFont.Glyphs.IndexOf(CurrentGlyph);
				if (inx >= 0 && inx < boundsFontView.Count)
					return boundsFontView[inx];
			}
			return Rectangle.Empty;
		}

		/// <summary>
		/// Scroll the pictureBoxFontView in its parent panel to make sure the current glyph is visible
		/// in the bitmap image.
		/// </summary>
		void SelectFontViewGlyph(bool center = false)
		{
			var rc = BoundsOfSelectedGlyph();
			if (!rc.IsEmpty)
			{
				//
				// The scroll value appears to take on the range
				// of 0 to the delta between the Image and Panel dimensions.
				// Though the scroll maximum does not reflect this.
				// So we adjust the value to nudge the glyph into view.
				//
				//var min = splitContainer2.Panel2.VerticalScroll.Minimum;
				var max = splitContainer2.Panel2.VerticalScroll.Maximum;

				var v = splitContainer2.Panel2.VerticalScroll.Value;
				var ph = splitContainer2.Panel2.Height;
				var ih = pictureBoxFontView.Image.Height;

				var delta = ih - ph;

				if (max == 100 && delta > 100)
				{
					// if we get here before Form_Shown,
					// the scroll max will still be at the default 100
					// Debug.Fail("Font view UI has not settled");
					return;
				}

				// nothing to do if the Image fits entirely within the Panel
				if (delta > 0)
				{
					const int margin = 10;
					// calculate a new value, with a bit of a margin added
					// but only if the glyph is not fully in view
					int newv = v;
					if (center)
					{
						// try to center glyph in the scroll
						newv = Math.Max(0, ((rc.Bottom + rc.Top) - ph) / 2);
					}
					else
					{   // try to keep scroll stable
						if (rc.Top < v + margin)
							newv = rc.Top - margin;
						else if (rc.Bottom > v + ph - margin)
							newv = rc.Bottom - ph + margin;
						if (newv < 0)
							newv = 0;
						else if (newv > delta)
							newv = delta;
					}
					if (newv != v && newv <= max)
					{
						//Debug.WriteLine($"v:{v} max:{max} delta:{delta} left:{left} newv:{newv}");
						try
						{
							splitContainer2.Panel2.VerticalScroll.Value = newv;
							splitContainer2.Panel2.PerformLayout(); // required to make scrollbar do it's job?!
						}
						catch (Exception)
						{
						}
					}
				}
			}
			pictureBoxFontView.Invalidate();
		}

		/// <summary>
		/// Draw a red box around the current glyph in the pictureBoxFontView image.
		/// </summary>
		private void pictureBoxFontView_Paint(object sender, PaintEventArgs e)
		{
			if (pictureBoxFontView.Image is null)
				return;
			// here we're painting over the Image already drawn by the base PictureBox code
			var rc = BoundsOfSelectedGlyph();
			if (!rc.IsEmpty)
			{
				using var pen = new Pen(Color.Red, (float)Math.Ceiling(PixelsPerDot / 2.0));
				e.Graphics.DrawRectangle(pen, rc);
			}
		}

		/// <summary>
		/// Allow the mouse to select a new current glyph from the pictureBoxFontView image.
		/// </summary>
		private void pictureBoxFontView_MouseDown(object sender, MouseEventArgs e)
		{
			// locate the left,right bounds containing the mouse
			for (int i = 0; i < boundsFontView.Count; i++)
			{
				var rc = boundsFontView[i];
				if (rc.Contains(e.X, e.Y))
				{
					// find the glyph based on the index
					Glyph glyph;
					var s = SampleText;
					if (!string.IsNullOrWhiteSpace(s))
					{
						// using the sample text
						var c = (ushort)s[i];
						glyph = CurrentFont.Glyphs.FirstOrDefault(g => g.Code == c);
					}
					else
					{
						// using the full glyphs list
						glyph = CurrentFont.Glyphs[i];
					}
					// may not find the glyph, if the sample text has characters not in the list
					if (glyph is not null)
					{
						// select it
						SelectGlyph(glyph);
					}
				}
			}
		}

		/// <summary>
		/// Resize the font view image with it's containing panel.
		/// </summary>
		private void splitContainer2_Panel2_Resize(object sender, EventArgs e)
		{
			ShowFontView();
		}

		/// <summary>
		/// Handle glyph selection change from the listViewGlyphs.
		/// </summary>
		private void listViewGlyphs_SelectedIndexChanged(object sender, EventArgs e)
		{
			numericUpDownGlyphCode.Value = CurrentGlyph?.Code ?? 0;
			pictureBoxGlyph.Invalidate();
			SelectFontViewGlyph();
		}

		/// <summary>
		/// Process shortcut keys for commands in the listViewGlyphs.
		/// </summary>
		private void listViewGlyphs_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Insert:
					insertToolStripMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.Delete:
					deleteToolStripMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.X:
					if (e.Control)
						cutToolStripMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.C:
					if (e.Control)
						copyToolStripMenuItem_Click(null, EventArgs.Empty);
					break;
				case Keys.V:
					if (e.Control)
						pasteInsertToolStripMenuItem_Click(null, EventArgs.Empty);
					break;
				default:
					break;
			}

		}

		private void listViewGlyphs_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Left:
					{
						var item = listViewGlyphs.FocusedItem;
						if (item is null || item.Index <= 0)
							break;
						listViewGlyphs.FocusedItem = listViewGlyphs.Items[item.Index - 1];
						listViewGlyphs.SelectedIndices.Clear();
						listViewGlyphs.SelectedIndices.Add(item.Index - 1);
						listViewGlyphs.EnsureVisible(item.Index - 1);
						break;
					}
				case Keys.Right:
					{
						var item = listViewGlyphs.FocusedItem;
						if (item is null || item.Index >= listViewGlyphs.Items.Count - 1)
							break;
						listViewGlyphs.FocusedItem = listViewGlyphs.Items[item.Index + 1];
						listViewGlyphs.SelectedIndices.Clear();
						listViewGlyphs.SelectedIndices.Add(item.Index + 1);
						listViewGlyphs.EnsureVisible(item.Index + 1);
					}
					break;
			}
		}

		/// <summary>
		/// Change the glyph code UI interaction between hex and decimal.
		/// </summary>
		private void checkBoxHexCode_CheckedChanged(object sender, EventArgs e)
		{
			numericUpDownGlyphCode.Hexadecimal = checkBoxHexCode.Checked;
			UpdateAllGlyphItems();
			listViewGlyphs.Focus();
		}

		/// <summary>
		/// Get a string array of values for the glyph's ListViewItem.
		/// </summary>
		/// <param name="glyph">The glyph</param>
		/// <returns>String array for the subitems</returns>
		private string[] GlyphSubitemValues(Glyph glyph)
		{
			var c = (glyph.Code < 0x20 || glyph.Code >= 0x80 && glyph.Code <= 0xA0 || glyph.Code >= 0xFFFE) ? (char)0xFFFD : (char)glyph.Code;
			// column 0 is a hidden dummy because it cannot be set to align center!
			return
			[
				"",
				c.ToString(),
				checkBoxHexCode.Checked ? $"0x{glyph.Code:X2}" : glyph.Code.ToString(),
				glyph.Width.ToString(),
				glyph.Height.ToString(),
				glyph.xAdvance.ToString(),
				glyph.xOffset.ToString(),
				glyph.yOffset.ToString()
			];
		}

		/// <summary>
		/// Create a new ListViewItem for a glyph.
		/// </summary>
		/// <param name="glyph">The glyph</param>
		/// <returns>A new ListViewItem for the glyph</returns>
		private ListViewItem NewGlyphItem(Glyph glyph)
		{
			// column 0 is a hidden dummy because it cannot be set to align center!
			return new ListViewItem(GlyphSubitemValues(glyph))
			{
				Tag = glyph
			};
		}

		/// <summary>
		/// Update the text display of the ListViewItem for a glyph.
		/// </summary>
		/// <param name="glyph">The glyph</param>
		void UpdateGlyphItem(Glyph glyph)
		{
			if (glyph is null)
				return;
			// the Segoe UI font used in the listViewGlyphs doesn't display some characters,
			// so use a known char for those; '?' in a black diamond
			var item = ItemOfGlyph(glyph);
			switch (glyph.Status)
			{
				case Glyph.States.Inserted:
					item.BackColor = Color.Silver;
					item.ForeColor = Color.DimGray;
					break;
				case Glyph.States.Error:
					item.BackColor = Color.RosyBrown;
					break;
				case Glyph.States.Normal:
				default:
					break;
			}
			var vals = GlyphSubitemValues(glyph);
			for (int i = 0; i < item.SubItems.Count; i++)
			{
				if (item.SubItems[i].Text != vals[i])
					item.SubItems[i].Text = vals[i];
			}
		}

		/// <summary>
		/// Update the text display in the listViewGlyphs for all the glyphs.
		/// </summary>
		void UpdateAllGlyphItems()
		{
			listViewGlyphs.BeginUpdate();
			foreach (var glyph in CurrentFont.Glyphs)
				UpdateGlyphItem(glyph);
			listViewGlyphs.EndUpdate();
		}

		/// <summary>
		/// Update Enable state for all items in the contextMenuStripGlyphList.
		/// </summary>
		private void contextMenuStripGlyphList_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var enable = CurrentGlyph is not null;
			insertToolStripMenuItem.Enabled = CurrentFont is not null;
			deleteToolStripMenuItem.Enabled = enable;
			cutToolStripMenuItem.Enabled = enable;
			copyToolStripMenuItem.Enabled = enable;
			pasteToolStripMenuItem.Enabled = enable && Clipboard.ContainsData(fmtGlyphName);
			pasteInsertToolStripMenuItem.Enabled = CurrentFont is not null && Clipboard.ContainsData(fmtGlyphName);
			flipHorizontalToolStripMenuItem.Enabled = enable;
			flipVerticalToolStripMenuItem.Enabled = enable;
			rotate90CWToolStripMenuItem.Enabled = enable;
			rotate90CCWToolStripMenuItem.Enabled = enable;
			rotate180ToolStripMenuItem.Enabled = enable;
			clearToolStripMenuItem.Enabled = enable;
			setRectToolStripMenuItem.Enabled = enable;
			flattenGlyphListToolStripMenuItem.Enabled = CurrentFont is not null && CurrentFont.Glyphs.Any();
		}

		/// <summary>
		/// Update the UI for the selection of a new font.
		/// </summary>
		/// <param name="font">The font to be slected</param>
		void SetFont(GfxFont font)
		{
			CurrentFont = font;
			RebuildAllGlyphItems();
			toolStripLabelHeight.Text = $"Line Height: {CurrentFont.yAdvance}";
			RecalcDesignSpace();
			if (font.Glyphs.Any())
			{
				// initially select a glyph
				Glyph glyph = null;
				if (!string.IsNullOrWhiteSpace(SampleText))
				{
					// first non-empty glyph represented in the sample text
					glyph = font.Glyphs.FirstOrDefault(g => SampleText.Contains((char)g.Code) && !g.Bounds.IsEmpty);
				}
				if (glyph is null)
				{
					// pick a glyph close to the middle between the first and last non-empty glyphs
					// this attempts to show the widest range of glyphs, especially when sampling fonts
					// by dropping font files and sequencing through them (manually or with auto-sequencing)
					glyph = font.Glyphs.FirstOrDefault(g => !g.Bounds.IsEmpty);
					var inx1 = IndexOfGlyph(glyph);
					var inx2 = IndexOfGlyph(font.Glyphs.Last(g => !g.Bounds.IsEmpty));
					for (int i = (inx1 + inx2) / 2; i != inx1; i += Math.Sign(inx1 - inx2))
					{
						if (!font.Glyphs[i].Bounds.IsEmpty)
						{
							glyph = font.Glyphs[i];
							break;
						}
					}
				}
				if (glyph is null)
					SelectIndex(0);
				else
					SelectGlyph(glyph);
			}
			toolStripLabelNumGlyphs.Text = $"# Glyphs: {CurrentFont.Glyphs.Count}";
			ShowFontView(true);
			SetTitle();
			DocChanged = false;
		}

		/// <summary>
		/// Rebuild all the font glyph ListViewItems.
		/// </summary>
		private void RebuildAllGlyphItems()
		{
			listViewGlyphs.BeginUpdate();
			listViewGlyphs.Items.Clear();
			foreach (var glyph in CurrentFont.Glyphs)
			{
				// add item with placeholder subitems
				listViewGlyphs.Items.Add(NewGlyphItem(glyph));
			}
			listViewGlyphs.EndUpdate();
		}

		/// <summary>
		/// Build a Filter string for Open/Save dialogs.
		/// </summary>
		/// <param name="exts">The extension, title string pairs returned from GFXFont.GetLoad/SaveFileExtensions</param>
		/// <param name="allTitle">A title string for the "all files" entry; null if none desired</param>
		/// <returns>The Filter string</returns>
		private static string BuildFilter(IEnumerable<(string ext, string title)> exts, string allTitle)
		{
			// eg:  "All Font Files|*.h;*.gfxfntx|Font H Files (*.h)|*.h|XML Font Files (*.gfxfntx)|*.gfxfntx",
			var allString = string.IsNullOrEmpty(allTitle) ? "" : $"{allTitle}|";
			var filter = "";
			// add each ext, title pair
			foreach (var (ext, title) in exts)
			{
				var wild = $"*{ext}";
				if (!string.IsNullOrEmpty(allTitle))
					allString += $"{wild};";
				filter += $"{title} ({wild})|{wild}|";
			}
			if (!string.IsNullOrEmpty(allTitle))
				allString = allString.TrimEnd() + "|";
			return $"{allString}{filter.TrimEnd('|')}";
		}

		/// <summary>
		/// Check for unsaved changes for the current font and offer the user the opportunity
		/// to save them.
		/// </summary>
		/// <param name="closeReason">From the FormClosing event; no Cancel option if not CloseReason.UserClosing</param>
		/// <returns>
		/// False if the pending operation should be cancelled due to save failure or cancelling by the user.
		/// </returns>
		bool CheckChanges(CloseReason closeReason = CloseReason.UserClosing)
		{
			if (DocChanged)
			{
				switch (MessageBox.Show("Save changes?", "GFX Font Editor",
						(closeReason == CloseReason.UserClosing) ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
						MessageBoxIcon.Exclamation))
				{
					case DialogResult.Yes:
						if (!SaveFile(CurrentFont.FullPathName))
							return false;
						break;
					case DialogResult.No:
						break;
					case DialogResult.Cancel:
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Close the current file and create a new simple font for editing.
		/// </summary>
		void NewFile()
		{
			if (!CheckChanges())
				return;
			var font = new GfxFont() { yAdvance = 8 };
			var glyph = new Glyph(null, 0, 0, 0, 0, 8) { Code = 0x20 };
			glyph.SetRect(glyph.xAdvance, font.yAdvance);
			font.Add(glyph);
			SetFont(font);
		}

		[DllImport("user32.dll")]
		//[LibraryImport("user32.dll")]
		private static extern IntPtr SendMessage(
			IntPtr hWnd,
			UInt32 msg,
			IntPtr wParam,
			IntPtr lParam);

		/// <summary>
		/// Close the current file and load a font file for editing.
		/// </summary>
		/// <param name="fileName">The name of the file to open</param>
		/// <returns>False if the load fails or closing the existing file fails or was cancelled.</returns>
		bool LoadFile(string fileName)
		{
			if (!CheckChanges())
				return false;
			try
			{
				Application.UseWaitCursor = true;
				SendMessage(Handle, 0x20, Handle, (IntPtr)1);
				var font = GfxFont.LoadFile(fileName);
				if (font is not null)
				{
					const int limit = 2048;
					if (font.Glyphs.Count > limit)
					{
						MessageBox.Show($"Large file warning:\r\nThis file will be truncated to {limit} of {font.Glyphs.Count} glyphs.", "GFX Font Editor");
						font.Truncate(limit);
					}
					SetFont(font);
					MRUNames.AddEntry(fileName);
					return true;
				}
				MRUNames.RemoveEntry(fileName);
				return false;
			}
			catch
			{
				MRUNames.RemoveEntry(fileName);
				return false;
			}
			finally
			{
				Application.UseWaitCursor = false;
				//SendMessage(Handle, 0x20, Handle, (IntPtr)1);
			}
		}

		/// <summary>
		/// Save the current font to a file.
		/// </summary>
		/// <param name="fileName">The name of the file for saving</param>
		/// <returns>True if the save succeeded</returns>
		bool SaveFile(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				// ask the user for a name for a new unnamed file
				return SaveFileAs();
			}
			else if (CurrentFont.SaveFile(fileName))
			{
				DocChanged = false;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Save the current font to a file with a name of the user's chosing.
		/// </summary>
		/// <returns>False if the save fails or the user cancels selection of a name</returns>
		bool SaveFileAs()
		{
			SaveFileDialog sfd = new()
			{
				FileName = Path.GetFileNameWithoutExtension(CurrentFont.FullPathName),
				InitialDirectory = Path.GetDirectoryName(CurrentFont.FullPathName),
				DefaultExt = ".h",
				Filter = BuildFilter(GfxFont.GetSaveFileExtensions(), null),
				FilterIndex = 0,
				Title = "Save Font"
			};
			if (sfd.ShowDialog() != DialogResult.OK)
				return false;
			if (SaveFile(sfd.FileName))
			{
				SetTitle();
				return true;
			}
			return false;
		}

		/// <summary>
		/// A list of files which were dropped onto the UI and remain to be opened.
		/// </summary>
		List<string> FilesToOpen = new();

		/// <summary>
		/// True if we're autoloading thru the dropped files using a timer.
		/// </summary>
		bool AutoSeqFiles = false;

		/// <summary>
		/// Timer to use when autoloading dropped files.
		/// </summary>
		readonly System.Windows.Forms.Timer OpenTimer = new() { Enabled = false, Interval = 100 };

		/// <summary>
		/// Show the next file to open on the tool strip
		/// </summary>
		void ShowNextFile()
		{
			if (FilesToOpen is not null && FilesToOpen.Count > 0)
			{
				toolStripButtonNextFile.Text = $"Next file: {Path.GetFileName(FilesToOpen.First())} ({FilesToOpen.Count} left)";
				toolStripButtonNextFile.Visible = true;
			}
			else
			{
				toolStripButtonNextFile.Text = "";
				toolStripButtonNextFile.Visible = false;
			}
		}

		/// <summary>
		/// Open the next file in FilesToOpen.
		/// </summary>
		private void OpenNextFile()
		{
			if (FilesToOpen != null && FilesToOpen.Count > 0)
			{
				string fileName = FilesToOpen.First();
				FilesToOpen.RemoveAt(0);
				LoadFile(fileName);
				if (FilesToOpen.Count > 0 && AutoSeqFiles)
				{
					// autosequence to next file
					OpenTimer.Interval = 2000;
					OpenTimer.Enabled = true;
				}
				ShowNextFile();
			}
			else
			{
				AutoSeqFiles = false;
			}
		}

		/// <summary>
		/// Sellect the next file to load from FilesToOpen.
		/// </summary>
		private void toolStripButtonNextFile_Click(object sender, EventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Shift) != 0)
			{
				AutoSeqFiles = false;
				FilesToOpen = new();
				ShowNextFile();
				OpenTimer.Enabled = false;
				return;
			}
			// CONTROL as a modifier key enters auto sequence mode
			bool newAuto = (Control.ModifierKeys & Keys.Control) != 0;
			if (newAuto != AutoSeqFiles)
			{
				AutoSeqFiles = newAuto;
				OpenTimer.Enabled = AutoSeqFiles;
				if (!AutoSeqFiles)
					return;
			}
			OpenNextFile();
		}

		/// <summary>
		/// The data format name to use with the clipboard.
		/// </summary>
		static string fmtGlyphName => "GFXGlyph";

		/// <summary>
		/// Process files dropped in the Editor.
		/// </summary>
		private void Editor_DragDrop(object sender, DragEventArgs e)
		{
			Activate();
			String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);
			if (files.Length > 0)
			{
				FilesToOpen ??= new();
				// process the files and validate the extension before adding to list
				foreach (var file in files)
				{
					if (Directory.Exists(file))
					{
						// enumerate all the files in a directory tree
						foreach (var f in Directory.EnumerateFiles(file, "*.*", SearchOption.AllDirectories))
						{
							if (GfxFont.GetLoadFileExtensions().Select(i => i.ext).Contains(Path.GetExtension(f)))
								FilesToOpen.Add(f);
						}
					}
					else if (GfxFont.GetLoadFileExtensions().Select(i => i.ext).Contains(Path.GetExtension(file)))
					{
						// just add one file
						FilesToOpen.Add(file);
					}
				}
				// set automatic sequencing if CTRL is pressed
				AutoSeqFiles = (e.KeyState & 8) == 8;
				// the timer will load the first file
				OpenTimer.Interval = 100;
				OpenTimer.Enabled = true;
			}
		}

		/// <summary>
		/// Provide feedback that dropping files is accepted.
		/// </summary>
		private void Editor_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		/// <summary>
		/// Open the next file from FilesToOpen.
		/// </summary>
		private void OpenTimer_Tick(object sender, EventArgs e)
		{
			OpenTimer.Enabled = false;
			if (!CanFocus || pictureBoxGlyph.Capture)
			{
				// kill the auto sequence if a modal window (e.g. dialog or assertion) is open
				// or if an mouse operation has been initiated
				AutoSeqFiles = false;
				return;
			}
			OpenNextFile();
		}

		/// <summary>
		/// Handle the INSERT command for the listViewGlyphs.
		/// </summary>
		private void insertToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentFont is null)
				return;
			int code;
			if (CurrentGlyph is not null)
				code = CurrentGlyph.Code - 1;
			else
				code = (CurrentFont.Glyphs.LastOrDefault(g => g.Code != 0xFFFF)?.Code ?? -1) + 1;
			code = Math.Max(0, code);
			var glyph = new Glyph([], 0, 0, 0, 0, CurrentFont.MaxAdvance) { Code = (ushort)code };
			int inx = CurrentFont.Add(glyph);
			listViewGlyphs.Items.Insert(inx, NewGlyphItem(glyph));
			SelectGlyph(glyph);
		}

		/// <summary>
		/// Handle the DELETE command for the listViewGlyphs.
		/// </summary>
		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentGlyph is null)
				return;
			// track the lowest item index for new selection
			var inx = listViewGlyphs.Items.Count;
			// remove all selected glyphs from the font and from the listViewGlyphs
			// use SelectedItems and build a list which will survive the removal of items
			foreach (var item in listViewGlyphs.SelectedItems.OfType<ListViewItem>().ToList())
			{
				var glyph = GlyphOfItem(item);
				CurrentFont.Remove(glyph);
				inx = Math.Min(inx, item.Index);
				listViewGlyphs.Items.Remove(item);
			}
			// select the lowest indexed of the removed items
			if (listViewGlyphs.Items.Count != 0)
			{
				if (inx >= listViewGlyphs.Items.Count)
					inx = listViewGlyphs.Items.Count - 1;
				SelectIndex(inx);
			}
			OnChange();
		}

		/// <summary>
		/// Handle the CUT command for the listViewGlyphs.
		/// </summary>
		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// cut is copy and delete
			copyToolStripMenuItem_Click(null, EventArgs.Empty);
			deleteToolStripMenuItem_Click(null, EventArgs.Empty);
		}

		/// <summary>
		/// Handle the COPY command for the listViewGlyphs.
		/// </summary>
		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentGlyph is null)
				return;
			// copy an XML string to the clipboard representing the list of selected glyphs
			var s = Glyph.GlyphsToXmlString(listViewGlyphs.SelectedItems.OfType<ListViewItem>().Select(i => GlyphOfItem(i)));
			// use SetDataObject rather than SetData so that data will
			// remain on the Clipboard after application exit
			DataObject dobj = new(fmtGlyphName, s);
			Clipboard.SetDataObject(dobj, true);
		}

		/// <summary>
		/// Handle the PASTE command for the listViewGlyphs.
		/// </summary>
		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentGlyph is null || !Clipboard.ContainsData(fmtGlyphName))
				return;
			if (Clipboard.ContainsData(fmtGlyphName))
			{
				// build glyphs list from XML string on the clipboard
				var s = (string)Clipboard.GetData(fmtGlyphName);
				var glyphs = Glyph.GlyphsFromXmlString(s);
				if (!glyphs.Any())
					return;
				int inx = 0;
				// Enumerate the list of selected glyphs and set each from glyphs
				// from the clipboard, looping through the clipboad list as needed
				// in this way, for example, a copied glyph can be pasted back into multiple glyphs
				foreach (var g in listViewGlyphs.SelectedItems.OfType<ListViewItem>().Select(i => GlyphOfItem(i)))
				{
					var glyph = glyphs.ElementAt(inx++);
					if (glyph == glyphs.Last())
						inx = 0;
					g.CopyFrom(glyph);
					UpdateGlyphItem(glyph);
				}
				OnChange();
			}
		}

		/// <summary>
		/// Handle the PASTE INSERT command for the listViewGlyphs.
		/// </summary>
		private void pasteInsertToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentFont is null || !Clipboard.ContainsData(fmtGlyphName))
				return;
			if (Clipboard.ContainsData(fmtGlyphName))
			{
				// build glyphs list from XML string on the clipboard
				var s = (string)Clipboard.GetData(fmtGlyphName);
				var glyphs = Glyph.GlyphsFromXmlString(s);
				if (!glyphs.Any())
					return;
				foreach (var glyph in glyphs)
				{
					if (CurrentFont.Glyphs.Any(g => g.Code == glyph.Code))
						glyph.Status = Glyph.States.Error;
					var inx = CurrentFont.Add(glyph);
					listViewGlyphs.Items.Insert(inx, NewGlyphItem(glyph));
				}
				SelectGlyph(glyphs.First());
				OnChange();
			}
		}

		private void DoSelected(Action<Glyph> act)
		{
			// act on all selected glyphs
			foreach (var item in listViewGlyphs.SelectedItems.OfType<ListViewItem>())
			{
				var glyph = GlyphOfItem(item);
				act(glyph);
				UpdateGlyphItem(glyph);
			}
			OnChange();
		}

		/// <summary>
		/// Handle the CLEAR command for the listViewGlyphs.
		/// </summary>
		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.Clear());

		/// <summary>
		/// Handle the SET RECT command for the listViewGlyphs.
		/// </summary>
		private void setRectToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.SetRect(g.xAdvance, CurrentFont.yAdvance));

		/// <summary>
		/// Handle the FILE/NEW command for the listViewGlyphs.
		/// </summary>
		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			NewFile();
		}

		/// <summary>
		/// Handle the FILE/OPEN command for the listViewGlyphs.
		/// </summary>
		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new()
			{
				Filter = BuildFilter(GfxFont.GetLoadFileExtensions(), "All Font Files"),
				FilterIndex = 0,
				Title = "Open Font File"
			};
			if (ofd.ShowDialog() != DialogResult.OK)
				return;
			LoadFile(ofd.FileName);
		}

		/// <summary>
		/// Handle the FILE/SAVE command for the listViewGlyphs.
		/// </summary>
		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFile(CurrentFont.FullPathName);
		}

		/// <summary>
		/// Handle the FILE/SAVE AS command for the listViewGlyphs.
		/// </summary>
		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileAs();
		}

		/// <summary>
		/// Display an about box for the app.
		/// </summary>
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox box = new()
			{
				AppMoreInfo =

			"This program is licensed under the GNU General Public License v3.0:" +
			Environment.NewLine + Environment.NewLine +
			"https://www.gnu.org/licenses/gpl-3.0.en.html"

			+ Environment.NewLine + Environment.NewLine +

			"View the author's GitHub page for the most up-to-date version and more information:" +
			Environment.NewLine + Environment.NewLine +
			"https://github.com/ScottFerg56/GFXFontEditor"

			+ Environment.NewLine + Environment.NewLine +

			"Hundreds of useful bitmap fonts in YAFF format can be found at Rob Hagemans' hoard:" +
			Environment.NewLine + Environment.NewLine +
			"https://github.com/robhagemans/hoard-of-bitfonts"

			+ Environment.NewLine + Environment.NewLine +

			"Adafruit provides a collection of free bitmap fonts in header (.h) form at their GitHub site:" +
			Environment.NewLine + Environment.NewLine +
			"https://github.com/adafruit/Adafruit-GFX-Library/tree/master/Fonts"

			};
			box.ShowDialog(this);
		}

		/// <summary>
		/// Handle the FLATTEN command for the listViewGlyphs.
		/// </summary>
		private void flattenGlyphListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentFont is null)
				return;
			if (GfxFont.CheckFlatness(CurrentFont.Glyphs))
				return;
			var curGlyph = CurrentGlyph;
			var glyphs = GfxFont.FlattenGlyphList(CurrentFont.Glyphs);
			CurrentFont.Truncate(0);
			CurrentFont.AddGlyphs(glyphs);
			RebuildAllGlyphItems();
			SelectGlyph(curGlyph);
			OnChange();
		}

		/// <summary>
		/// Flip the selected glyphs horizontally.
		/// </summary>
		private void flipHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.FlipHorz());

		/// <summary>
		/// Flip the selected glyphs vertically.
		/// </summary>
		private void flipVerticalToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.FlipVert());

		/// <summary>
		/// Rotate the selected glyphs 180 degrees.
		/// </summary>
		private void rotate180ToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.Rotate180());

		/// <summary>
		/// Rotate the selected glyphs 90 degrees CW.
		/// </summary>
		private void rotate90CWToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.Rotate90CW());

		/// <summary>
		/// Rotate the selected glyphs 90 degrees CCW.
		/// </summary>
		private void rotate90CCWToolStripMenuItem_Click(object sender, EventArgs e)
			=> DoSelected(g => g.Rotate90CCW());

		/// <summary>
		/// Escape unprintable characters as unicode hex escape sequences.
		/// </summary>
		/// <param name="c">The character to encode</param>
		/// <returns>The encoded string representation</returns>
		private static string Escape(char c)
		{
			if (c < 0x20 || c >= 0x80)
				return $"\\u{(int)c:X4}";
			if (c == '\\')
				return @"\\";
			return c.ToString();
		}

		/// <summary>
		/// Add the selected glyph to the end of the sample text
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void addToSampleTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			toolStripTextBoxFVText.Text += Escape((char)CurrentGlyph.Code);
		}

		private void editFontPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CurrentFont is null)
				return;
			FontProperties.Edit(ref CurrentFont.Properties);
		}

		private void toolStripDropDownButtonFile_DropDownOpening(object sender, EventArgs e)
		{
			var enable = CurrentFont is not null;
			saveToolStripMenuItem.Enabled = enable;
			saveAsToolStripMenuItem.Enabled = enable;
			editFontPropertiesToolStripMenuItem.Enabled = enable;
		}
	}

	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
	public class ToolStripNumberControl : ToolStripControlHost
	{
		public ToolStripNumberControl() : base(new NumericUpDown()) { }
	}
}
