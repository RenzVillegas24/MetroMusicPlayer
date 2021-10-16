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
using System.Windows.Media.Animation;
using System.IO;
using System.ComponentModel;
using BandedSpectrumAnalyzer;
using Un4seen.Bass;
using Metro.Lyric;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for LyricContent.xaml
    /// </summary>
    public partial class LyricContent : UserControl
    {

        public MyLyric lyric;
        Double startTime;
        Double endTime;
        Double timeIntervalBetween;
        int lyricLineIndex;
        private int fontsize = 40;

        public Color FontBackColor
        { get; set; }
        public Color FontForeColor
        { get; set; }

        private double PSpeed = 1;
        private double PSynchronization = 0;

        public Double Speed
        {
            get { return PSpeed; }
            set
            {   PSpeed = value;  }
        }
        public Double Synchronization
        {
            get { return PSynchronization; }
            set
            {   PSynchronization = value;   }
        }

        public LyricContent()
        {
            InitializeComponent();
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngineL_PropertyChanged;
            }
            catch { }
        }

        #region Bass Engine Events
        private void BassEngineL_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "FileTag":
                    Dispatcher.BeginInvoke(new Action(() =>
              {
                  string filePath = BassEngine.Instance.FileTag.Name;
                  string lyricPath = filePath.Remove(filePath.Length - 3) + "lrc";
                  nowLyricTextBlock.Text = "";
                  if (File.Exists(lyricPath) == true)
                  {
                      lyric = null;
                      InitializeMyLyric(lyricPath, "");

                      foreach (string line in lyric.LyricTextLines)
                      {
                          nowLyricTextBlock.Text += "\r\n" + line;
                      }

                  }
                  else
                  {
                      lyric = null;
                  }
              
              }));
                    break;
                case "ChannelPosition":
                    if (lyric != null)
                    {
                            int durationTime = (int)(Bass.BASS_ChannelBytes2Seconds(BassEngine.Instance.ActiveStreamHandle, BassEngine.Instance.ChannelPosition)*1000);
                            ChangeLrcTimeInterval();
                            GetLyricIndex((durationTime + Synchronization) * Speed);

                        }
                    break;

            }

        }

        #endregion
        Double offset;


        private void InitializeMyLyric(string path, string lrcStr)
        {
            if (path != "" || lrcStr != "")
            {
                try
                {
                    if (lyric == null)
                    {
                        if (path != "" && lrcStr == "")
                        {
                            lyric = new MyLyric(path, Encoding.Default);
                        }
                        else if (path == "" && lrcStr != "")
                        {
                            lyric = new MyLyric(lrcStr);
                        }
                    }
                   
                    offset = 0;
                    lyricLineIndex = 0;
                    ChangeLrcTimeInterval();
                    if (BassEngine.Instance.ChannelPosition != 0)
                    {
                        double durationTime = Bass.BASS_ChannelBytes2Seconds(BassEngine.Instance.ActiveStreamHandle, BassEngine.Instance.ChannelLength) * 1000;
                        GetLyricIndex(durationTime);
                    }
                }
                catch
                {
                    lyric = null;
                }
            }
        }

        private void GetLyricIndex(double time)
        {
           
            if (lyric != null)
            {
                if (time <= lyric.LyricTimeLinesDValue[0])
                {
                    lyricLineIndex = 0;
                }
                else if (time > lyric.LyricTimeLinesDValue[lyric.LyricTimeLinesDValue.Count - 1])
                {
                    lyricLineIndex = lyric.LyricTimeLinesDValue.Count - 1;
                }
                else
                {
                    for (int i = 0; i < lyric.LyricTimeLinesDValue.Count; i++)
                    {
                         if (time < lyric.LyricTimeLinesDValue[i + 1] && time >= lyric.LyricTimeLinesDValue[i])
                            {
                                lyricLineIndex = i;
                                break;
                            }
                    }
                }
                if (lyricLineIndex != 0 && lyricLineIndex < lyric.LyricTimeLines.Count - 1)
                {
                    preLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex - 1];
                    //nowLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex];
                    //nextLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex + 1];
                  
                }
                else if (lyricLineIndex == 0)
                {
                    preLyricTextBlock.Text = " ";
                    //nowLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex];
                  
                }
                else if (lyricLineIndex == lyric.LyricTimeLines.Count - 1)
                {
                    preLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex - 1];
                    //nowLyricTextBlock.Text = lyric.LyricTextLines[lyricLineIndex];
                   
                }
                ChangeLrcTimeInterval();
            }
            
        }
    
        private void ChangeLrcTimeInterval()
        {
            startTime = lyric.LyricTimeLinesDValue[lyricLineIndex];
            endTime = 0d;
            if (lyricLineIndex != lyric.LyricTimeLinesDValue.Count - 1)
            {
                endTime = lyric.LyricTimeLinesDValue[lyricLineIndex + 1];
                timeIntervalBetween = endTime - startTime;
                if (timeIntervalBetween >= 1000)
                {
                    timeIntervalBetween -= 100;
                }
            }
            else
            {
                timeIntervalBetween = 6666;
            }
        }

        private void ChangeUIElementOpacity(TextBox tb, int Beg, int To, int From, int Dur)
        {
            try
            {
                DoubleAnimation da = new DoubleAnimation(From, To, TimeSpan.FromMilliseconds(Dur))
                {
                    BeginTime = TimeSpan.FromMilliseconds(Beg)
                };
                tb.BeginAnimation(Panel.OpacityProperty, da);
            }
            catch { }
        }     

        internal void ChangeFontForeColor(Color dskLrcPlayedForecolor, Color dskLrcUnplayedForecolor)
        {
            FontBackColor  = dskLrcUnplayedForecolor;
            FontForeColor = dskLrcPlayedForecolor;
        }

        internal int GetFontSize()
        {
            return (int)nowLyricTextBlock.FontSize;
        }


        
        internal void ChangeFontFamily(string dskLrcFontFamily)
        {
            nowLyricTextBlock.FontFamily = new FontFamily(dskLrcFontFamily);
        }

        internal void ChangeFontStyle(string dskLrcFontStyle)
        {
            if (dskLrcFontStyle == "Bold")
            {
                nowLyricTextBlock.FontStyle = FontStyles.Normal;
                nowLyricTextBlock.FontWeight = FontWeights.Bold;
            }
            else if (dskLrcFontStyle == "Italic")
            {
                nowLyricTextBlock.FontStyle = FontStyles.Italic;
                nowLyricTextBlock.FontWeight = FontWeights.Normal;
            }
            else if (dskLrcFontStyle == "Italic, Bold")
            {
                nowLyricTextBlock.FontStyle = FontStyles.Italic;
                nowLyricTextBlock.FontWeight = FontWeights.Bold;
            }
            else
            {
                nowLyricTextBlock.FontStyle = FontStyles.Normal;
                nowLyricTextBlock.FontWeight = FontWeights.Normal;
            }

        }

        internal string GetFontFamily()
        {
            return nowLyricTextBlock.FontFamily.ToString();
        }

        internal string GetFontStyle()
        {
            string ret = "Normal";
            if (nowLyricTextBlock.FontStyle == FontStyles.Normal &&
                nowLyricTextBlock.FontWeight == FontWeights.Normal)
            {
                ret = "Normal";
            }
            if (nowLyricTextBlock.FontStyle == FontStyles.Italic &&
                nowLyricTextBlock.FontWeight == FontWeights.Normal)
            {
                ret = "Italic";
            }
            if (nowLyricTextBlock.FontStyle == FontStyles.Italic &&
                nowLyricTextBlock.FontWeight == FontWeights.Bold)
            {
                ret = "Italic, Bold";
            }
            if (nowLyricTextBlock.FontStyle == FontStyles.Normal &&
                nowLyricTextBlock.FontWeight == FontWeights.Bold)
            {
                ret = "Bold";
            }
            return ret;
        }

        private void nowLyricTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ChangeLrcTimeInterval();
            }
            catch { }
              double dur =  endTime - startTime;

        }

        private void preLyricTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lyric != null)
            { 
                ChangeLrcTimeInterval();

                double size = (nowLyricTextBlock.FontSize * nowLyricTextBlock.FontFamily.LineSpacing);
                double top = size * lyricLineIndex;
                double dur = endTime - startTime;

                Storyboard Anim = new Storyboard();

                DoubleAnimationUsingKeyFrames DblAnim = new DoubleAnimationUsingKeyFrames();
                SplineDoubleKeyFrame SplDblAnim = new SplineDoubleKeyFrame();
                SplDblAnim.KeySpline = new KeySpline(0, 0.425, 0, 1);
              
                SplDblAnim.KeyTime = new TimeSpan(0, 0, 0, 0, 250);
                SplDblAnim.KeySpline = new KeySpline(0, 0.425, 0, 1);
                
                SplDblAnim.Value = (lyric.LyricTextLines.Count * size) - (top + 1); ;
                DblAnim.KeyFrames.Add(SplDblAnim);
                nowLyricTrans.BeginAnimation(TranslateTransform.YProperty, DblAnim);

                
                nowLyricTextBlock.Margin = new Thickness(0, -(lyric.LyricTextLines.Count * size) ,0,0);
               

                

            }
            
          

        }


     

    }
}
