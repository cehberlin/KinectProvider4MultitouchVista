using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNI;
using System.Diagnostics;
using System.Drawing;

namespace KinectProvider
{
    /// <summary>
    /// One user with two hands
    /// keeping track of color and so on
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class User : IDisposable
    {
        int id;

        /// <summary>
        /// Different colors for different users
        /// </summary>
        static readonly Color[] USERCOLOR = { Color.Blue, Color.Red, Color.Green, Color.Violet,
                                                Color.Yellow, Color.Brown, Color.Cyan, Color.Magenta };

        /// <summary>
        /// Id this user
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Current user joint setup
        /// </summary>
        Dictionary<SkeletonJoint, SkeletonJointPosition> joints;

        public Dictionary<SkeletonJoint, SkeletonJointPosition> Joints
        {
            get { return joints; }
            set { joints = value; }
        }

        /// <summary>
        /// User hands (usally two)
        /// </summary>
        Hand[] hands;

        public User(int id, Dictionary<SkeletonJoint, SkeletonJointPosition> joints)
        {
            this.id = id;
            this.joints = joints;

            hands = new Hand[2];

            //init hands, generate color based on id
            for (int i = 0; i < hands.Length; i++)
            {
                hands[i] = new Hand(USERCOLOR[id % USERCOLOR.Length]);
            }
        }

        /// <summary>
        /// Update specific hand
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="hand"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public HandContact updateHand(Point3D pos, uint hand, InputStatus status)
        {
            if (hand > 1)
            {
                hand = 1;
            }

            return hands[hand].update(pos.X, pos.Y, status);
        }

        /// <summary>
        /// Update just hand state
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public HandContact updateHand(uint hand, InputStatus status)
        {
            if (hand > 1)
            {
                hand = 1;
            }

            return hands[hand].update(status);
        }

        public void Dispose()
        {
            for (int i = 0; i < hands.Length; i++)
            {
                hands[i].Dispose();
            }
        }
    }
}
