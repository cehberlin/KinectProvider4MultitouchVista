using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectProvider
{
    /// <summary>
    /// Very simple low pass implementation for point coordinates
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class PointLowPass
    {
        float storeX = 0;

        float storeY = 0;

        float smoothing;

        Boolean init = true;

        public PointLowPass(float smoothing)
        {
            this.smoothing = smoothing;
        }

        public void filter(float inputX, float inputY)
        {
            if (init)
            {
                storeX = inputX;
                storeY = inputY;
                init = false;
            }
            else
            {
                storeX = storeX + (inputX - storeX) * smoothing;
                storeY = storeY + (inputY - storeY) * smoothing;
            }
        }

        public float X
        {
            get { return storeX; }
            set { storeX = value; }
        }

        public float Y
        {
            get { return storeY; }
            set { storeY = value; }
        }
    }
}
