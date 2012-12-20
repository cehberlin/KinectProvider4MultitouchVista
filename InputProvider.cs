using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using Multitouch.Contracts;

namespace KinectProvider
{
    /// <summary>
    /// Implementation of the multitouchvista provider interface
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    [AddIn("KinectProvider", Publisher = "Christopher-Eyk Hrabia", Description = "Provides input from Kinect based on OpenNI", Version = VERSION)]
    [Export(typeof(IProvider))]
    public class InputProvider : IProvider
    {
        internal const string VERSION = "1.0.0.0";

        public event EventHandler<NewFrameEventArgs> NewFrame;

        KinectManager deviceManager;
        Queue<Contact> contactsQueue;

        public InputProvider()
        {
            contactsQueue = new Queue<Contact>();

            SendEmptyFrames = false;
        }

        public bool SendImageType(ImageType imageType, bool isEnable)
        {
            return false;
        }

        public void Start()
        {
            deviceManager = new KinectManager(this);
        }

        public void Stop()
        {
            if (deviceManager != null)
            {
                deviceManager.Dispose();
                deviceManager = null;
            }
        }

        public bool IsRunning
        {
            get { return deviceManager != null; }
        }

        /// <summary>
        /// Will be called by an updating thread
        /// </summary>
        public void raiseFrame(long timestamp)
        {
            lock (contactsQueue)
            {
                if (SendEmptyFrames || contactsQueue.Count > 0)
                {
                    EventHandler<NewFrameEventArgs> eventHandler = NewFrame;
                    if (eventHandler != null)
                        eventHandler(this, new NewFrameEventArgs(timestamp, contactsQueue, null));
                    contactsQueue.Clear();
                }
            }
        }

        internal void EnqueueContact(HandContact contact)
        {
            lock (contactsQueue)
            {
                contactsQueue.Enqueue((HandContact)contact.Clone());
            }
        }

        public UIElement GetConfiguration()
        {
            return new ConfigurationWindow();
        }

        public bool HasConfiguration
        {
            get { return true; }
        }

        public bool SendEmptyFrames { get; set; }
    }
}