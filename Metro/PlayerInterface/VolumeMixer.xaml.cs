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
    /// Interaction logic for VolumeMixer.xaml
    /// </summary>
    public partial class VolumeMixer : Window
    {
        public VolumeMixer()
        {
            InitializeComponent();
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           


        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (rectangle1.IsMouseOver)
                {
                    rectangle1.Margin = new Thickness(e.GetPosition(this).X - 10, 0, 0, 0);
                }
            }
        }
    }
}
