using System;
using System.Threading;
using Multitouch.Contracts;
using System.Windows;

namespace KinectProvider
{
    /// <summary>
    /// Class for encapsulating hand contact informations
    /// basically generated contacts based on inputstatus
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class HandContact : Contact, ICloneable
    {
        private static int idCounter = 0;

        const int height = 10;
        const int width = 10;

        /// <summary>
        /// Copy contact
        /// </summary>
        /// <param name="clone"></param>
        public HandContact(HandContact clone)
            : base(clone.Id, clone.State, clone.Position, clone.MajorAxis, clone.MinorAxis)
        {
            Orientation = clone.Orientation;
        }

        /// <summary>
        /// Create new contact
        /// </summary>
        /// <param name="position"></param>
        public HandContact(Point position)
            : base(idCounter, ContactState.New, position, width, height)
        {
            Interlocked.Increment(ref idCounter);

            Orientation = 0;
        }

        /// <summary>
        /// Update contact
        /// </summary>
        /// <param name="position"></param>
        /// <param name="status"></param>
        internal void Update(Point position, InputStatus status)
        {
            Position = position;

            if (status == InputStatus.TOUCHED)
            {
                State = ContactState.Moved;
            }
            else
            {
                State = ContactState.Removed;
            }
        }

        public override string ToString()
        {
            return string.Format("ID: {0}, Position: {1}, State: {2}, Handle: {3}", Id, Position, State);
        }

        public object Clone()
        {
            return new HandContact(this);
        }
    }
}