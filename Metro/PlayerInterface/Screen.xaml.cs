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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using BandedSpectrumAnalyzer;
using Un4seen.Bass;
using System.Diagnostics;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Metro.Assets;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for Screen.xaml
    /// </summary>
    public partial class Screen : UserControl
    {
        public Screen()
        {
            InitializeComponent();
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngine_PropertyChanged;
                spectrumAnalyzer.RegisterSoundPlayer(BassEngine.Instance);
                UIHelper.Bind(BassEngine.Instance, "ChannelLength", ProgressSlider, Slider.MaximumProperty);

            }   catch { }


        }

        Storyboard Anima;
        Storyboard AnimF;
        Storyboard AnimFWL;
        Storyboard AnimAll;
        Storyboard AnimNot;
        private bool inTimerPositionUpdate;

        #region Bass Engine Events
        private void BassEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FileTag":
                    Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, (float)0);
                      VolFade = 0;
                      FadeIn.Start();
                    Dispatcher.BeginInvoke(new Action(() =>
                        
              {
                  if (BassEngine.Instance.FileTag != null)
                  {
                      TagLib.Tag tag = BassEngine.Instance.FileTag.Tag;
                      AlbumText.Content = tag.Album;
                      ArtistText.Content = tag.Artists.Length > 0 ? tag.Artists[0] : string.Empty;
                      TitleText.Content = tag.Title;
                      //YearText.Text = tag.Year.ToString(CultureInfo.InvariantCulture);
                      //GenreText.Text = tag.Genres.Length > 0 ? tag.Genres[0] : string.Empty;
                      //TrackText.Text = tag.Track.ToString(CultureInfo.InvariantCulture);
                      //DiscText.Text = tag.Disc.ToString(CultureInfo.InvariantCulture);
                      try
                      {
                          if (tag.Pictures.Length > 0)
                          {
                              using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                              {
                                  BitmapImage albumImage = new BitmapImage();

                                  albumImage.BeginInit();
                                  albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                  albumImage.StreamSource = albumArtworkMemStream;
                                  albumImage.EndInit();
                                  AlbumArt.Source = albumImage;

                                  //Blur

                                  var n = 75;
                                  Rectangle r = new Rectangle();
                                  r.Fill = new ImageBrush(albumImage);
                                  BlurEffect effect = new BlurEffect() { Radius = 30, KernelType = KernelType.Gaussian };
                                  r.Effect = effect;
                                  Size sz = new Size(100, 100);
                                  r.Measure(sz);
                                  r.Arrange(new Rect(sz));
                                  RenderTargetBitmap rtb = new RenderTargetBitmap(n, n, 74, 74, PixelFormats.Pbgra32);
                                  rtb.Render(r);
                                  rtb.Freeze();

                                  //Color
                                  Rectangle rect = new Rectangle();
                                  rect.Fill = new ImageBrush(albumImage);
                                  rect.Measure(sz);
                                  rect.Arrange(new Rect(sz));
                                  RenderTargetBitmap rtbc = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
                                  rtbc.Render(rect);
                                  rtbc.Freeze();

                                  MemoryStream stream = new MemoryStream();
                                  BitmapEncoder encoder = new BmpBitmapEncoder();
                                  encoder.Frames.Add(BitmapFrame.Create(rtbc));
                                  encoder.Save(stream);

                                  PictureAnalysis.GetMostUsedColor(new System.Drawing.Bitmap(stream));

                                  System.Drawing.Color imgColor = new System.Drawing.Color();
                                  imgColor = PictureAnalysis.MostUsedColor;

                                  Double MostUsed = (PictureAnalysis.MostUsedColorIncidence);
                                  Double Percent = (MostUsed / 10000) * 100;

                                  SolidColorBrush eee = new SolidColorBrush();
                                  eee.Color = Color.FromArgb(imgColor.A, imgColor.R, imgColor.G, imgColor.B);

                                  if (Percent >= 1)
                                  {
                                      AlbumBack.Background = eee;
                                      
                                      BlurEffect eMask = new BlurEffect() { Radius = 3, KernelType = KernelType.Gaussian };
                                      Rectangle rMask = new Rectangle() {Effect = eMask, Fill = Brushes.Black, Margin= new Thickness(2) };
                                      Size szMask = new Size(100, 100);
                                      rMask.Measure(szMask);
                                      rMask.Arrange(new Rect(szMask));
                                      RenderTargetBitmap mRTB = new RenderTargetBitmap(100, 100,96, 96, PixelFormats.Pbgra32);
                                      mRTB.Render(rMask);
                                      mRTB.Freeze();

                                      Border bMask = new Border() { Width = 100, Height = 100, SnapsToDevicePixels = true, Background = new ImageBrush() { Stretch = Stretch.Fill, ImageSource = mRTB } };
                                      
                                      VisualBrush mask = new VisualBrush() { Visual = bMask };
                                      AlbumArt.OpacityMask = mask;
                                     

                                  }
                                  else
                                  {
                                      AlbumBack.Background = new ImageBrush() { Stretch = Stretch.Fill, ImageSource = rtb };
                                      
                                        
                                      Border bMask = new Border() { Width = 100, Height = 100, SnapsToDevicePixels = true , CornerRadius = new CornerRadius(2.5), Background= Brushes.Black};
                                      VisualBrush mask = new VisualBrush() { Visual = bMask };
                                      AlbumArt.OpacityMask = mask;
                                       
                                  }
                                  albumArtworkMemStream.Close();
                                  stream.Close();
                              }
                          }
                          else
                          {
                              AlbumArt.Source = null;
                              AlbumBack.Background = null;
                          }
                      }
                      catch { }
                      finally { }
                      if (lyricContent1.lyric != null)
                      {
                          Anima = MainGr.FindResource("Open") as Storyboard;
                          MainGr.BeginStoryboard(Anima);
                      }
                      else
                      {
                          Anima = MainGr.FindResource("Close") as Storyboard;
                          MainGr.BeginStoryboard(Anima);
                      }
                  }
                  else
                  {
                      AlbumText.Content = string.Empty;
                      ArtistText.Content = string.Empty;
                      TitleText.Content = string.Empty;
                      //YearText.Text = string.Empty;
                      //GenreText.Text = string.Empty;
                      //TrackText.Text = string.Empty;
                      //DiscText.Text = string.Empty;
                      AlbumArt.Source = null;
                      AlbumBack.Background = null;
                  }
                  UpdateLayout();
              }));
                    break;
                case "ChannelPosition":
                    inTimerPositionUpdate = true;
                    ProgressSlider.Value = BassEngine.Instance.ChannelPosition;

                    int totalTime = (int)Bass.BASS_ChannelBytes2Seconds(BassEngine.Instance.ActiveStreamHandle, BassEngine.Instance.ChannelPosition);
                    int durationTime = (int)Bass.BASS_ChannelBytes2Seconds(BassEngine.Instance.ActiveStreamHandle, BassEngine.Instance.ChannelLength);
                    
                    TimeSpan totalTimeSpan = new TimeSpan(0, 0, totalTime);
                    TimeSpan durationTimeSpan = new TimeSpan(0, 0, durationTime);

                    if (durationTimeSpan.Hours == 0)
                    {
                        positionLabel.Content = totalTimeSpan.Minutes.ToString("00") + ":" + totalTimeSpan.Seconds.ToString("00");
                        durationLabel.Content = durationTimeSpan.Minutes.ToString("00") + ":" + durationTimeSpan.Seconds.ToString("00");
                    }
                    else if (durationTimeSpan.Hours == 0 && durationTimeSpan.Minutes == 0)
                    {
                        positionLabel.Content = totalTimeSpan.Seconds.ToString("00");
                        durationLabel.Content = durationTimeSpan.Seconds.ToString("00");
                    }
                    else
                    {
                        positionLabel.Content = totalTimeSpan.ToString();
                        durationLabel.Content = durationTimeSpan.ToString();
                    }
                    inTimerPositionUpdate = false;
                    break;
                case "IsPlaying":
                    if (BassEngine.Instance.IsPlaying == true)
                    {
                        AnimF = spectrumAnalyzer.FindResource("Open") as Storyboard;
                        spectrumAnalyzer.BeginStoryboard(AnimF);
                        if (lyricContent1.lyric != null)
                        {
                            AnimF = spectrumAnalyzer.FindResource("OpenWLyrics") as Storyboard;
                            spectrumAnalyzer.BeginStoryboard(AnimF);
                        }
                        
                    }
                    else
                    {
                        AnimF = spectrumAnalyzer.FindResource("Close") as Storyboard;
                        spectrumAnalyzer.BeginStoryboard(AnimF);
                        if (lyricContent1.lyric != null)
                        {
                            AnimF = spectrumAnalyzer.FindResource("CloseWLyrics") as Storyboard;
                            spectrumAnalyzer.BeginStoryboard(AnimF);
                        }
                    }
                    break;
            }

        }
        #endregion

        private void ScreenInter_Loaded(object sender, RoutedEventArgs e)
        {
            FadeIn.Tick += new EventHandler(FadeIn_Tick);
            FadeIn.Interval = new TimeSpan(0, 0, 0, 0, 10);
            FadeOut.Tick += new EventHandler(FadeOut_Tick);
            FadeOut.Interval = new TimeSpan(0, 0, 0, 0, 10);

            LyricSettings.PropertyChanged += new PropertyChangedEventHandler(LyricSettings_PropertyChanged);
        }

        void LyricSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LyricSpeed":
                    lyricContent1.Speed = LyricSettings.Speed;

                    break;

                case "LyricSynchronization":
                    lyricContent1.Synchronization = LyricSettings.Synchronization;

                    break;

            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!inTimerPositionUpdate)
            {
                Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, 0);
                BassEngine.Instance.SetChannelPosition((long)e.NewValue);
            }
        }

        private void ScreenInter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                if (lyricContent1.lyric != null)
                {
                    if ((this.ActualHeight - 156) >= 550)
                    {
                        spectrumAnalyzer.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                        spectrumAnalyzer.Height = 550;
                    }
                    else
                    {
                        spectrumAnalyzer.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        spectrumAnalyzer.Height = this.ActualHeight - 156;
                    }
                }
                else
                {
                    if ((this.ActualHeight - 91) >= 550)
                    {
                        spectrumAnalyzer.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                        spectrumAnalyzer.Height = 550;
                    }
                    else
                    {
                        spectrumAnalyzer.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        spectrumAnalyzer.Height = this.ActualHeight - 91;
                    }
                }
            }
        }

        private void volumeChange(object value)
        {
            AnimF = volumeButton.FindResource("White") as Storyboard;
            ((System.Windows.Shapes.Path)value).BeginStoryboard(AnimF);

            foreach (FrameworkElement item in ((Panel)(volMute.Parent)).Children)
            {
                if (item.Name != ((System.Windows.Shapes.Path)value).Name)
                {
                    AnimFWL = volumeButton.FindResource("Trans") as Storyboard;
                    (item as System.Windows.Shapes.Path).BeginStoryboard(AnimFWL);
                }
            }
        }


        private void volumeSliderRaw_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newVol = ushort.MaxValue * e.NewValue;

            uint v = ((uint)newVol) & 0xffff;
            uint vALL = v | (v << 16);

            BassEngine.WaveOutSetVolume(IntPtr.Zero, vALL);

            if (volumeSliderRaw != null && volumeSliderRaw.IsLoaded)
            {

                if (volumeSliderRaw.Value == 0)
                { volumeChange(volMute); }
                else if (volumeSliderRaw.Value > 0 && volumeSliderRaw.Value <= 0.20)
                { volumeChange(volZero); }
                else if (volumeSliderRaw.Value > 0.20 && volumeSliderRaw.Value <= 0.40)
                { volumeChange(volOne); }
                else if (volumeSliderRaw.Value > 0.40 && volumeSliderRaw.Value <= 0.85)
                { volumeChange(volTwo); }
                else if (volumeSliderRaw.Value > 0.85)
                { volumeChange(volThree); }
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeSliderRaw != null && volumeSliderRaw.IsLoaded)
            {

                AnimF = volumeSliderRaw.FindResource("Change") as Storyboard;
                ((AnimF as Storyboard).Children[0] as DoubleAnimation).To = volumeSlider.Value;
                volumeSliderRaw.BeginStoryboard(AnimF);
            }

        }

        private void circleButtonRipple1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Window w = new VolumeMixer();
            w.Show();
        }

        private void ScreenInter_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
        }

        private void circleButtonRipple2_Click(object sender, RoutedEventArgs e)
        {
            Window w = new ScreenLyricsInfo();
            w.Show();
        }
        
        private DispatcherTimer FadeIn = new DispatcherTimer();
        private DispatcherTimer FadeOut = new DispatcherTimer();
        private double VolFade = 0;

        private void FadeOut_Tick(object sender, EventArgs e)
        {
            if (VolFade > 0)
            {
                VolFade = VolFade - 5;
                Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, (float)VolFade / 100);
            }
            else if (VolFade == 0)
            {
                FadeOut.Stop();
            }

        }
        private void FadeIn_Tick(object sender, EventArgs e)
        {
            if (VolFade < 100)
            {
                VolFade = VolFade + 5;
                Bass.BASS_ChannelSetAttribute(BassEngine.Instance.ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, (float)VolFade / 100);
            }
            else if (VolFade == 100)
            {
                FadeIn.Stop();
            }

        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VolFade = 100;
            FadeOut.Start();
        }

        private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VolFade = 0;
            FadeIn.Start();
        }

        private void OpenFade(object sender, MouseEventArgs e)
        {
            if (!BassEngine.Instance.IsPlaying)
            {
                AnimF = spectrumAnalyzer.FindResource("Open") as Storyboard;
                spectrumAnalyzer.BeginStoryboard(AnimF);
                if (lyricContent1.lyric != null)
                {
                    AnimF = spectrumAnalyzer.FindResource("OpenWLyrics") as Storyboard;
                    spectrumAnalyzer.BeginStoryboard(AnimF);
                }
                
            }
        }

        private void CloseFade(object sender, MouseEventArgs e)
        {
            if (!BassEngine.Instance.IsPlaying)
            {
                AnimF = spectrumAnalyzer.FindResource("Close") as Storyboard;
                spectrumAnalyzer.BeginStoryboard(AnimF);
                if (lyricContent1.lyric != null)
                {
                    AnimF = spectrumAnalyzer.FindResource("CloseWLyrics") as Storyboard;
                    spectrumAnalyzer.BeginStoryboard(AnimF);
                }
            }
        }

        void repeat()
        {
            if (repeatButton.Tag.ToString() == "Normal")
            {
                repeatButton.Tag = "RepeatOne";
                AnimFWL = repeatButton.FindResource("RepeatOne") as Storyboard;
                repeatButton.BeginStoryboard(AnimFWL);
            }
            else if (repeatButton.Tag.ToString() == "RepeatOne")
            {
                repeatButton.Tag = "Repeat";
                AnimFWL = repeatButton.FindResource("Repeat") as Storyboard;
                repeatButton.BeginStoryboard(AnimFWL);
            }
            else
            {
                repeatButton.Tag = "Normal";
                AnimFWL = repeatButton.FindResource("Normal") as Storyboard;
                repeatButton.BeginStoryboard(AnimFWL);
            }
        }


        void shuffle()
        {
            if (shuffleButton.Tag.ToString() == "Normal")
            {
                shuffleButton.Tag = "Shuffle";
                AnimFWL = shuffleButton.FindResource("Shuffle") as Storyboard;
                shuffleButton.BeginStoryboard(AnimFWL);
            }
            else
            {
                shuffleButton.Tag = "Normal";
                AnimFWL = shuffleButton.FindResource("Normal") as Storyboard;
                shuffleButton.BeginStoryboard(AnimFWL);
            }
        }

        public bool mute()
        {
                AnimNot = volumeSliderRaw.FindResource("Max") as Storyboard;
                AnimAll = volumeSliderRaw.FindResource("Min") as Storyboard;
            if (volumeSlider.Tag.ToString() == "Max")
            {
                volumeSlider.Tag = "Min";
                volumeSlider.IsEnabled = false;
                volumeSliderRaw.BeginStoryboard(AnimAll);
                return true;
            }
            else
            {
                volumeSlider.Tag = "Max";
                volumeSlider.IsEnabled = true;
                ((AnimNot as Storyboard).Children[0] as DoubleAnimation).To = volumeSlider.Value;
                volumeSliderRaw.BeginStoryboard(AnimNot);
                return false;

            }
        }
        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            shuffle();
        }

        private void repeatButton_Click(object sender, RoutedEventArgs e)
        {
            repeat();
        }

        private void volumeButton_Click(object sender, RoutedEventArgs e)
        {
            mute();
        }

        private void volumeSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

    }

    public static class LyricSettings
    {
        private static double LSpeed = 1;
        private static double LSynchronization = 0;

        public static event PropertyChangedEventHandler PropertyChanged;
        private static void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(Speed,new PropertyChangedEventArgs(info));
            }
        }

        public static double Speed
        {
            get { return LSpeed; }
            set
            {
                double oldValue = LSpeed;
                LSpeed = value;
                if (oldValue != LSpeed)
                    NotifyPropertyChanged("LyricSpeed");
            }
        }

        public static double Synchronization
        {
            get { return LSynchronization; }
            set
            {
                double oldValue = LSynchronization;
                LSynchronization = value;
                if (oldValue != LSynchronization)
                    NotifyPropertyChanged("LyricSynchronization");
            }
        }
    }

}
