using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageLib {
	public struct ColoredRectangle {
		public ColoredRectangle(int x, int y, int width, int height, FastColor color) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
			Color = color;
		}
		public readonly int X, Y, Width, Height;
		public readonly FastColor Color;

		public int Left { get { return X; } }
		public int Right { get { return X + Width; } }
		public int Top { get { return Y; } }
		public int Bottom { get { return Y + Height; } }
	}

	public class CandidateImage {
		private readonly ColoredRectangle[] m_Rectangles;

		public CandidateImage() {
			m_Rectangles = new ColoredRectangle[0];
		}
		public CandidateImage(ColoredRectangle[] rectangles) {
			m_Rectangles = rectangles;
		}

		public ColoredRectangle[] Rectangles { get { return m_Rectangles; } }
		public float Fitness { get; set; }

		public FastColor GetPixel(int x, int y) {
			var c = FastColor.Black;
			for (int i = 0; i < m_Rectangles.Length; i++) {
				var h = m_Rectangles[i];
				if (x >= h.X && y >= h.Y && x < h.X + h.Width && y < h.Y + h.Height)
					c = h.Color.Blend(c);
			}
			return c;
		}
	}

	//public class ImageRenderer {
	//    public int Width { get; set; }
	//    public int Height { get; set; }

	//    public Bitmap Render(CandidateImage image) {
	//        var bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
	//        using (var g = Graphics.FromImage(bitmap)) {
	//            g.Clear(Color.Black);
	//            foreach (var r in image.Rectangles) {
	//                using (var brush = new SolidBrush(r.Color)) {
	//                    g.FillRectangle(brush, new Rectangle(r.X, r.Y, r.Width, r.Height));
	//                }
	//            }
	//        }
	//        return bitmap;
	//    }
	//}

	//public class ImageFitnessComparer {
	//    public double CalculateFitness(DisposableBitmapData obd, DisposableBitmapData cbd) {
	//        double error = 0;
	//        if (obd.Width != cbd.Width) throw new ArgumentException();
	//        if (obd.Height != cbd.Height) throw new ArgumentException();

	//        unsafe {
	//            for (int h = 0; h < obd.Height; h++) {
	//                var origRow = (byte*)(obd.Scan0 + h * obd.Stride);
	//                var candRow = (byte*)(cbd.Scan0 + h * cbd.Stride);

	//                for (int w = 0; w < obd.Width; w++) {
	//                    var origPixel = ReadPixel(origRow, w, obd.PixelFormat);
	//                    var candPixel = ReadPixel(candRow, w, cbd.PixelFormat);

	//                    error += 0.0
	//                        + (origPixel.R - candPixel.R) * (origPixel.R - candPixel.R)
	//                        + (origPixel.G - candPixel.G) * (origPixel.G - candPixel.G)
	//                        + (origPixel.B - candPixel.B) * (origPixel.B - candPixel.B)
	//                        ;
	//                }
	//            }
	//        }
	//        return 1 - error;
	//    }
	//    private unsafe static Color ReadPixel(byte* row, int x, PixelFormat pixelFormat) {
	//        switch (pixelFormat) {
	//            case PixelFormat.Format24bppRgb:
	//                return Color.FromArgb(
	//                    row[x * 3 + 2],
	//                    row[x * 3 + 1],
	//                    row[x * 3 + 0]);

	//            case PixelFormat.Format32bppArgb:
	//                return Color.FromArgb(
	//                    row[x * 4 + 3],
	//                    row[x * 4 + 2],
	//                    row[x * 4 + 1],
	//                    row[x * 4 + 0]);

	//            default:
	//                throw new ArgumentOutOfRangeException(string.Format("Pixel format {0} not supported.", pixelFormat));
	//        }
	//    }
	//}

	public static class GraphicExtensions {
		public static DisposableBitmapData LockBits(this Bitmap bitmap, ImageLockMode lockMode = ImageLockMode.ReadOnly) {
			var bitmapData = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				lockMode,
				bitmap.PixelFormat);
			return new DisposableBitmapData(bitmap, bitmapData);
		}
	}
	public class DisposableBitmapData : IDisposable {
		private readonly Bitmap m_Bitmap;
		private readonly BitmapData m_BitmapData;
		private readonly IntPtr m_Scan0;
		private readonly int m_Stride, m_Width, m_Height;
		private readonly PixelFormat m_PixelFormat;

		public DisposableBitmapData(Bitmap bitmap, BitmapData bitmapData) {
			m_Bitmap = bitmap;
			m_BitmapData = bitmapData;
			m_Scan0 = m_BitmapData.Scan0;
			m_Stride = m_BitmapData.Stride;
			m_Width = m_BitmapData.Width;
			m_Height = m_BitmapData.Height;
			m_PixelFormat = m_BitmapData.PixelFormat;
		}

		public int Height { get { return m_Height; } }
		public int Width { get { return m_Width; } }
		public PixelFormat PixelFormat { get { return m_BitmapData.PixelFormat; } }

		public IntPtr Scan0 { get { return m_Scan0; } }
		public int Stride { get { return m_Stride; } }

		public unsafe FastColor GetPixel(int x, int y) {
			var row = (byte*)(m_Scan0 + y * m_Stride);
			switch (m_PixelFormat) {
				case PixelFormat.Format24bppRgb:
					return new FastColor(
						row[x * 3 + 2],
						row[x * 3 + 1],
						row[x * 3 + 0]);

				case PixelFormat.Format32bppArgb:
					return new FastColor(
						//row[x * 4 + 3],
						row[x * 4 + 2],
						row[x * 4 + 1],
						row[x * 4 + 0]);

				default:
					throw new ArgumentOutOfRangeException(string.Format("Pixel format {0} not supported.", m_BitmapData.PixelFormat));
			}
		}

		void IDisposable.Dispose() {
			m_Bitmap.UnlockBits(m_BitmapData);
		}
	}

	public class MemoryBitmap : IDisposable {
		private readonly FastColor[] m_Pixels;
		private readonly int m_Width, m_Height;

		private MemoryBitmap(int width, int height) {
			m_Pixels = new FastColor[width * height];
			m_Width = width;
			m_Height = height;
		}
		public void Clear() {
			for (int i = 0; i < m_Pixels.Length; i++) {
				m_Pixels[i] = FastColor.Black;
			}
		}

		private static readonly Dictionary<Tuple<int, int>, Stack<MemoryBitmap>> m_ObjectPool = new Dictionary<Tuple<int, int>, Stack<MemoryBitmap>>();
		private static readonly object m_ObjectPoolGuard = new object();

		public static MemoryBitmap Create(int width, int height) {
			var key = Tuple.Create(width, height);
			MemoryBitmap bitmap = null;
			lock (m_ObjectPoolGuard) {
				Stack<MemoryBitmap> bucket;
				if (!m_ObjectPool.TryGetValue(key, out bucket))
					m_ObjectPool.Add(key, bucket = new Stack<MemoryBitmap>());
				if (bucket.Count > 0) bitmap = bucket.Pop();
			}
			if (bitmap == null) bitmap = new MemoryBitmap(width, height);
			bitmap.Clear();
			return bitmap;
		}
		void IDisposable.Dispose() {
			var key = Tuple.Create(m_Width, m_Height);
			lock (m_ObjectPoolGuard) {
				Stack<MemoryBitmap> bucket;
				if (!m_ObjectPool.TryGetValue(key, out bucket))
					m_ObjectPool.Add(key, bucket = new Stack<MemoryBitmap>());
				bucket.Push(this);
			}
		}

		public int Width { get { return m_Width; } }
		public int Height { get { return m_Height; } }

		public FastColor GetPixel(int x, int y) {
#if DEBUG
			if (x >= Width || x < 0) throw new ArgumentOutOfRangeException("x");
			if (y >= Height || y < 0) throw new ArgumentOutOfRangeException("y");
#endif
			return m_Pixels[y * Width + x];
		}
		public void SetPixel(int x, int y, FastColor c) {
#if DEBUG
			if (x >= Width || x < 0) throw new ArgumentOutOfRangeException("x");
			if (y >= Height || y < 0) throw new ArgumentOutOfRangeException("y");
#endif
			m_Pixels[y * Width + x] = c;
		}
	}

	public struct FastColor : IEquatable<FastColor> {
		public FastColor(byte alpha, byte red, byte green, byte blue) {
			A = alpha;
			R = (byte)(red * alpha / 255);
			G = (byte)(green * alpha / 255);
			B = (byte)(blue * alpha / 255);
		}
		public FastColor(byte red, byte green, byte blue) {
			A = 255;
			R = red;
			G = green;
			B = blue;
		}

		public readonly byte A;
		public readonly byte R;
		public readonly byte G;
		public readonly byte B;

		private static byte ToByte(int i) {
			return i > 255 ? (byte)255 : (byte)i;
		}
		public FastColor Blend(FastColor b) {
			//Assuming's b's alpha is 255
			byte aBlend = (byte)((255 - this.A));
			return new FastColor(
				(byte)(this.R + b.R * aBlend / 255),
				(byte)(this.G + b.G * aBlend / 255),
				(byte)(this.B + b.B * aBlend / 255));
		}

		public override string ToString() { return string.Format("A:{0} R:{1} G:{2} B:{3}", A, R, G, B); }
		public Color ToColor() { return Color.FromArgb(A, R * 255 / A, G * 255 / A, B * 255 / A); }
		public bool Equals(FastColor other) { return A == other.A && R == other.R && G == other.G && B == other.B; }

		public static readonly FastColor Black = new FastColor(0, 0, 0);
		public static readonly FastColor White = new FastColor(255, 255, 255);
	}
}
