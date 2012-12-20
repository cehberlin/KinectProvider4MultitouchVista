using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ManagedWinapi.Hooks;
using ManagedWinapi.Windows;
using OpenNI;
using System.Diagnostics;
using KinectProvider;
using KinectProvider.Properties;

namespace KinectProvider
{
    /// <summary>
    /// This class manages all data capturing and setup for getting imagininary/virtual touchinput from a
    /// kinect by usind NITE and OPENNI
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    class KinectManager : IDisposable
    {
        private readonly string CONFIG_XML_FILE = @"OpenNIConfig.xml";
        private readonly InputProvider inputProvider;

        /// <summary>
        /// Offset of the virtual touchscreen relative to the user
        /// Only used if relativeTouchDetectionEnabled = true
        /// </summary>
        private int virtualScreenZRelativeOffset = 0;

        /// <summary>
        ///   /// <summary>
        /// Offset of the virtual touchscreen absolute to the sensor
        /// Only used if relativeTouchDetectionEnabled = false
        /// </summary>
        /// </summary>
        private int virtualScreenZAbsoluteOffset = 0;

        /// <summary>
        /// Select if relative user or absolute virtual touchscreen mode
        /// </summary>
        private bool relativeTouchDetectionEnabled = true;

        //Kinect/OpenNi handling objects
        private Context context;
        private ScriptNode scriptNode;
        private DepthGenerator depth;
        private UserGenerator userGenerator;
        private SkeletonCapability skeletonCapbility;
        private PoseDetectionCapability poseDetectionCapability;
        private string calibPose;

        /// <summary>
        /// Thread for processing te kinect data
        /// </summary>
        private Thread readerThread;

        /// <summary>
        /// For updating settings
        /// </summary>
        private Thread settingsUpdateThread;

        /// <summary>
        /// Flag for stopping the threads
        /// </summary>
        private bool shouldRun;

        private Dictionary<int, User> users;

        /// <summary>
        /// Not used in the moment, could be useful for handling out of bounds from the touch area
        /// </summary>
        private int maxDepth;

        //Used to convert kinect resolution to 
        //Screen coordinates
        private float virtualScreenConvertFactorX;
        private float virtualScreenConvertFactorY;
        private int virtualScreenXStart = 0;
        private int virtualScreenYStart = 0;

        //counting updateframes
        private long framecounter = 0;

        //Crop corners of sensor frame, because they are less reliable and difficult to use
        private int kinectXCrop = 20;
        private int kinectYCrop = 20;

        /// <summary>
        /// Save mapMode for determining device resolution
        /// </summary>
        MapOutputMode mapMode;

        /// <summary>
        /// Lock for releasing and clearing users
        /// </summary>
        Object userReleaseLock = new Object();


        public KinectManager(InputProvider inputProvider)
        {

            this.inputProvider = inputProvider;

            //get configuration
            String OpenNiconfigPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + CONFIG_XML_FILE;

            this.context = Context.CreateFromXmlFile(OpenNiconfigPath, out scriptNode);
            this.depth = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            if (this.depth == null)
            {
                throw new Exception("Viewer must have a depth node!");
            }

            this.maxDepth = this.depth.DeviceMaxDepth;

            this.userGenerator = new UserGenerator(this.context);
            this.skeletonCapbility = this.userGenerator.SkeletonCapability;
            this.poseDetectionCapability = this.userGenerator.PoseDetectionCapability;
            this.calibPose = this.skeletonCapbility.CalibrationPose;

            this.userGenerator.NewUser += userGenerator_NewUser;
            this.userGenerator.LostUser += userGenerator_LostUser;
            this.poseDetectionCapability.PoseDetected += poseDetectionCapability_PoseDetected;
            this.skeletonCapbility.CalibrationComplete += skeletonCapbility_CalibrationComplete;

            this.skeletonCapbility.SetSkeletonProfile(SkeletonProfile.Upper);
            this.users = new Dictionary<int, User>();
            this.userGenerator.StartGenerating();

            this.mapMode = this.depth.MapOutputMode;

            //load settings
            updateSettings();

            //start threads
            this.shouldRun = true;
            this.readerThread = new Thread(ReaderThread);
            this.readerThread.Start();

            this.settingsUpdateThread = new Thread(runUpdateSettings);
            this.settingsUpdateThread.Start();

            Console.WriteLine("Device initialized");

        }

        private void runUpdateSettings()
        {
            while (this.shouldRun)
            {
                updateSettings();
                //Thread.Sleep(5000);
                Thread.Sleep(500);
            }
        }

        private void updateSettings()
        {
            //Read settings
            int virtualScreenZRelativeOffsetNew = Settings.Default.TouchPlaneOffset;
            int virtualScreenZAbsoluteOffsetNew = Settings.Default.TouchPlaneAbsOffset;
            bool relativeTouchDetectionEnabledNew = Settings.Default.TouchRelativeEnabled;

            //compare with stored settings
            if (virtualScreenZRelativeOffset != virtualScreenZRelativeOffsetNew ||
                virtualScreenZAbsoluteOffset != virtualScreenZAbsoluteOffsetNew ||
                relativeTouchDetectionEnabled != relativeTouchDetectionEnabledNew)
            {
                virtualScreenZRelativeOffset = virtualScreenZRelativeOffsetNew;
                virtualScreenZAbsoluteOffset = virtualScreenZAbsoluteOffsetNew;
                relativeTouchDetectionEnabled = relativeTouchDetectionEnabledNew;

                Console.WriteLine("Current configuration:");
                Console.WriteLine("Virtual-Touch-Plane-Offset: " + virtualScreenZRelativeOffset);
                Console.WriteLine("Virtual-Touch-Plane-Abs-Offset: " + virtualScreenZAbsoluteOffset);
                Console.WriteLine("Enabled relative touch detection: " + relativeTouchDetectionEnabled);

                Rectangle virtualScreen = SystemInformation.VirtualScreen;
                virtualScreenConvertFactorX = (virtualScreen.Width / (float)(mapMode.XRes - kinectXCrop * 2));
                virtualScreenConvertFactorY = (virtualScreen.Height / (float)(mapMode.YRes - kinectYCrop * 2));

                //determine screen working area
                Screen[] screens = Screen.AllScreens;
                for (int index = 0; index <= screens.GetUpperBound(0); index++)
                {
                    int workingX = screens[index].WorkingArea.X;
                    int workingY = screens[index].WorkingArea.Y;

                    if (workingX < virtualScreenXStart)
                    {
                        virtualScreenXStart = workingX;
                    }
                    if (workingY < virtualScreenYStart)
                    {
                        virtualScreenYStart = workingY;
                    }
                }

                Console.WriteLine("Virtual screen x:" + virtualScreen.Width + " y:" + virtualScreen.Height);
                Console.WriteLine("Virtual screen Start coordinates x:" + virtualScreenXStart + " y:" + virtualScreenYStart);
                Console.WriteLine("Device depth map x:" + mapMode.XRes + " y:" + mapMode.YRes);
                Console.WriteLine("Device depth map cropped x:" + (mapMode.XRes - kinectXCrop * 2) + " y:" + (mapMode.YRes - kinectYCrop * 2));
                Console.WriteLine("Convert Factor x:" + virtualScreenConvertFactorX + " y:" + virtualScreenConvertFactorY);
            }
        }


        void skeletonCapbility_CalibrationComplete(object sender, CalibrationProgressEventArgs e)
        {
            if (e.Status == CalibrationStatus.OK)
            {
                Console.WriteLine("User added: " + e.ID);
                this.skeletonCapbility.StartTracking(e.ID);
                this.users.Add(e.ID, new User(e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition>()));
            }
            else if (e.Status != CalibrationStatus.ManualAbort)
            {
                if (this.skeletonCapbility.DoesNeedPoseForCalibration)
                {
                    this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
                }
                else
                {
                    this.skeletonCapbility.RequestCalibration(e.ID, true);
                }
            }
        }

        void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e)
        {
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            this.skeletonCapbility.RequestCalibration(e.ID, true);
        }

