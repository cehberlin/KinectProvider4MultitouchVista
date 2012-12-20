using System;
using System.Windows;
using KinectProvider.Properties;

namespace KinectProvider
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    /// <author>Christopher-Eyk Hrabia - www.ceh-photo.de</author>
    public partial class ConfigurationWindow
    {

        public static readonly DependencyProperty TouchPlaneOffsetProperty =
            DependencyProperty.Register("TouchPlaneOffset", typeof(int), typeof(ConfigurationWindow), new UIPropertyMetadata(350));

        public static readonly DependencyProperty TouchPlaneAbsOffsetProperty =
           DependencyProperty.Register("TouchPlaneAbsOffset", typeof(int), typeof(ConfigurationWindow), new UIPropertyMetadata(900));

        public static readonly DependencyProperty TouchRelativeEnabledProperty =
           DependencyProperty.Register("TouchRelativeEnabled", typeof(bool), typeof(ConfigurationWindow), new UIPropertyMetadata(true));


        public int TouchPlaneOffset
        {
            get { return (int)GetValue(TouchPlaneOffsetProperty); }
            set { SetValue(TouchPlaneOffsetProperty, value); }
        }

        public int TouchPlaneAbsOffset
        {
            get { return (int)GetValue(TouchPlaneAbsOffsetProperty); }
            set { SetValue(TouchPlaneAbsOffsetProperty, value); }
        }

        public bool TouchRelativeEnabled
        {
            get { return (bool)GetValue(TouchRelativeEnabledProperty); }
            set { SetValue(TouchRelativeEnabledProperty, value); }
        }

        public bool TouchAbsoluteEnabled
        {
            get { return !TouchRelativeEnabled; }
            set { TouchRelativeEnabled = !value; }
        }


        public ConfigurationWindow()
        {
            DataContext = this;
            InitializeComponent();

            TouchPlaneOffset = Settings.Default.TouchPlaneOffset;
            TouchPlaneAbsOffset = Settings.Default.TouchPlaneAbsOffset;
            TouchRelativeEnabled = Settings.Default.TouchRelativeEnabled;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.TouchPlaneOffset = TouchPlaneOffset;
            Settings.Default.TouchPlaneAbsOffset = TouchPlaneAbsOffset;
            Settings.Default.TouchRelativeEnabled = TouchRelativeEnabled;
            Settings.Default.Save();
            Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Settings.Default.Reload();
        }
    }
}