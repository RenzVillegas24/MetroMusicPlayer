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
    /// Interaction logic for AnimatedCaret.xaml
    /// </summary>
    public partial class AnimatedCaret : UserControl
    {
        Storyboard In;
        Storyboard Out;
        public AnimatedCaret()
        {
            InitializeComponent();
        }

        private void CustomTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            MoveCaret();
        }

        private void MoveCaret()
        {
            var c = CustomTextBox.GetRectFromCharacterIndex(CustomTextBox.CaretIndex).Location;
            var t = CustomTextBox.GetRectFromCharacterIndex(CustomTextBox.Text.Length).Location;
            In = Caret.FindResource("Anims") as Storyboard;

            ((In.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0] as SplineThicknessKeyFrame).Value = new Thickness(c.X, c.Y, 0, 0);
            ((In.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0] as SplineDoubleKeyFrame).Value = CustomTextBox.FontSize + 4;
            Caret.BeginAnimation(MarginProperty, (ThicknessAnimationUsingKeyFrames)In.Children[0]);
            Caret.BeginAnimation(HeightProperty, (DoubleAnimationUsingKeyFrames)In.Children[1]);
              
        
        }

        private void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MoveCaret();
        }
    }
}
