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
using BandedSpectrumAnalyzer;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for PlayBox.xaml
    /// </summary>
    public partial class PlayBox : UserControl
    {
        public PlayBox()
        {
            InitializeComponent(); 
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngineP_PropertyChanged;
            }
            catch { }
            finally { }
        }

        #region Bass Engine Events
        private void BassEngineP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "IsPlaying":
                    if (BassEngine.Instance.IsPlaying)
                    {
                        Enter = PlayPauseBtn.FindResource("PauseAnim") as Storyboard;
                        PlayPauseBtn.BeginStoryboard(Enter);
                    }
                    else
                    {
                        Enter = PlayPauseBtn.FindResource("PlayAnim") as Storyboard;
                        PlayPauseBtn.BeginStoryboard(Enter);
                    }
                break;
            }

        }
        #endregion
  

        Storyboard Enter;
        Storyboard Close;
        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.IsPlaying)
            {
                BassEngine.Instance.Pause();
            }
            else
            {
                BassEngine.Instance.Play();
            }
        }

        private void PlayPauseBtn_MouseOver(object sender, MouseEventArgs e)
        {
            Close = PlayPauseBtn.FindResource("MouseOverLeave") as Storyboard;
            (sender as Button).BeginStoryboard(Close);
            (Close.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1;
            (Close.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1;
        }

        private void PlayPauseBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            Close = PlayPauseBtn.FindResource("MouseOverLeave") as Storyboard;
            (sender as Button).BeginStoryboard(Close);
            (Close.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1.2;
            (Close.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1.2;

        }
    }
}
