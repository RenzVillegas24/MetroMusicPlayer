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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

using BandedSpectrumAnalyzer;
using Un4seen.Bass;
using System.Collections.ObjectModel;

namespace Metro.PlayerInterface
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : UserControl
    {
        Storyboard Open;
        Storyboard Open1;
        Storyboard Open2;
        Storyboard Close;

        public Playlist()
        {
            InitializeComponent();
            try
            {
                BassEngine.Instance.PropertyChanged += BassEngine_PropertyChangedPl;
                spectrumMuzic.RegisterSoundPlayer(BassEngine.Instance);
                UIHelper.Bind(BassEngine.Instance, "ChannelLength", progressTime, Slider.MaximumProperty);

            }
            catch { }
        }

        #region Bass Engine Events
        private void BassEngine_PropertyChangedPl(object sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "FileTag":
                    Dispatcher.BeginInvoke(new Action(() =>
              {
                  if (BassEngine.Instance.FileTag != null)
                  {

                      Close = playBoxControl.FindResource("Open-Close") as Storyboard;
                      playBoxControl.BeginStoryboard(Close);

                      UpdateLayout();
                      TagLib.Tag tag = BassEngine.Instance.FileTag.Tag;
                      AlbumText.Content = tag.Album;
                      ArtistText.Content = tag.Artists.Length > 0 ? tag.Artists[0] : string.Empty;
                      TitleText.Content = tag.Title;
                      try
                      {
                          if (tag.Pictures.Length > 0)
                          {
                              speakerAnimationAnalyzer1.Visibility = System.Windows.Visibility.Hidden;
                              using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data))
                              {
                                  BitmapImage albumImage = new BitmapImage();

                                  albumImage.BeginInit();
                                  albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                  albumImage.StreamSource = albumArtworkMemStream;
                                  albumImage.EndInit();
                                  albumArt.Source = albumImage;

                                  //Blur

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
                                  playBoxBg.Background = new ImageBrush() { Stretch = Stretch.Fill, ImageSource = rtb };

                                  albumArtworkMemStream.Close();

                              }

                          }
                          else
                          {
                              albumArt.Source = null;
                              playBoxBg.Background = null;
                              speakerAnimationAnalyzer1.Visibility = System.Windows.Visibility.Visible;
                          }
                      }
                      catch { }
                      finally { }
                  }
                  else
                  {
                      AlbumText.Content = string.Empty;
                      ArtistText.Content = string.Empty;
                      TitleText.Content = string.Empty;
                      albumArt.Source = null;
                      playBoxBg.Background = null;
                  }
              }));
                    break;

                case "ChannelPosition":
                    progressTime.Value = BassEngine.Instance.ChannelPosition;

                    if (BassEngine.Instance.ChannelPosition == BassEngine.Instance.ChannelPosition - 2 && btnAllSongContent.SelectedIndex != btnAllSongContent.Items.Count - 1)
                    {
                        btnAllSongContent.SelectedIndex = btnAllSongContent.SelectedIndex + 1;
                    }
                    break;

                case "IsPlaying":
                        if (playBoxControl.IsMouseOver)
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
                    break;
            
            }

        }
        #endregion
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            Storyboard Enter;
            Storyboard Close;

            string title = "";

            Dispatcher.BeginInvoke(new Action(() =>
                {

                    foreach (string files in GetFileList("*.mp3", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)))
                    {

                        TagLib.File tagFile = TagLib.File.Create(files);

                        if (tagFile.Tag.Title == null)
                            title = files.Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\", "").Replace(".mp3", "");
                        else
                            title = tagFile.Tag.Title;

                        if (tagFile.Tag.Pictures.Length > 0)
                        {
                            using (MemoryStream albumArtworkMemStream = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data))
                            {
                                BitmapImage albumImage = new BitmapImage();

                                albumImage.BeginInit();
                                albumImage.DecodePixelWidth = 100;
                                albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                albumImage.StreamSource = albumArtworkMemStream;
                                albumImage.EndInit();

                                btnAllSongContent.Items.Add(new MusicList()
                                {

                                    fullPath = files,
                                    title = title,
                                    artist = tagFile.Tag.Artists.Length > 0 ? tagFile.Tag.Artists[0] : string.Empty,
                                    album = tagFile.Tag.Album,
                                    thumb = albumImage

                                });
                            }
                        }
                        else
                        {
                            btnAllSongContent.Items.Add(new MusicList()
                            {

                                fullPath = files,
                                title = title,
                                artist = tagFile.Tag.Artists.Length > 0 ? tagFile.Tag.Artists[0] : string.Empty,
                                album = tagFile.Tag.Album,
                                thumb = null

                            });
                        }

                    }
                }));
        }

        public static double FileListLength;

        public static IEnumerable<string> GetFileList(string fileExtension, string rootFolder)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolder);
            string[] tmp;
            
                rootFolder = pending.Dequeue();
                tmp = Directory.GetFiles(rootFolder,fileExtension,SearchOption.AllDirectories);
                FileListLength = tmp.Length;
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }

            
        }

        private void animatedListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MusicList musicList = (MusicList)btnAllSongContent.SelectedItem;
            if (musicList != null)
            {
                btnAllSongContent.Tag = null;
                string URL = musicList.fullPath;

                BandedSpectrumAnalyzer.BassEngine.Instance.OpenFile(URL);
                BandedSpectrumAnalyzer.BassEngine.Instance.Play();
            }
        }

     

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          
            
        }

        private void playBoxControl_MouseEnter(object sender, MouseEventArgs e)
        {
                if (BassEngine.Instance.CanPlay || BassEngine.Instance.CanStop)
                {
                    Open = playBoxControl.FindResource("Open") as Storyboard;
                    playBoxControl.BeginStoryboard(Open);
                }
        }

        private void playBoxControl_MouseLeave(object sender, MouseEventArgs e)
        {
                if (BassEngine.Instance.IsPlaying)
                {
                    Close = playBoxControl.FindResource("Close") as Storyboard;
                    playBoxControl.BeginStoryboard(Close);
                }
            
        }
       public void SearchToggle()
        {
            if (searchArea.Tag.ToString() == "Opened")
            {
                searchArea.Tag = "Closed";
                Open = searchArea.FindResource("Out") as Storyboard;

                searchArea.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Open.Children[0]);
                searchArea.Background = new SolidColorBrush(Colors.Transparent);
                searchArea.Background.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Open.Children[1]);
                searchBox.BeginAnimation(VisibilityProperty, (ObjectAnimationUsingKeyFrames)Open.Children[2]);
                ((Open.Children[3] as DoubleAnimationUsingKeyFrames).KeyFrames[0] as SplineDoubleKeyFrame).Value = 15;
                ((Open.Children[4] as DoubleAnimationUsingKeyFrames).KeyFrames[0] as SplineDoubleKeyFrame).Value = 15;
                searchBoxIcon.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Open.Children[3]);
                searchBoxIcon.BeginAnimation(HeightProperty, (DoubleAnimationUsingKeyFrames)Open.Children[4]);
                searchBoxIcon.Fill = new SolidColorBrush(Colors.Black);
                searchBoxIcon.Fill.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Open.Children[5]);
                ((Open.Children[6] as ObjectAnimationUsingKeyFrames).KeyFrames[0] as DiscreteObjectKeyFrame).Value = Geometry.Parse("F1 M 42.5,22C 49.4036,22 55,27.5964 55,34.5C 55,41.4036 49.4036,47 42.5,47C 40.1356,47 37.9245,46.3435 36,45.2426L 26.9749,54.2678C 25.8033,55.4393 23.9038,55.4393 22.7322,54.2678C 21.5607,53.0962 21.5607,51.1967 22.7322,50.0251L 31.7971,40.961C 30.6565,39.0755 30,36.8644 30,34.5C 30,27.5964 35.5964,22 42.5,22 Z M 42.5,26C 37.8056,26 34,29.8056 34,34.5C 34,39.1944 37.8056,43 42.5,43C 47.1944,43 51,39.1944 51,34.5C 51,29.8056 47.1944,26 42.5,26 Z ");
                searchBoxIcon.BeginAnimation(System.Windows.Shapes.Path.DataProperty, (ObjectAnimationUsingKeyFrames)Open.Children[6]);
                searchBoxIconA.BeginAnimation(RotateTransform.AngleProperty, (DoubleAnimationUsingKeyFrames)Open.Children[7]);
               
            }
            else if (searchArea.Tag.ToString() == "Closed")
            {
                if (listArrangement.Tag.ToString() == "Opened")
                    ListArrangementToggle();

                searchArea.Tag = "Opened";
                Close = searchArea.FindResource("In") as Storyboard;
                
                searchArea.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Close.Children[0]);
                searchArea.Background = new SolidColorBrush(Colors.Transparent);
                searchArea.Background.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Close.Children[1]);
                searchBox.BeginAnimation(VisibilityProperty, (ObjectAnimationUsingKeyFrames)Close.Children[2]);
                searchBoxIcon.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Close.Children[3]);
                searchBoxIcon.BeginAnimation(HeightProperty, (DoubleAnimationUsingKeyFrames)Close.Children[4]);
                searchBoxIcon.Fill = new SolidColorBrush(Colors.White);
                searchBoxIcon.Fill.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Close.Children[5]);
                searchBoxIcon.BeginAnimation(System.Windows.Shapes.Path.DataProperty, (ObjectAnimationUsingKeyFrames)Close.Children[6]);
                searchBoxIconA.BeginAnimation(RotateTransform.AngleProperty, (DoubleAnimationUsingKeyFrames)Close.Children[7]);
                
                Keyboard.Focus(searchBox);
                searchBox.SelectAll();
            }
        }

       public void ListArrangementToggle()
       {
           if (listArrangement.Tag.ToString() == "Opened")
           {
               listArrangement.Tag = "Closed";
               Open = searchArea.FindResource("Out") as Storyboard;

               listArrangement.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Open.Children[0]);
               listArrangement.Background = new SolidColorBrush(Colors.Transparent);
               listArrangement.Background.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Open.Children[1]);
               listSelection.BeginAnimation(VisibilityProperty, (ObjectAnimationUsingKeyFrames)Open.Children[2]);
               ((Open.Children[3] as DoubleAnimationUsingKeyFrames).KeyFrames[0] as SplineDoubleKeyFrame).Value = 12;
               ((Open.Children[4] as DoubleAnimationUsingKeyFrames).KeyFrames[0] as SplineDoubleKeyFrame).Value = 12;
               listAIcon.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Open.Children[3]);
               listAIcon.BeginAnimation(HeightProperty, (DoubleAnimationUsingKeyFrames)Open.Children[4]);
               listAIcon.Fill = new SolidColorBrush(Colors.Black);
               listAIcon.Fill.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Open.Children[5]);
               ((Open.Children[6] as ObjectAnimationUsingKeyFrames).KeyFrames[0] as DiscreteObjectKeyFrame).Value = Geometry.Parse("F1M19,23L27,23 27,31 19,31 19,23z M19,34L27,34 27,42 19,42 19,34z M31,23L57,23 57,31 31,31 31,23z M19,45L27,45 27,53 19,53 19,45z M31,34L57,34 57,42 31,42 31,34z M31,45L57,45 57,53 31,53 31,45z");
               listAIcon.BeginAnimation(System.Windows.Shapes.Path.DataProperty, (ObjectAnimationUsingKeyFrames)Open.Children[6]);
               listAIconA.BeginAnimation(RotateTransform.AngleProperty, (DoubleAnimationUsingKeyFrames)Open.Children[7]);

           }
           else if (listArrangement.Tag.ToString() == "Closed")
           {
               if (searchArea.Tag.ToString() == "Opened")
                   SearchToggle();

               listArrangement.Tag = "Opened";
               Close = searchArea.FindResource("In") as Storyboard;
               listArrangement.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Close.Children[0]);
               listArrangement.Background = new SolidColorBrush(Colors.Transparent);
               listArrangement.Background.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Close.Children[1]);
               listSelection.BeginAnimation(VisibilityProperty, (ObjectAnimationUsingKeyFrames)Close.Children[2]);
               listAIcon.BeginAnimation(WidthProperty, (DoubleAnimationUsingKeyFrames)Close.Children[3]);
               listAIcon.BeginAnimation(HeightProperty, (DoubleAnimationUsingKeyFrames)Close.Children[4]);
               listAIcon.Fill = new SolidColorBrush(Colors.White);
               listAIcon.Fill.BeginAnimation((SolidColorBrush.ColorProperty), (ColorAnimationUsingKeyFrames)Close.Children[5]);
               listAIcon.BeginAnimation(System.Windows.Shapes.Path.DataProperty, (ObjectAnimationUsingKeyFrames)Close.Children[6]);
               listAIconA.BeginAnimation(RotateTransform.AngleProperty, (DoubleAnimationUsingKeyFrames)Close.Children[7]);

           }
       }
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
           SearchToggle();
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchBox.Text;
            btnAllSongContent.Items.Filter = delegate(object obj)
            {
                MusicList musicList = (MusicList)obj;
                string str = musicList.title.ToString().ToLower();
                if (string.IsNullOrEmpty(str)) return false;
                int index = str.IndexOf(searchText.ToLower(), 0);
                return (index > -1);
            };
        }

        private void searchButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (searchArea.Tag.ToString() == "Closed")
            {
                Open1 = searchArea.FindResource("Hover") as Storyboard;
                searchArea.BeginStoryboard(Open1);
            }
        }

        private void listAButton_Click(object sender, RoutedEventArgs e)
        {
            ListArrangementToggle();
        }


        private int ListIndex;
        private void MIAddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
        }
        private void MIAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
        }
        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
            MusicList file = (MusicList)btnAllSongContent.Items[ListIndex];
            FileInfo fileInfo = new FileInfo(file.fullPath);
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the file?\r\n\r\nFile Name: " + fileInfo.Name + "\r\nPath: " + file.fullPath + "\r\nSize: " + Math.Round((double)((double)fileInfo.Length / (1024 * 1024)), 2) + " mb",
                                                     "Delete File", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                File.Delete(file.fullPath);
            btnAllSongContent.Items.RemoveAt(ListIndex);
            }
        }
        private void MIRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
            btnAllSongContent.Items.RemoveAt(ListIndex);
        }
        private void MIShare_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
        }
        private void MIInformation_Click(object sender, RoutedEventArgs e)
        {
            if (ListIndex == -1)
                return;
            MusicList file = (MusicList)btnAllSongContent.Items[ListIndex];
            Window info = new MusicInformation();
            info.Tag = file.fullPath;
            info.ShowDialog();
        }

        private void btnAllSongContent_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListIndex = GetCurrentIndex(btnAllSongContent, e.GetPosition);
            e.Handled=true;
        }

        #region GetListboxIndexFromPosition

        delegate Point GetPositionDelegate(IInputElement element);
        private int GetCurrentIndex(ItemsControl itemsControl,GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0;i < itemsControl.Items.Count; i++)
            {
                ListBoxItem item = GetListBoxItem(itemsControl,i);
                if (item == null)
                    continue;
                if(IsMouseOverTarget(item, getPosition))
                { index = i; break; }
            }
            return index;
        }
        private ListBoxItem GetListBoxItem(ItemsControl itemsControl, int index)
        {
            if (itemsControl.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;
            return itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }
        private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        #endregion

        private void btnAllSongContent_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void refreshButton_MouseEnter(object sender, MouseEventArgs e)
        {
                Open1 = refreshButton.FindResource("Hover") as Storyboard;
                refreshButton.BeginStoryboard(Open1);
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            btnAllSongContent.Items.Clear();

            Open = loadingGrid.FindResource("In") as Storyboard;
            Close = loadingGrid.FindResource("Out") as Storyboard;

            loadingGrid.BeginStoryboard(Open);

            loadingGridLAbel.Content = "Loading";

            string title = "";
            double count = 0;


            loadingGridProgress.Maximum = FileListLength; 
                foreach (string files in GetFileList("*.mp3", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)))
                {
                    

                    //loadingGridProgress.Value = count;
                    Open2 = loadingGrid.FindResource("Value") as Storyboard;
                    (Open2.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = count;

                    count = count + 1;

                    (Open2.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[1].Value = count;
                    loadingGridPercentage.BeginStoryboard(Open2);

                    loadingGridPercentage.Content = Math.Round(((count / FileListLength)*100),1) + "%";
                    TagLib.File tagFile = TagLib.File.Create(files);

                    if (tagFile.Tag.Title == null)
                        title = files.Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\", "").Replace(".mp3", "");
                    else
                        title = tagFile.Tag.Title;

                    if (tagFile.Tag.Pictures.Length > 0)
                    {
                        using (MemoryStream albumArtworkMemStream = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data))
                        {
                            BitmapImage albumImage = new BitmapImage();

                            albumImage.BeginInit();
                            albumImage.DecodePixelWidth = 100;
                            albumImage.CacheOption = BitmapCacheOption.OnLoad;
                            albumImage.StreamSource = albumArtworkMemStream;
                            albumImage.EndInit();


                            btnAllSongContent.Items.Add(new MusicList()
                            {

                                fullPath = files,
                                title = title,
                                artist = tagFile.Tag.Artists.Length > 0 ? tagFile.Tag.Artists[0] : string.Empty,
                                album = tagFile.Tag.Album,
                                thumb = albumImage

                            });
                        }
                    }
                    else
                    {
                        btnAllSongContent.Items.Add(new MusicList()
                        {

                            fullPath = files,
                            title = title,
                            artist = tagFile.Tag.Artists.Length > 0 ? tagFile.Tag.Artists[0] : string.Empty,
                            album = tagFile.Tag.Album,
                            thumb = null

                        });
                    }
                    DoEvents();

                }

            count = 0;

            loadingGridLAbel.Content = "Done";
            loadingGrid.BeginStoryboard(Close);

        }

        static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                                                    (SendOrPostCallback)delegate(object arg)
                                                    {
                                                        var f = arg as DispatcherFrame;
                                                        f.Continue = false;
                                                    }, frame);
            Dispatcher.PushFrame(frame);
        
        }

      /*  public ObservableCollection<T> Shuffle<T>(this ObservableCollection<T> input)
        {
            var n = input.Count;
            while (n > 1)
            {
                var box = new double[1];
                while (!( < n * (Double.MaxValue/n)))

            }
        }
       */


       /*
        private void panelButtonClick(object sender)
        {
            Enter = Menu.FindResource("panelButtons-Checked") as Storyboard;
            (sender as Button).BeginStoryboard(Enter);

            Button btn = (Button)sender;
            foreach (FrameworkElement item in ((Panel)btn.Parent).Children)
            {
                if (btn.Name != item.Name)
                {
                    Close = Menu.FindResource("panelButtons-Normal") as Storyboard;
                    (item as Button).BeginStoryboard(Close);

                    OtherAnim = Menu.FindResource("panelButtonsItems-Out") as Storyboard;
                    OtherAnim.SetValue(Storyboard.TargetNameProperty, item.Name + "Content");
                    (item as Button).BeginStoryboard(OtherAnim);

                }

                else 
                {

                    OtherAnim = Menu.FindResource("panelButtonsItems-Open") as Storyboard;
                    OtherAnim.SetValue(Storyboard.TargetNameProperty, item.Name + "Content");
                    (item as Button).BeginStoryboard(OtherAnim);

                }


            }
       
        }
 */
    }
    public class MusicList : ObservableCollection<MusicList>
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string fullPath { get; set; }
        public BitmapImage thumb { get; set; }
    }
}
