using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using BandedSpectrumAnalyzer;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for SpeakerAnimationAnalyzer.xaml
    /// </summary>
    public partial class SpeakerAnimationAnalyzer : UserControl
    {
        DispatcherTimer CircleUpdate = new DispatcherTimer();
        private float[] channelData = new float[2048];
        public SpeakerAnimationAnalyzer()
        {
            InitializeComponent();
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngine_PropertyChangedPl;
            }
            catch { }
        }
        #region Bass Engine Events
        private void BassEngine_PropertyChangedPl(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {

                case "ChannelPosition":
                
                    break;

                case "IsPlaying":
                    if (BassEngine.Instance.IsPlaying)
                    {
                        CircleUpdate.Start();
                    }
                    else
                    {
                        CircleUpdate.Stop();
                    }
                    break;

            }

        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CircleUpdate.Interval = TimeSpan.FromMilliseconds(50);
            CircleUpdate.Tick += new EventHandler(CircleUpdate_Tick);
        }
        public void CircleUpdate_Tick(object sender, EventArgs e)
        {
            BassEngine.Instance.GetFFTData(channelData);
            double equals = (20 * Math.Log10(channelData[1]));

            if (Math.Max((250 + equals), 0) != 0)
            {
                freq1.Height = Math.Min(Math.Max((250 + equals),0), 250);
                freq1.Width = Math.Min(Math.Max((250 + equals), 0), 250);
            }
            else
            {
                freq2.Height = 225; freq2.Width = 225;
            }

            double equals2 = (20 * Math.Log10((double)channelData[500]));

            if (Math.Max((200 + equals2), 0) != 0)
            {
                freq2.Height = Math.Min(Math.Min(Math.Max((200 + equals2), 0), freq1.Height), 150);
                freq2.Width = Math.Min(Math.Min(Math.Max((200 + equals2), 0), freq1.Width), 150);
            }
            else
            {
                freq2.Height = 175; freq2.Width = 175;
            }
            double equals3 = (20 * Math.Log10((double)channelData[1500]));

            freq3.Height = Math.Max(Math.Min((170 + equals3)/1.25, freq2.Height), 50);
            freq3.Width = Math.Max(Math.Min((170 + equals3)/1.25, freq2.Width), 50);


        }
    }
}
