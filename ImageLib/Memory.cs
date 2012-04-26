using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ImageLib {
	/// <summary>
	/// From: http://msdn.microsoft.com/en-us/library/aa664786(VS.71).aspx
	/// </summary>
	public unsafe static class UnmanagedMemory {
		// Handle for the process heap. This handle is used in all calls to the
		// HeapXXX APIs in the methods below.
		private static readonly int ph = GetProcessHeap();

		/// <summary>
		/// Allocates a memory block of the given size. The allocated memory is automatically initialized to zero.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static IntPtr Alloc(int size) {
			void* result = HeapAlloc(ph, HEAP_ZERO_MEMORY, size);
			if (result == null) throw new OutOfMemoryException();
			return new IntPtr(result);
		}
		
		/// <summary>
		/// Copies count bytes from src to dst. The source and destination blocks are permitted to overlap.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="count"></param>
		public static void Copy(IntPtr src, IntPtr dst, int count) {
			//byte* ps = (byte*)src.ToPointer();
			//byte* pd = (byte*)dst.ToPointer();
			//if (ps > pd) {
			//    for (; count != 0; count--) *pd++ = *ps++;
			//} else if (ps < pd) {
			//    for (ps += count, pd += count; count != 0; count--) *--pd = *--ps;
			//}
			memcpy(dst, src, new UIntPtr((uint)count));
		}
		
		/// <summary>
		/// Frees a memory block.
		/// </summary>
		/// <param name="block"></param>
		public static void Free(IntPtr block) {
			if (!HeapFree(ph, 0, block.ToPointer())) throw new InvalidOperationException();
		}

		/// <summary>
		/// Re-allocates a memory block. If the reallocation request is for a larger size, the additional region of memory is automatically initialized to zero.
		/// </summary>
		/// <param name="block"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static IntPtr ReAlloc(IntPtr block, int size) {
			void* result = HeapReAlloc(ph, HEAP_ZERO_MEMORY, block.ToPointer(), size);
			if (result == null) throw new OutOfMemoryException();
			return new IntPtr(result);
		}

		/// <summary>
		/// Returns the size of a memory block.
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public static int SizeOf(IntPtr block) {
			int result = HeapSize(ph, 0, block.ToPointer());
			if (result == -1) throw new InvalidOperationException();
			return result;
		}

		// Heap API flags
		private const int HEAP_ZERO_MEMORY = 0x00000008;
		
		// Heap API functions
		[DllImport("kernel32")]
		static extern int GetProcessHeap();
		[DllImport("kernel32")]
		static extern void* HeapAlloc(int hHeap, int flags, int size);
		[DllImport("kernel32")]
		static extern bool HeapFree(int hHeap, int flags, void* block);
		[DllImport("kernel32")]
		static extern void* HeapReAlloc(int hHeap, int flags, void* block, int size);
		[DllImport("kernel32")]
		static extern int HeapSize(int hHeap, int flags, void* block);

		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
	}
}
