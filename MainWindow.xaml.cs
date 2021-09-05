using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicPlayer.Enums;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SongQueue songQueue = new SongQueue();
        private List<Uri> songList = new List<Uri>();
        private readonly string[] validFileExtentions = { ".wav", ".mp3" };
        private const string SONG_DIRECTORY = @"D:\Steam\steamapps\common\LobotomyCorp\OST";
        public MainWindow()
        {
            InitializeComponent();
            PopulateSongList();
            DisplaySongList();
            UpdateStatusBarProgress();
            MusicElement.Volume = 0.10;            
        }


        private void PopulateSongList()
        {
            string[] files = Directory.GetFiles(SONG_DIRECTORY);
            songList.AddRange(files
                .Where(file => IsValidFileExtension(file))
                .Select(file => new Uri(file)));         
        }

        private void DisplaySongList()
        {
            List<string> songStringList = new List<string>();
            foreach (Uri song in songList)
            {
                string[] songString = song.OriginalString.Split("\\");
                songStringList.Add(songString[^1][0..^4]);
            }

            SongListDisplay.ItemsSource = songStringList;
            songQueue.ImportSongList(songList);
            MusicElement.Source = songQueue.GetCurrentSong();
            HighLightCurrentSong();
        }

        private bool IsValidFileExtension(string file)
        {
            return file.Length > 4 && validFileExtentions.Contains(file[^4..]);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)PlayButton.Content == "Play")
            {
                MusicElement.Play();
                PlayButton.Content = "Pause";
            }

            else if ((string)PlayButton.Content == "Pause")
            {
                MusicElement.Pause();
                PlayButton.Content = "Play";
            }
        }

        private void PlayNextSong(object sender, RoutedEventArgs e)
        {
            Uri nextSong = songQueue.GetNextSong();
            if (nextSong == null)
            {
                MusicElement.Stop();
            }
            else if (MusicElement.Source == nextSong)
            {
                MusicElement.Stop();
                MusicElement.Play();
            }

            PlayNewSong(nextSong);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MusicElement.Stop();

            if ((string)PlayButton.Content == "Pause")
            {
                MusicElement.Play();
            }
                
            TimelineSlider.Value = 0;
            PlayButton.Focus();
        }

        private void BackButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MusicElement.Source = songQueue.GetPreviousSong();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicElement.Volume = (double)VolumeSlider.Value / 100;
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!MusicElement.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            double seekTime = TimelineSlider.Value / 100 * MusicElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)seekTime);
            MusicElement.Position = ts;
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            Repeat mode = Repeat.Off;
            RadioButton radioButton = (sender as RadioButton);
            switch (radioButton.Content)
            {
                case "Song":
                    mode = Repeat.Song;
                    break;

                case "Playlist":
                    mode = Repeat.Playlist;
                    break;
            }

            songQueue.SetRepeatMode(mode);
            PlayButton.Focus();
        }

        private void SongListDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlayButton.Content = "Pause";
            Uri songUri = new Uri(SONG_DIRECTORY + SongListDisplay.SelectedItem.ToString() + ".wav");
            if (songList.Contains(songUri))
            {

            } else
            {
                songUri = new Uri(SONG_DIRECTORY + SongListDisplay.SelectedItem.ToString() + ".mp3");
            }

            PlayNewSong(songQueue.PlaySpecificSong(songUri));
        }

        public async Task UpdateStatusBarProgress()
        {
            await Task.Delay(500);
            if (!MusicElement.NaturalDuration.HasTimeSpan)
            {
                await UpdateStatusBarProgress(); ;
            }

            TimelineSlider.Value = 100 * MusicElement.Position / MusicElement.NaturalDuration.TimeSpan;
            await UpdateStatusBarProgress();
        }

        private void PlayNewSong(Uri songUri)
        {
            MusicElement.Source = songUri;
            MusicElement.Play();
            TimelineSlider.Value = 0;
            HighLightCurrentSong();
            PlayButton.Focus();
        }

        private void SpaceBarLetGo(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                PlayButton_Click(sender, e);
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            Random rng = new Random();
            IOrderedEnumerable<Uri> randomizedSongList = songList.OrderBy(item => rng.Next());
            songList = randomizedSongList.ToList();
            DisplaySongList();
            PlayButton.Focus();
        }

        private void HighLightCurrentSong()
        {
            SongListDisplay.SelectedItem = SongListDisplay.Items.GetItemAt(songQueue.GetCurrentIndex());
            SongListDisplay.ScrollIntoView(SongListDisplay.Items.GetItemAt(songQueue.GetCurrentIndex()));
        }
    }
}