        void userGenerator_NewUser(object sender, NewUserEventArgs e)
        {

            if (this.skeletonCapbility.DoesNeedPoseForCalibration)
            {
                this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
            }
            else
            {
                this.skeletonCapbility.RequestCalibration(e.ID, true);
            }
        }

        void userGenerator_LostUser(object sender, UserLostEventArgs e)
        {
            lock (userReleaseLock)
            {
                if (this.users.ContainsKey(e.ID))
                {
                    User user = null;
                    user = this.users[e.ID];
                    Console.WriteLine("Lost user: " + e.ID);
                    releaseUserTouches(e.ID);
                    this.users.Remove(e.ID);

                    if (user != null)
                    {
                        user.Dispose();
                    }
                }
            }

        }

        public void Dispose()
        {
            this.shouldRun = false;
            this.readerThread.Join();
            this.settingsUpdateThread.Join();
            this.context.Release();
        }

        private void GetJoint(int user, SkeletonJoint joint)
        {
            SkeletonJointPosition pos = this.skeletonCapbility.GetSkeletonJointPosition(user, joint);
            if (pos.Position.Z == 0)
            {
                pos.Confidence = 0;
            }
            else
            {
                pos.Position = this.depth.ConvertRealWorldToProjective(pos.Position);
            }
            this.users[user].Joints[joint] = pos;
        }

