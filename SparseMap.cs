namespace GFXFontEditor
{
	/// <summary>
	/// A class to maintain a set of points to support a sparse bitmap.
	/// </summary>
	public class SparseMap
	{
		/// <summary>
		/// The points in the sparse map.
		/// </summary>
		protected List<Point> Points = new();

		/// <summary>
		/// SparseMap constructor.
		/// </summary>
		/// <param name="data">Input data representing a minimal bitmap</param>
		/// <param name="width">The width of the data in the bitmap</param>
		/// <param name="height">The height of the data in the bitmap</param>
		/// <remarks>
		/// See @SetData for an explanation of the data format.
		/// </remarks>
		public SparseMap(IEnumerable<byte> data, int width, int height)
		{
			SetData(data, width, height);
		}

		/// <summary>
		/// Set the points for the SparseMap from a bitmap.
		/// </summary>
		/// <param name="data">Input data representing a minimal bitmap</param>
		/// <param name="width">The width of the data in the bitmap</param>
		/// <param name="height">The height of the data in the bitmap</param>
		protected void SetData(IEnumerable<byte> data, int width, int height)
		{
			// https://glenviewsoftware.com/projects/products/adafonteditor/adafruit-gfx-font-format/
			// x loops thru each row y
			// the mask moves seamlessly thru the bitmap data one bit at a time,
			// high order bit first, until all bytes are exhausted
			Points.Clear();
			byte mask = 0x80;
			int inx = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if ((data.ElementAt(inx) & mask) != 0)
						Set(x, y);
					mask >>= 1;
					if (mask == 0)
					{
						mask = 0x80;
						++inx;
					}
				}
			}
		}

		/// <summary>
		/// Clear the cached informational data for Bounds and Data bitmap.
		/// </summary>
		protected void ClearCache()
		{
			_Bounds = Rectangle.Empty;
			_Data = Array.Empty<byte>();
		}

		protected Rectangle _Bounds;
		/// <summary>
		/// The Bounds is a tight rectangle surrounding the cloud of Point data.
		/// </summary>
		public Rectangle Bounds
		{
			get
			{
				if (_Bounds.IsEmpty && Points.Any())
				{
					_Bounds = new Rectangle(Points[0].X, Points[0].Y, 1, 1);
					foreach (var pt in Points)
					{
						_Bounds = Rectangle.Union(_Bounds, new Rectangle(pt.X, pt.Y, 1, 1));
					}
				}
				return _Bounds;
			}
		}

		/// <summary>
		/// Get the state of the pixel at a given coordinate.
		/// </summary>
		/// <param name="x">X coordinate of the desired pixel</param>
		/// <param name="y">Y coordinate of the desired pixel</param>
		/// <returns>True iff the Point at that coordinate is set</returns>
		public bool Get(int x, int y)
		{
			return Points.Contains(new Point(x, y));
		}

		/// <summary>
		/// Set the state of the pixel at a given coordinate.
		/// </summary>
		/// <param name="x">X coordinate of the desired pixel</param>
		/// <param name="y">Y coordinate of the desired pixel</param>
		public void Set(int x, int y)
		{
			if (!Get(x, y))
			{
				Points.Add(new Point(x, y));
				ClearCache();
			}
		}

		/// <summary>
		/// Clear the state of the pixel at a given coordinate.
		/// </summary>
		/// <param name="x">X coordinate of the desired pixel</param>
		/// <param name="y">Y coordinate of the desired pixel</param>
		public void Clear(int x, int y)
		{
			if (Get(x, y))
			{
				Points.Remove(new Point(x, y));
				ClearCache();
			}
		}

		/// <summary>
		/// Toggle the state of the pixel at a given coordinate.
		/// </summary>
		/// <param name="x">X coordinate of the desired pixel</param>
		/// <param name="y">Y coordinate of the desired pixel</param>
		public void Toggle(int x, int y)
		{
			if (Get(x,y))
				Clear(x, y);
			else
				Set(x, y);
		}

		/// <summary>
		/// Clear the entire SparseMap of pixels.
		/// </summary>
		public void Clear()
		{
			Points.Clear();
			ClearCache();
		}

		/// <summary>
		/// Apply an offset to every set pixel.
		/// </summary>
		/// <param name="x">X offset for each pixel</param>
		/// <param name="y">Y offset for each pixel</param>
		public void Offset(int x, int y)
		{
			List<Point> pts = new();
			foreach (Point p in Points)
			{
				p.Offset(x, y);
				pts.Add(p);
			}
			Points = pts;
			ClearCache();
		}

		protected byte[] _Data = Array.Empty<byte>();
		/// <summary>
		/// Returns the data bitmap representing the SparseMap.
		/// </summary>
		/// <returns>A bitmap, tightly bound on the set pixels</returns>
		public IReadOnlyCollection<byte> GetData()
		{
			if (_Data.Length == 0)
			{
				_Data = new byte[(int)Math.Ceiling((Bounds.Width * Bounds.Height) / 8.0)];
				foreach (var pt in Points)
				{
					int bit = (pt.Y - Bounds.Top) * Bounds.Width + (pt.X - Bounds.Left);
					int inx = bit / 8;
					int slot = bit % 8;
					byte mask = (byte)(0x80 >> slot);
					_Data[inx] |= mask;
				}
			}
			return Array.AsReadOnly(_Data);
		}
	}
}
