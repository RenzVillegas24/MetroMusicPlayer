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
using BandedSpectrumAnalyzer;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Threading;

namespace Metro
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer : Window
    {
        Point DistanceFromStart;
        Point Relative;
        Boolean isMoving;
        Boolean isLoaded = true;

        Storyboard Open;
        Storyboard Close;
        Storyboard anim;
        Storyboard anim2;
        Storyboard anim3;

        Double titleWidth;
        Rect dekstopWorkingArea = System.Windows.SystemParameters.WorkArea;

        public MiniPlayer()
        {
            InitializeComponent();
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngine_PropertyChangedMin;
                spectrumMusicMin.RegisterSoundPlayer(BassEngine.Instance);

            }
            catch { }
        }


        #region Bass Engine Events
        private void BassEngine_PropertyChangedMin(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "FileTag":
                        Update();
                        break;

                case "ChannelPosition":
                        progressTime.Percentage = BassEngine.Instance.ChannelPosition;
                        progressTime.MaxPercentage = BassEngine.Instance.ChannelLength;
                        break;

                case "IsPlaying":
                 /*   if (playBoxControl.IsMouseOver)
                    {
                        if (!BassEngine.Instance.IsPlaying)
                        {
                            Open = playBoxControl.FindResource("Open") as Storyboard;
                            playBoxControl.BeginStoryboard(Open);
                        }
                    }
                    else
                    {
                        if (BassEngine.Instance.IsPlaying)
                        {
                            Close = playBoxControl.FindResource("Close") as Storyboard;
                            playBoxControl.BeginStoryboard(Close);
                        }
                        else
                        {
                            Open = playBoxControl.FindResource("Open") as Storyboard;
                            playBoxControl.BeginStoryboard(Open);
                        }
                    }
                  */
                    break;

            }

        }
        #endregion



        void PlayPauseBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            anim = MainGrid.FindResource("MouseOverLeave") as Storyboard;
            (anim.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1;
            (anim.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1;
            if(show.Opacity == 1)
                progressTime.BeginStoryboard(anim);
        }

        void PlayPauseBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            anim = MainGrid.FindResource("MouseOverLeave") as Storyboard;
            (anim.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1.25;
            (anim.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 1.25;
            if (show.Opacity == 1)
                progressTime.BeginStoryboard(anim);
            (sender as Button).BeginStoryboard(anim);
        }

        void Update()
        {
                if (BassEngine.Instance.FileTag != null)
                {

                    TagLib.Tag tag = BassEngine.Instance.FileTag.Tag;
                    TitleText.Text = tag.Title;
                    ArtistText.Content = tag.Artists.Length > 0 ? tag.Artists[0] : string.Empty;

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
                                albumArt.Source = albumImage;

                                //Blur
                                /*
                                var n = 75;
                                Rectangle r = new Rectangle();
                                r.Fill = new ImageBrush(albumImage);
                                BlurEffect effect = new BlurEffect() { Radius = 10, KernelType = KernelType.Gaussian };
                                r.Effect = effect;
                                Size sz = new Size(98, 98);
                                r.Measure(sz);
                                r.Arrange(new Rect(sz));
                                RenderTargetBitmap rtb = new RenderTargetBitmap(n, n, n, n, PixelFormats.Pbgra32);
                                rtb.Render(r);
                                rtb.Freeze();
                                this.Background = new ImageBrush() { Stretch = Stretch.Fill, ImageSource = rtb };
                                */
                                albumArtworkMemStream.Close();
                                border2.Visibility = System.Windows.Visibility.Visible;

                                spectrumMusicMin.Visibility = System.Windows.Visibility.Collapsed;

                            }

                        }
                        else
                        {
                            albumArt.Source = null;
                            border2.Visibility = System.Windows.Visibility.Collapsed;
                            spectrumMusicMin.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    catch { }
                    finally { }
                }
                else
                {
                    ArtistText.Content = string.Empty;
                    TitleText.Text = string.Empty;
                    albumArt.Source = null;
                    border2.Visibility = System.Windows.Visibility.Collapsed;
                    spectrumMusicMin.Visibility = System.Windows.Visibility.Visible;
                }


        }

        private void TitleText_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Open = TitleGrid.FindResource("Scrolling") as Storyboard;
            double titleWidth = MeasureString(TitleText.Text, TitleText).Width;

            TitleText.MinWidth = titleWidth;
            TitleText.Width = titleWidth;

            if (titleWidth > 120)
            {
                TitleText.Margin = new Thickness(140, 0, -titleWidth, 0);
                (Open.Children[0] as DoubleAnimation).To = -(titleWidth + 150);
                (Open.Children[0] as DoubleAnimation).From = 10;
                BeginStoryboard(Open);
            }
            else
            {
                TitleText.Margin = new Thickness(0, 0, 0, 0);
                (Open.Children[0] as DoubleAnimation).To = 0;
                (Open.Children[0] as DoubleAnimation).From = 0;
                BeginStoryboard(Open);
            }
        }

        private Size MeasureString(string candidate, TextBlock textBlock)
        {

            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    textBlock.FontFamily, textBlock.FontStyle,
                    textBlock.FontWeight, textBlock.FontStretch
                    ),
                textBlock.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        DispatcherTimer timer = new DispatcherTimer();
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                e.Handled = true;
                isMoving = false;
                isDragging.Text = "false";

                    if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
                    {
                        anim2 = MainGrid.FindResource("Close") as Storyboard;
                        (anim2.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(150, 0, -150, 0);
                        BorderRadiusRight.BeginStoryboard(anim2);
                        GridCircle.BeginStoryboard(anim2);
                    }
                    else
                    {
                        anim2 = MainGrid.FindResource("Close") as Storyboard;
                        (anim2.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(-150, 0, 150, 0);
                        BorderRadiusLeft.BeginStoryboard(anim2);
                        GridCircle.BeginStoryboard(anim2);
                    }
                
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 750);
                timer.Start();

                foreach (Window window in App.Current.Windows)
                {
                    if (!window.IsActive)
                        window.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            this.Visibility = System.Windows.Visibility.Collapsed;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            playBoxSmall.PlayPauseBtn.MouseLeave += new MouseEventHandler(PlayPauseBtn_MouseLeave);
            playBoxSmall.PlayPauseBtn.MouseEnter += new MouseEventHandler(PlayPauseBtn_MouseEnter);
            
            GridCircle.RenderTransform = new TranslateTransform(this.ActualWidth - 150, this.ActualHeight - 150);  
            Update();
            this.Top = 0; this.Left = 0;
            this.Width = System.Windows.SystemParameters.WorkArea.Width;
            this.Height = System.Windows.SystemParameters.WorkArea.Height;

            DistanceFromStart.X = dekstopWorkingArea.Right;
            DistanceFromStart.Y = dekstopWorkingArea.Bottom;

            Open = MainGrid.FindResource("Load") as Storyboard;
            (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(150, 0, -150, 0);
            (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(150, 0, -150, 0);
            GridCircle.BeginStoryboard(Open);
            BorderRadiusRight.BeginStoryboard(Open);
        }

        private void hide_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
            {
                Open = MainGrid.FindResource("OutContainer") as Storyboard;
                BorderRadiusRight.BeginStoryboard(Open);

                Close = MainGrid.FindResource("In") as Storyboard;
                (Close.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[2].Value = new Thickness(20, 0, 0, 0);
                (Close.Children[1] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(20, 0, 0, 0);
                this.BeginStoryboard(Close);
            }
            else
            {
                Open = MainGrid.FindResource("OutContainer") as Storyboard;
                BorderRadiusLeft.BeginStoryboard(Open);

                Close = MainGrid.FindResource("In") as Storyboard;
                (Close.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[2].Value = new Thickness(0, 0, 20, 0);
                (Close.Children[1] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(0, 0, 20, 0);
                this.BeginStoryboard(Close);
            }
                playBoxSmall.IsEnabled = true;

        }

        private void show_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
            {
                anim2 = MainGrid.FindResource("InContainer") as Storyboard;
                BorderRadiusRight.BeginStoryboard(anim2);

                Close = MainGrid.FindResource("Out") as Storyboard;
                this.BeginStoryboard(Close);
            }
            else
            {
                anim2 = MainGrid.FindResource("InContainer") as Storyboard;
                BorderRadiusLeft.BeginStoryboard(anim2);

                Close = MainGrid.FindResource("Out") as Storyboard;
                this.BeginStoryboard(Close);
            }
                playBoxSmall.IsEnabled = false;

        }

        private void BorderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = (sender as Border).ActualHeight / 2;
            BorderRadiusRight.CornerRadius = new CornerRadius(height, 0, 0, height);
            BorderRadiusLeft.CornerRadius = new CornerRadius(0, height, height, 0);
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                    this.Top = 0; this.Left = 0;
                    this.Width = System.Windows.SystemParameters.WorkArea.Width;
                    this.Height = System.Windows.SystemParameters.WorkArea.Height;
                    

                    if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
                    {
                        Open = MainGrid.FindResource("Load") as Storyboard;
                        (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(150, 0, -150, 0);
                        (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(150, 0, -150, 0);
                        GridCircle.BeginStoryboard(Open);
                        BorderRadiusRight.BeginStoryboard(Open);
                    }
                    else if (isLoaded) { isLoaded = false; }
                    else
                    {
                        Open = MainGrid.FindResource("Load") as Storyboard;
                        (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(-150, 0, 150, 0);
                        (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = new Thickness(-150, 0, 150, 0);
                        GridCircle.BeginStoryboard(Open);
                        BorderRadiusLeft.BeginStoryboard(Open);
                    }


            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && isMoving)
            {
                Point MousePoint = Mouse.GetPosition(this);
                DistanceFromStart.X = MousePoint.X - Relative.X - 5 ;
                DistanceFromStart.Y = MousePoint.Y - Relative.Y - 5 ;
                GridCircle.RenderTransform = new TranslateTransform(DistanceFromStart.X, DistanceFromStart.Y);

                isDragging.Text = "true";

                if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
                    circleDirection.Text = "right";
                else
                    circleDirection.Text = "left";
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point RelativeMousePoint = Mouse.GetPosition(gridBig);
            Relative.X = RelativeMousePoint.X;
            Relative.Y = RelativeMousePoint.Y;

            if (isDragging.Text == "True")
            {
                Open = this.FindResource("Dropping") as Storyboard;
                (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(0);
                BorderRadiusLeft.BeginStoryboard(Open);
                BorderRadiusRight.BeginStoryboard(Open);

                anim2 = this.FindResource("DropFade") as Storyboard;
                (anim2.Children[0] as DoubleAnimation).To = 1;
                (anim2.Children[0] as DoubleAnimation).BeginTime = new TimeSpan(0);
                BeginStoryboard(anim2);

            }
            if(GridCircleChild.ActualHeight == 150)
                isMoving = true;
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMoving && isDragging.Text == "true")
            {
                isMoving = false;
                isDragging.Text = "false";

                anim = this.FindResource("Drop") as Storyboard;
                Open = this.FindResource("Dropping") as Storyboard;
                Close = this.FindResource("DropOut") as Storyboard;
                
                if ((DistanceFromStart.X + 77.5) >= this.ActualWidth / 2)
                {
                    (anim.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = this.ActualWidth - 149;
                    (anim.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = this.ActualHeight - 150;
                    GridCircle.BeginStoryboard(anim);
                    (Close.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(-150, 0, 0, 0);
                    BorderRadiusLeft.BeginStoryboard(Close);
                    (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(0);
                    BorderRadiusRight.BeginStoryboard(Open);
                }
                else
                {
                    (anim.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = -1;
                    (anim.Children[1] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = this.ActualHeight - 150;
                    GridCircle.BeginStoryboard(anim);
                    (Close.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(0, 0, -150, 0);
                    BorderRadiusRight.BeginStoryboard(Close);
                    (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(0);
                    BorderRadiusLeft.BeginStoryboard(Open);

                }
                anim2 = this.FindResource("DropFade") as Storyboard;
                (anim2.Children[0] as DoubleAnimation).To = 0;
                (anim2.Children[0] as DoubleAnimation).BeginTime = new TimeSpan(0, 0, 1);
                BeginStoryboard(anim2);

            }

           
        }

        private void isDragging_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isDragging.Text == "true")
            {
                Open = this.FindResource("Dropping") as Storyboard;
                (Open.Children[0] as ThicknessAnimationUsingKeyFrames).KeyFrames[0].Value = new Thickness(0);
                BorderRadiusLeft.BeginStoryboard(Open);
                BorderRadiusRight.BeginStoryboard(Open);

                anim2 = this.FindResource("DropFade") as Storyboard;
                (anim2.Children[0] as DoubleAnimation).To = 1;
                (anim2.Children[0] as DoubleAnimation).BeginTime = new TimeSpan(0);
                BeginStoryboard(anim2);
                
            }
        }

        private void circleDirection_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (circleDirection.Text == "right")
            {
                hide.RenderTransform = new RotateTransform(0);
                hide.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                show.RenderTransform = new RotateTransform(0);
                show.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                GridCircleChild.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

                Open = GridCircle.FindResource("Larger") as Storyboard;
                BorderRadiusRight.BeginStoryboard(Open);

                anim2 = GridCircle.FindResource("Smaller") as Storyboard;
                BorderRadiusLeft.BeginStoryboard(anim2);
            }
            else
            {
                hide.RenderTransform = new RotateTransform(180);
                hide.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                show.RenderTransform = new RotateTransform(180);
                show.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                GridCircleChild.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                
                Open = GridCircle.FindResource("Smaller") as Storyboard;
                BorderRadiusRight.BeginStoryboard(Open);

                anim2 = GridCircle.FindResource("Larger") as Storyboard;
                BorderRadiusLeft.BeginStoryboard(anim2);


            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
        
        }

    }
}
