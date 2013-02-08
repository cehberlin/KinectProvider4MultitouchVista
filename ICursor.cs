using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KinectProvider
{
    interface ICursor: IDisposable
    {
        void setBitmap(Bitmap bitmap);

        void Show(Point point);

    }
}
