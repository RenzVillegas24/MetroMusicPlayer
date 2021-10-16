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
using System.Windows.Shapes;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for ScreenLyricsInfo.xaml
    /// </summary>
    public partial class ScreenLyricsInfo : Window
    {
        public ScreenLyricsInfo()
        {
            InitializeComponent();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                double sl;
                if (slider1.Value < 2)
                    sl = (slider1.Value / 2);
                else
                    sl = ((slider1.Value - 1) * 1.33333333);
                LyricSettings.Speed = sl;
                label1.Content = "Speed = " + Math.Round(LyricSettings.Speed, 2) * 100 + "%";
            }
            catch { }
            finally { }
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                LyricSettings.Synchronization = slider2.Value;
                label2.Content = "Synchronization = " + Math.Round(LyricSettings.Synchronization, 2) + "s";
            }
            catch { }
            finally { }
        }
    }
}
