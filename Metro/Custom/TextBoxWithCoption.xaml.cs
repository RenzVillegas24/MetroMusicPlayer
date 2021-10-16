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

namespace Metro.Custom
{
    /// <summary>
    /// Interaction logic for TextBoxWithCoption.xaml
    /// </summary>
    public partial class TextBoxWithCoption : UserControl
    {
        Storyboard In = new Storyboard();
        Storyboard In2 = new Storyboard();

        public TextBoxWithCoption()
        {
            InitializeComponent();
        }

        private void Caption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox.Focus();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox.Text))
            {
                In = Container.FindResource("In") as Storyboard;
                this.BeginStoryboard(In);
            }
            In2 = Container.FindResource("In-Focus") as Storyboard;
            this.BeginStoryboard(In2);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox.Text))
            {
                In = Container.FindResource("Out") as Storyboard;
                this.BeginStoryboard(In);
            }
            In2 = Container.FindResource("Out-Focus") as Storyboard;
            this.BeginStoryboard(In2);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox.Text))
            {
                In = Container.FindResource("Out") as Storyboard;
                this.BeginStoryboard(In);
            }
            else
            {
                In = Container.FindResource("In") as Storyboard;
                this.BeginStoryboard(In);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsFocused)
            {
                if (string.IsNullOrEmpty(TextBox.Text))
                {
                    In = Container.FindResource("Out") as Storyboard;
                    this.BeginStoryboard(In);
                }
                else
                {
                    In = Container.FindResource("In") as Storyboard;
                    this.BeginStoryboard(In);
                }
            }
        }
    }
}
