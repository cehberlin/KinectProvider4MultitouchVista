using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using KinectProvider;

namespace KinectProvider
{
    public enum InputStatus { UNKNOWN, TOUCHED, CURSOR };

    /// <summary>
    /// One user hand, holding data as well as cursor representation
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class Hand : IDisposable
    {
        ICursor cursor;
      
        InputStatus currentStatus = InputStatus.UNKNOWN;

        HandContact currentContact = null;

        System.Windows.Point position = new System.Windows.Point();

        //Another possible Lowpass -> uncomment code below as well
        //PointLowPass pointFilter = new PointLowPass(0.07f);

        private Bitmap[] cursorImage = new Bitmap[3];

        InputStatus status = InputStatus.UNKNOWN;


        const int FREQUENCY = 350;
        const int CUTOFF = 15;
        Butterworth xFilter;
        Butterworth yFilter;

        Boolean firstUpdate = true;

        public System.Windows.Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public InputStatus Status
        {
            get { return currentStatus; }
            set { currentStatus = value; }
        }

        public Hand(Color color)
        {
            cursorImage[(int)InputStatus.CURSOR] = getColoredImage(Resources.DiscImage, color);
            cursorImage[(int)InputStatus.UNKNOWN] = getColoredImage(Resources.DiscImage, color);
            cursorImage[(int)InputStatus.TOUCHED] = getColoredImage(Resources.DiscImageFilled, color);
            cursor = CursorFactory.getCursor(cursorImage[(int)status]);
        }

        /// <summary>
        /// Colorize the image
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private Bitmap getColoredImage(Bitmap bitmap, Color color)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Width; y++)
                {
                    Color current = bitmap.GetPixel(x, y);
                    double r = current.R + (1 - current.R / 255.0) * color.R;
                    double g = current.G + (1 - current.G / 255.0) * color.G;
                    double b = current.B + (1 - current.B / 255.0) * color.B;
                    newBitmap.SetPixel(x, y, Color.FromArgb((int)current.A, (int)r, (int)g, (int)b));
                }
            }
            return newBitmap;
        }

        /// <summary>
        /// Update state and position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public HandContact update(float x, float y, InputStatus newStatus)
        {
            updatePosition(x, y);

            newStatus = updateStatus(newStatus);
            cursor.Show(new System.Drawing.Point((int)position.X, (int)position.Y));

            return currentContact;
        }

        /// <summary>
        /// Update just state
        /// </summary>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public HandContact update(InputStatus newStatus)
        {
            newStatus = updateStatus(newStatus);
            cursor.Show(new System.Drawing.Point((int)position.X, (int)position.Y));

            return currentContact;
        }

        /// <summary>
        /// Update the status, means contact for driver as well as the cursor
        /// </summary>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        private InputStatus updateStatus(InputStatus newStatus)
        {
            //generate contact for virtual touch driver
            if ((currentStatus == InputStatus.UNKNOWN || currentStatus == InputStatus.CURSOR) && newStatus == InputStatus.TOUCHED)
            {
                currentContact = new HandContact(position);
            }
            else if (currentStatus == InputStatus.TOUCHED)
            {
                currentContact.Update(position, newStatus);
            }
            else
            {
                newStatus = InputStatus.CURSOR;
                if (currentContact != null)
                {
                    currentContact.Update(position, newStatus);
                }
            }

            //set cursor
            if (currentStatus != newStatus)
            {
                currentStatus = newStatus;
                cursor.setBitmap( cursorImage[(int)newStatus]);
            }
            return newStatus;
        }

        /// <summary>
        /// Just proceeds and filters the position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void updatePosition(float x, float y)
        {
            //pointFilter.filter(x, y);
            //x = pointFilter.X;
            //y = pointFilter.Y;

            if (firstUpdate)
            {
                xFilter = new Butterworth(FREQUENCY, CUTOFF, x);
                yFilter = new Butterworth(FREQUENCY, CUTOFF, y);
                firstUpdate = false;
            }
            else
            {
                x = (float)xFilter.filter(x);
                y = (float)yFilter.filter(y);
            }


            position.X = x;
            position.Y = y;
        }

        public void Dispose()
        {
            cursor.Dispose();
        }
    }
}
