using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Drawing.Imaging;

namespace CrashReporterWPF.Net
{
    class CaptureScreenshot
    {

        /// <summary>
        /// Captures the screen.
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Captures the window.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        public System.Drawing.Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            var hdcSrc = User32.GetWindowDC(handle);
            // get the size
            var windowRect = new User32.Rect();
            User32.GetWindowRect(handle, ref windowRect);
            var width = windowRect.right - windowRect.left;
            var height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            var hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            var hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            var hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, Gdi32.Srccopy);
            // restore selection
            Gdi32.SelectObject(hdcDest, hOld);
            // clean up
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            System.Drawing.Image img = System.Drawing.Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            Gdi32.DeleteObject(hBitmap);
            return img;
        }


        /// <summary>
        /// Capture screenshot of a window
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="format">The format.</param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            System.Drawing.Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }


        /// <summary>
        /// Capture desktop screenshot
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="format">The format.</param>
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            var img = CaptureScreen();
            img.Save(filename, format);
        }

        private static class Gdi32
        {
            public const int Srccopy = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDc);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDc);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);
        }

        private static class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public readonly int left;
                public readonly int top;
                public readonly int right;
                public readonly int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
        }
    }
}