        private void GetJoints(int user)
        {
            GetJoint(user, SkeletonJoint.Torso);

            GetJoint(user, SkeletonJoint.LeftHand);

            GetJoint(user, SkeletonJoint.RightHand);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="joint"></param>
        /// <param name="jointRef"></param>
        /// <param name="screenPlaneZoffset"></param>
        /// <returns>true if touched </returns>
        private void DetectVirtualRelativeTouch(User user, uint hand, SkeletonJoint jointRef, int screenPlaneZoffset)
        {

            SkeletonJoint jointHand;
            if (hand == 0)
            {
                jointHand = SkeletonJoint.LeftHand;
            }
            else
            {
                jointHand = SkeletonJoint.RightHand;
            }

            Point3D pos = user.Joints[jointHand].Position;

            Point3D posRef = user.Joints[jointRef].Position;

            InputStatus status;

            //hand not probably tracked return
            if (user.Joints[jointHand].Confidence < 0.6)
            {
                sendContact(user.updateHand(hand, InputStatus.UNKNOWN));
                return;
            } //if the reference is not valid, only cursor mode will be reliable
            else if (user.Joints[jointRef].Confidence < 0.6)
            {
                status = InputStatus.CURSOR;
            }
            else
            { //both joints are well tracked, touch detection is executed
                if (pos.Z > (posRef.Z - screenPlaneZoffset))
                {//No Touch
                    status = InputStatus.CURSOR;
                }
                else
                {//Touch
                    status = InputStatus.TOUCHED;
                }
            }

            //convert kinectcoordinates to screencoordinates
            pos.X = virtualScreenXStart + virtualScreenConvertFactorX * (pos.X - kinectXCrop);
            pos.Y = virtualScreenYStart + virtualScreenConvertFactorY * (pos.Y - kinectYCrop);

            HandContact contact = user.updateHand(pos, hand, status);

            sendContact(contact);

        }

        private void sendContact(HandContact contact)
        {
            if (contact != null)
            {
                inputProvider.EnqueueContact(contact);
            }
        }

        private void DetectVirtualAbsoluteTouch(User user, uint hand, int screenPlaneZoffset)
        {

            SkeletonJoint jointHand;
            if (hand == 0)
            {
                jointHand = SkeletonJoint.LeftHand;
            }
            else
            {
                jointHand = SkeletonJoint.RightHand;
            }

            Point3D pos = user.Joints[jointHand].Position;

            InputStatus status;

            //hand not probably tracked return
            if (user.Joints[jointHand].Confidence < 0.6)
            {
                sendContact(user.updateHand(hand, InputStatus.UNKNOWN));
                return;
            }
            else
            { //both joints are well tracked, touch detection is executed
                if (pos.Z > screenPlaneZoffset)
                {//No Touch
                    status = InputStatus.CURSOR;
                }
                else
                {//Touch
                    status = InputStatus.TOUCHED;
                }
            }

            //convert kinectcoordinates to screencoordinates
            pos.X = virtualScreenXStart + virtualScreenConvertFactorX * (pos.X - kinectXCrop);
            pos.Y = virtualScreenYStart + virtualScreenConvertFactorY * (pos.Y - kinectYCrop);

            HandContact contact = user.updateHand(pos, hand, status);

            if (contact != null)
            {
                inputProvider.EnqueueContact(contact);
            }

        }

        private void releaseUserTouches(int userId)
        {
            if (users.ContainsKey(userId))
            {
                User user = this.users[userId];
                HandContact contact = user.updateHand(0, InputStatus.UNKNOWN);
                HandContact contactTwo = user.updateHand(1, InputStatus.UNKNOWN);

                if (contact != null)
                {
                    inputProvider.EnqueueContact(contact);
                }
                if (contactTwo != null)
                {
                    inputProvider.EnqueueContact(contactTwo);
                }
            }
        }

        /// <summary>
        /// Detect touch and movement of hands
        /// </summary>
        /// <param name="user"></param>
        private void DetectTouches(int user)
        {
            GetJoints(user);

            if (relativeTouchDetectionEnabled)
            {
                DetectVirtualRelativeTouch(this.users[user], 0, SkeletonJoint.Torso, virtualScreenZRelativeOffset);
                DetectVirtualRelativeTouch(this.users[user], 1, SkeletonJoint.Torso, virtualScreenZRelativeOffset);
            }
            else
            {
                DetectVirtualAbsoluteTouch(this.users[user], 0, virtualScreenZAbsoluteOffset);
                DetectVirtualAbsoluteTouch(this.users[user], 1, virtualScreenZAbsoluteOffset);
            }
        }


        private unsafe void ReaderThread()
        {

            DepthMetaData depthMD = new DepthMetaData();

            while (this.shouldRun)
            {
                try
                {
                    this.context.WaitOneUpdateAll(this.depth);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                    //Debugger.Break();
                }

                this.depth.GetMetaData(depthMD);

                lock (this)
                {
                    int[] users = this.userGenerator.GetUsers();
                    foreach (int user in users)
                    {

                        if (this.skeletonCapbility.IsTracking(user))
                        {
                            DetectTouches(user);
                        }
                        else
                        {
                            //Do not wait for access
                            if(Monitor.TryEnter(userReleaseLock))
                            {
                                releaseUserTouches(user);
                                Monitor.Exit(userReleaseLock);
                            }
                        }
                    }
                }

                inputProvider.raiseFrame(framecounter++);

            }
        }


    }
}