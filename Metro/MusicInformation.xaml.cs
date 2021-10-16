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
using System.IO;
using System.Windows.Media.Animation;

namespace Metro
{
    /// <summary>
    /// Interaction logic for MusicInformation.xaml
    /// </summary>
    public partial class MusicInformation : Window
    {
        public MusicInformation()
        {
            InitializeComponent();
        }

        Storyboard Open;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Opacity = 0;
            Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (this.Tag != "Null")
                    {
                        TagLib.File tag = TagLib.File.Create(this.Tag.ToString());
                        this.Title = System.IO.Path.GetFileName(this.Tag.ToString());

                        if (tag.Tag.Pictures.Length > 0)
                        {
                            using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Tag.Pictures[0].Data.Data))
                            {
                                BitmapImage albumImage = new BitmapImage();

                                albumImage.BeginInit();
                                albumImage.CacheOption = BitmapCacheOption.OnLoad;
                                albumImage.StreamSource = albumArtworkMemStream;
                                albumImage.EndInit();
                                albumartImage.Source = albumImage;
                            }
                        }

                        titleBox.Caption.Content = "Title";
                        artistBox.Caption.Content = "Artist";
                        albumBox.Caption.Content = "Album";
                        yearBox.Caption.Content = "Year";
                        discNumberBox.Caption.Content = "Disc Number";
                        trackNumberBox.Caption.Content = "Track Number";
                        genreBox.Caption.Content = "Genre";
                        composerBox.Caption.Content = "Composer";

                        /*
                        tag.Tag.Title = titleBox.TextBox.Text;
                        tag.Tag.Album = albumBox.TextBox.Text;
                        tag.Tag.Year = uint.Parse(yearBox.TextBox.Text);
                        tag.Tag.Disc = uint.Parse(discNumberBox.TextBox.Text);
                        tag.Tag.Track = uint.Parse(trackNumberBox.TextBox.Text);

                        tag.Tag.Title = artistBox.TextBox.Text;
                        tag.Tag.Title = genreBox.TextBox.Text;
                        tag.Tag.Composers = composerBox.TextBox.Text;
                         */

                        titleBox.TextBox.Text = tag.Tag.Title;
                        artistBox.TextBox.Text = tag.Tag.Artists.Length > 0 ? tag.Tag.Artists[0] : string.Empty;
                        albumBox.TextBox.Text = tag.Tag.Album;
                        yearBox.TextBox.Text = tag.Tag.Year.ToString();
                        discNumberBox.TextBox.Text = tag.Tag.Disc.ToString();
                        trackNumberBox.TextBox.Text = tag.Tag.Track.ToString();
                        genreBox.TextBox.Text = tag.Tag.Genres.Length > 0 ? tag.Tag.Genres[0] : string.Empty;
                        composerBox.TextBox.Text = tag.Tag.Composers.Length > 0 ? tag.Tag.Composers[0] : string.Empty;
                        
                        Open = MainGrid.FindResource("Open") as Storyboard;
                        MainGrid.BeginStoryboard(Open);
                       
                    }
                }));
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

