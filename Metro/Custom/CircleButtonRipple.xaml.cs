using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Metro.Custom
{
    class CircleButtonRipple : Button
    {
        static CircleButtonRipple()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CircleButtonRipple), new FrameworkPropertyMetadata(typeof(CircleButtonRipple)));
        }

        public Brush HighlightBackground
        {
            get { return (Brush)GetValue(HighlightBackgroundProperty); }
            set { SetValue(HighlightBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighlightBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightBackgroundProperty =
            DependencyProperty.Register("HighlightBackground", typeof(Brush), typeof(CircleButtonRipple), new PropertyMetadata(Brushes.White));

        Ellipse ellipse;
        Grid grid;
        Storyboard animation;
        Storyboard animation2;
        Storyboard enter;
        Storyboard leave;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ellipse = GetTemplateChild("PART_ellipse") as Ellipse;
            grid = GetTemplateChild("PART_grid") as Grid;
            animation = grid.FindResource("PART_animation") as Storyboard;
            animation2 = grid.FindResource("PART_animation2") as Storyboard;
            enter = grid.FindResource("PART_enter") as Storyboard;
            leave = grid.FindResource("PART_leave") as Storyboard;

            this.AddHandler(KeyDownEvent, new RoutedEventHandler((sender, e) =>
            {
                var targetWidth = Math.Max(ActualWidth, ActualHeight);
                var mousePosition = (e as KeyEventArgs);
                var startMargin = new Thickness(ActualWidth / 2, ActualHeight / 2, 0, 0);
                //set initial margin to mouse position
                if (mousePosition.Key == Key.Space || mousePosition.Key == Key.Enter)
                {
                    ellipse.Margin = startMargin;
                    //set the to value of the animation that animates the width to the target width
                    (animation.Children[0] as DoubleAnimation).To = targetWidth;

                    //set the to and from values of the animation that animates the distance relative to the container (grid)
                    (animation.Children[1] as ThicknessAnimation).From = startMargin;

                    (animation.Children[1] as ThicknessAnimation).To = new Thickness((ActualWidth - targetWidth) / 2, (ActualHeight - targetWidth) / 2, 0, 0);
                    ellipse.BeginStoryboard(animation);
                }

            }), true);
            this.AddHandler(MouseUpEvent, new RoutedEventHandler((sender, e) =>
            {
                ellipse.BeginStoryboard(animation2);

            }), true);
            this.AddHandler(KeyUpEvent, new RoutedEventHandler((sender, e) =>
            {
                ellipse.BeginStoryboard(animation2);
            }), true);
            this.AddHandler(MouseEnterEvent, new RoutedEventHandler((sender, e) =>
            {
                ellipse.BeginStoryboard(enter);
            }), true);
            this.AddHandler(MouseLeaveEvent, new RoutedEventHandler((sender, e) =>
            {
                ellipse.BeginStoryboard(leave);
            }), true); 
            this.AddHandler(MouseDownEvent, new RoutedEventHandler((sender, e) =>
            {
                var targetWidth = Math.Max(ActualWidth, ActualHeight);
                var mousePosition = (e as MouseButtonEventArgs).GetPosition(this);
                var startMargin = new Thickness(mousePosition.X, mousePosition.Y, 0, 0);
                //set initial margin to mouse position
                ellipse.Margin = startMargin;
                //set the to value of the animation that animates the width to the target width
                (animation.Children[0] as DoubleAnimation).To = targetWidth;

                //set the to and from values of the animation that animates the distance relative to the container (grid)
                (animation.Children[1] as ThicknessAnimation).From = startMargin;

                this.Focus();
                (animation.Children[1] as ThicknessAnimation).To = new Thickness((ActualWidth - targetWidth) / 2, (ActualHeight - targetWidth) / 2, 0, 0);
                ellipse.BeginStoryboard(animation);
            }), true);

        }
    }
}
