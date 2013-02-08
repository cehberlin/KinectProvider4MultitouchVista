using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace KinectProvider
{
    class CursorFactory
    {

        public static ICursor getCursor(Bitmap defaultCursor)
        {
            //if (true && isSingleScreen())//if (runOnAtLeastWin8())
            //{
            //    //Another possible cursor, which will even work on Windows 8 Startscreen, but less nice looking if you are not using squared cursors
            //    //the Win7 compatible cursor is more performant
            //    //moreover it is only working on one screen!!!
            //    return new Cursor(defaultCursor);
            //}
            //else
            //{
                return new DebugCursor(defaultCursor);
            //}
        }

        private static Boolean isSingleScreen()
        {
            return Screen.AllScreens.GetUpperBound(0) == 0;
        }

        private static Boolean runOnAtLeastWin8()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Win8 is 6.2
            return vs.Major > 5 && vs.Minor > 1; 
        } 
    }
}
