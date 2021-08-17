using System;
using System.IO;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicPlayer.Enums;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SongQueue songQueue = new SongQueue();
        List<Uri> songList = new();
        string[] validFileExtentions = { ".wav", ".mp3" };
        const string SONG_DIRECTORY = @"D:\Steam\steamapps\common\LobotomyCorp\OST";
        public MainWindow()
        {
            InitializeComponent();
            PopulateSongList();
            songQueue.ImportSongList(songList);
            MusicElement.Source = songQueue.GetCurrentSong();

            MusicElement.LoadedBehavior = MediaState.Manual;
            MusicElement.UnloadedBehavior = MediaState.Stop;
            MusicElement.Volume = 0.10;
        }

        private void MusicElement_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PopulateSongList()
        {
            string[] files = Directory.GetFiles(SONG_DIRECTORY);
            songList.AddRange(files
                .Where(file => IsValidFileExtension(file))
                .Select(file => new Uri(file)));
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

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MusicElement.Pause();
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
            MusicElement.Source = nextSong;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MusicElement.Stop();
            MusicElement.Play();
        }
        private void BackButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MusicElement.Source = songQueue.GetPreviousSong();
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicElement.Volume = (double)VolumeSlider.Value;
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!MusicElement.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            double seekTime = TimelineSlider.Value / 10 * MusicElement.NaturalDuration.TimeSpan.TotalMilliseconds;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, milliseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
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
            
        }
    }

}
