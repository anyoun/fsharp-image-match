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
	}

	public class CandidateImage {
		private readonly IList<ColoredRectangle> m_Rectangles;

		public CandidateImage() {
			m_Rectangles = new ColoredRectangle[0];
		}
		public CandidateImage(IList<ColoredRectangle> rectangles) {
			m_Rectangles = rectangles;
		}

		public IList<ColoredRectangle> Rectangles { get { return m_Rectangles; } }
		public float Fitness { get; set; }
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

		public DisposableBitmapData(Bitmap bitmap, BitmapData bitmapData) {
			m_Bitmap = bitmap;
			m_BitmapData = bitmapData;
		}

		public int Height { get { return m_BitmapData.Height; } }
		public int Width { get { return m_BitmapData.Width; } }
		public PixelFormat PixelFormat { get { return m_BitmapData.PixelFormat; } }

		public IntPtr Scan0 { get { return m_BitmapData.Scan0; } }
		public int Stride { get { return m_BitmapData.Stride; } }

		public unsafe FastColor GetPixel(int x, int y) {
			var row = (byte*)(m_BitmapData.Scan0 + y * Stride);
			switch (m_BitmapData.PixelFormat) {
				case PixelFormat.Format24bppRgb:
					return new FastColor(
						255,
						row[x * 3 + 2],
						row[x * 3 + 1],
						row[x * 3 + 0]);

				case PixelFormat.Format32bppArgb:
					return new FastColor(
						row[x * 4 + 3],
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

	//public unsafe class MemoryBitmap {
	//    private readonly IntPtr m_Data;
	//    private readonly int m_Width, m_Height, m_Stride;

	//    public MemoryBitmap(int width, int height) {
	//        m_Data = UnmanagedMemory.Alloc(width * height * 4);
	//        m_Width = width;
	//        m_Stride = width;
	//        m_Height = height;
	//    }

	//    public static MemoryBitmap FromBitmap(Bitmap b) {
	//        using (var d = b.LockBits()) {

	//            var memBitmap = new MemoryBitmap(d.Width, d.Height);

	//            for (int h = 0; h < d.Height; h++) {
	//                var src = (d.Scan0 + h * d.Stride);
	//                var dst = (memBitmap.m_Data + h * memBitmap.Stride);
	//                UnmanagedMemory.Copy(src, dst, d.Width);
	//            }

	//            return memBitmap;
	//        }
	//    }

	//    public int Width { get { return m_Width; } }
	//    public int Stride { get { return m_Stride; } }
	//    public int Height { get { return m_Height; } }

	//    public Color GetPixel(int x, int y) {
	//        var row = (byte*)(m_Data + y * Stride);
	//        return Color.FromArgb(
	//                    row[x * 4 + 3],
	//                    row[x * 4 + 2],
	//                    row[x * 4 + 1],
	//                    row[x * 4 + 0]);
	//    }
	//    public void SetPixel(int x, int y, Color c) {
	//        var row = (byte*)(m_Data + y * Stride);
	//        row[x * 4 + 3] = c.A;
	//        row[x * 4 + 2] = c.R;
	//        row[x * 4 + 1] = c.G;
	//        row[x * 4 + 0] = c.B;
	//    }

	//    void Finalize() {
	//        UnmanagedMemory.Free(m_Data);
	//    }
	//}

	public struct FastColor {
		private readonly Int32 Value;
		public FastColor(byte alpha, byte red, byte green, byte blue) {
			Value = alpha << 24 | red << 16 | green << 8 | blue;
		}

		public byte A { get { return (byte)(Value >> 24 & 255); } }
		public byte R { get { return (byte)(Value >> 16 & 255); } }
		public byte G { get { return (byte)(Value >> 8 & 255); } }
		public byte B { get { return (byte)(Value >> 0 & 255); } }

		public FastColor Blend(FastColor b) {
			return new FastColor(
				(byte)(this.A + b.A * (1 - this.A)),
				(byte)(this.A * this.R + b.A * b.R * ((byte)1 - this.A)),
				(byte)(this.A * this.G + b.A * b.G * ((byte)1 - this.A)),
				(byte)(this.A * this.B + b.A * b.B * ((byte)1 - this.A)));
		}

		public static readonly FastColor Black = new FastColor(255, 0, 0, 0);
		public static readonly FastColor White = new FastColor(255, 255, 255, 255);

		public Color ToColor() {
			return Color.FromArgb(Value);
		}
	}
}
