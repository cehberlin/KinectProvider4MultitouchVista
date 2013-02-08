using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace KinectProvider
{
    /// <summary>
    /// UI Cursor implementation which is painting on a windows handle
    /// this is also working with Win8 Startscreen
    /// Best results with bitmaps without transparent area --> squares
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class Cursor: ICursor
    {
        /// <summary>
        /// Rectangle struct for accessing User32 functions
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hwnd, [MarshalAs(UnmanagedType.Struct)] ref RECT lpRect, bool bErase);

        /// <summary>
        /// Save the old cursor point for invalidation
        /// </summary>
        private Point oldPoint = new Point();

        /// <summary>
        /// The current location
        /// </summary>
        private Point currentPoint = new Point();

        /// <summary>
        /// Cursor bitmap
        /// </summary>
        private Bitmap bitmap;

        /// <summary>
        /// Save rectangle struct to avoid repainting
        /// </summary>
        private RECT oldRectangle = new RECT();

        /// <summary>
        /// The desktop graphics instance
        /// </summary>
        private Graphics desktopG;

        /// <summary>
        /// Count how often we have repainted current position
        /// </summary>
        private int paintCounter = 0;

        /// <summary>
        /// Update graphics thread
        /// </summary>
        private Thread updateThread;

        private Boolean shouldRun;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitmap"> Initial cursor bitmap</param>
        public Cursor(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            this.desktopG = Graphics.FromHwnd(IntPtr.Zero);
            shouldRun = true;
            updateThread = new Thread(update);
            updateThread.Start();
        }

        /// <summary>
        /// Update thread routine
        /// </summary>
        private void update()
        {
            while (shouldRun)
            {
                updateGraphics(currentPoint);
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Update pointer location
        /// </summary>
        /// <param name="point">new point location</param>
        public void Show(Point point)
        {            
            currentPoint = point;
        }

        private void updateGraphics(Point point)
        {
            if (point.X == oldPoint.X && point.Y == oldPoint.Y)
            {
                //if we don't do this, the cursor will be hidden after stopping movement -- probably because of the paint update cycle
                if (paintCounter < 5)
                {
                    desktopG.DrawImage(bitmap, point.X - bitmap.Width / 2, point.Y - bitmap.Height / 2);
                }
                paintCounter++;

                return;
            }

            //reset counter on movement
            paintCounter = 0;

            //invalidate old location
            oldRectangle.left = oldPoint.X - bitmap.Width / 2;
            oldRectangle.top = oldPoint.Y - bitmap.Height / 2;
            oldRectangle.right = oldRectangle.left + bitmap.Width;
            oldRectangle.bottom = oldRectangle.top + bitmap.Height;

            unsafe
            {
                InvalidateRect(IntPtr.Zero, ref oldRectangle, false);
            }

            //draw new location
            desktopG.DrawImage(bitmap, point.X - bitmap.Width / 2, point.Y - bitmap.Height / 2);

            //save current point location
            oldPoint = point;
        }

        /// <summary>
        /// Set and get current cursor bitmap
        /// Bitmap should have as less transparent space on the outline as possible
        /// </summary>
        /// <param name="bitmap">the new bitmap</param>
        public void setBitmap(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void Dispose()
        {
            shouldRun = false;
            updateThread.Join();
        }
    }
}
