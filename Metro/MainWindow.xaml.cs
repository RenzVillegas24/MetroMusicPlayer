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
using System.Windows.Media.Effects;
using BandedSpectrumAnalyzer;
using System.ComponentModel;
using System.Windows.Threading;
using Metro.Lyric;
using Un4seen.Bass;
using System.Diagnostics;

namespace Metro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BassEngine.Instance.PropertyChanged += BassEngineM_PropertyChanged;
            BassNet.Registration("buddyknox@usa.org", "2X11841782815");
        }

        Storyboard Enter;
        Storyboard Close;
        Storyboard OtherAnim;
        Storyboard Anim1;
        Storyboard Anim2;
        Storyboard ContentAnim;
        Storyboard MenuAnim;
        MyLyric lyric;

        #region Bass Engine Events
        private void BassEngineM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "ActiveStreamHandle":
                    if (Menu.Tag == "open")
                    {
                    MenuAnim = Menu.FindResource("Close") as Storyboard;
                    Menu.BeginStoryboard(MenuAnim);
                    Menu.Tag = "close";
                    foreach (FrameworkElement item in ((Panel)Menu.Children[2]).Children)
                    {
                        Close = Menu.FindResource("menuButtons-Normal") as Storyboard;
                        (item as Button).BeginStoryboard(Close);
                    }
                      foreach (FrameworkElement item in ((Panel)Menu.Children[2]).Children)
                        {
                            Close = Menu.FindResource("menuButtons-Normal") as Storyboard;
                            (item as Button).BeginStoryboard(Close);
                            if (item.Name != menuFavorites.Name
                             && item.Name != menuMusics.Name
                             && item.Name != menuPlaylist.Name
                             && item.Name != menuCustomize.Name
                             && item.Name != menuSettings.Name)
                            {
                                OtherAnim = Menu.FindResource("menuButtonsOpen-Out") as Storyboard;
                                OtherAnim.SetValue(Storyboard.TargetNameProperty, item.Name + "Content");
                                (item as Button).BeginStoryboard(OtherAnim);
                            }
                        
                        }
                    }
                    break;
                case "IsPlaying":
                    if (BassEngine.Instance.IsPlaying)
                    {
                        TskPlPImage.Geometry = Geometry.Parse("M11,10 L17,10 17,26 11,26 M20,10 L26,10 26,26 20,26");
                    }
                    else
                    {
                        TskPlPImage.Geometry = Geometry.Parse("M11,10L18,13.74 18,22.28 11,26 M18,13.74L26,18 26,18 18,22.28");
                    }
                    break;

            }

        }
        #endregion
      
        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
                MenuAnim = Menu.FindResource("Open") as Storyboard;
                Menu.BeginStoryboard(MenuAnim);
                Menu.Tag = "open";        
        }
        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
                MenuAnim = Menu.FindResource("Close") as Storyboard;
                Menu.BeginStoryboard(MenuAnim);
                Menu.Tag = "close";
                foreach (FrameworkElement item in ((Panel)Menu.Children[2]).Children)
                {
                    Close = Menu.FindResource("menuButtons-Normal") as Storyboard;
                    (item as Button).BeginStoryboard(Close);
                    if (item.Name != menuFavorites.Name
                     && item.Name != menuMusics.Name
                     && item.Name != menuPlaylist.Name
                     && item.Name != menuCustomize.Name
                     && item.Name != menuSettings.Name)
                    {
                        OtherAnim = Menu.FindResource("menuButtonsOpen-Out") as Storyboard;
                        OtherAnim.SetValue(Storyboard.TargetNameProperty, item.Name + "Content");
                        (item as Button).BeginStoryboard(OtherAnim);
                    }      
                }
        }

        private void menuButtonClick(object sender)
        {
            Enter = Menu.FindResource("menuButtons-Checked") as Storyboard;
            (sender as Button).BeginStoryboard(Enter);

            Button btn = (Button)sender;
            foreach (FrameworkElement item in ((Panel)btn.Parent).Children)
            {
                if (btn.Name != item.Name)
                {
                    Close = Menu.FindResource("menuButtons-Normal") as Storyboard;
                    (item as Button).BeginStoryboard(Close);
                    if (item.Name != menuFavorites.Name
                     && item.Name != menuMusics.Name
                     && item.Name != menuPlaylist.Name
                     && item.Name != menuCustomize.Name
                     && item.Name != menuSettings.Name)
                    {
                        OtherAnim = Menu.FindResource("menuButtonsOpen-Out") as Storyboard;
                        OtherAnim.SetValue(Storyboard.TargetNameProperty, item.Name + "Content");
                        (item as Button).BeginStoryboard(OtherAnim);
                    }                   
                       
                }

                   
               

            }
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);
            ContentAnim = Menu.FindResource("menuButtonsOpen-In") as Storyboard;
            ContentAnim.SetValue(Storyboard.TargetNameProperty, menuOpenContent.Name);
            menuOpen.BeginStoryboard(ContentAnim);
        }

        private void menuMusics_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);

        }

        void PlaylistToggle()
        {
            if (playlistUI.Tag.ToString() == "Opened")
            {
                playlistUI.Tag = "Closed";
                ContentAnim = MainGrid.FindResource("Out2") as Storyboard;
                ContentAnim.SetValue(Storyboard.TargetNameProperty, playlistUI.Name);
                this.BeginStoryboard(ContentAnim);

                Close = MainGrid.FindResource("In2") as Storyboard;
                Close.SetValue(Storyboard.TargetNameProperty, screenUI.Name);
                this.BeginStoryboard(Close);
                playlistUI.Focus();
            }
            else if (playlistUI.Tag.ToString() == "Closed")
            {
                playlistUI.Tag = "Opened";
                Anim2 = MainGrid.FindResource("In") as Storyboard;
                Anim2.SetValue(Storyboard.TargetNameProperty, playlistUI.Name);
                this.BeginStoryboard(Anim2);


                Anim1 = MainGrid.FindResource("Out") as Storyboard;
                Anim1.SetValue(Storyboard.TargetNameProperty, screenUI.Name);
                this.BeginStoryboard(Anim1);
                screenUI.Focus();
            }
        }

        private void menuPlaylist_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);

            PlaylistToggle();
            CloseMenu_Click(sender, e);
        }

        private void menuFavorites_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);

        }

        private void menuEqualizer_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);
            ContentAnim = Menu.FindResource("menuButtonsOpen-In") as Storyboard;
            ContentAnim.SetValue(Storyboard.TargetNameProperty, menuEqualizerContent.Name);
            menuEqualizer.BeginStoryboard(ContentAnim);
        }

        private void menuCustomize_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);

        }

        private void menuSettings_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);

        }

        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {   menuButtonClick(sender);
            ContentAnim = Menu.FindResource("menuButtonsOpen-In") as Storyboard;
            ContentAnim.SetValue(Storyboard.TargetNameProperty, menuAboutContent.Name);
            menuAbout.BeginStoryboard(ContentAnim);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          
            
        }

        Boolean hasLyric;
        int lyricLineIndex;

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
           
        }

        private void stackPanel1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == System.Windows.WindowState.Maximized)
                {
                    this.WindowState = System.Windows.WindowState.Normal;
                }
                if (this.WindowState == System.Windows.WindowState.Normal)
                {
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.MediaPlayPause)
            {
                if (BassEngine.Instance.IsPlaying)
                    BassEngine.Instance.Pause();
                else
                    BassEngine.Instance.Play();
            }
            if (e.Key == Key.Escape && playlistUI.Tag.ToString() == "Opened")
            {
                PlaylistToggle();
                if (playlistUI.Tag == "Opened")
                {
                    playlistUI.SearchToggle();
                    playlistUI.searchArea.Tag = "Closed";
                }
            }
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                 PlaylistToggle();
                 if (playlistUI.searchArea.Tag == "Opened")
                 {
                     playlistUI.SearchToggle();
                     playlistUI.searchArea.Tag = "Closed";
                 }
            }
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                playlistUI.SearchToggle();
                if (playlistUI.Tag == "Closed")
                {
                    PlaylistToggle();
                    playlistUI.searchArea.Tag = "Opened";
                }
            }
        }

        private void TaskbarPrev_Click(object sender, EventArgs e)
        {

        }

        private void TaskbarPlayPause_Click(object sender, EventArgs e)
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

        private void TaskbarNext_Click(object sender, EventArgs e)
        {

        }

        private void onlinemusicButton_Click(object sender, RoutedEventArgs e)
        {

        }

        Window minPlayer = new MiniPlayer();
        private void minPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            minPlayer.Show();
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
          
           
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

      

      


    }
}
