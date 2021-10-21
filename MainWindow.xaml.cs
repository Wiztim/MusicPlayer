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
        private string SONG_DIRECTORY = "";

        private static readonly string APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string CFGFOLDER_PATH = Path.Combine(APPDATA_PATH, "MusicPlayer");
        private static readonly string CFGFILE_PATH = Path.Combine(CFGFOLDER_PATH, "config.txt");

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            PopulateSongList();
            DisplaySongList();
            UpdateStatusBarProgress();
        }

        private void LoadConfig()
        {
            if (!Directory.Exists(CFGFOLDER_PATH))
            {
                Directory.CreateDirectory(CFGFOLDER_PATH);
            }

            if (!File.Exists(CFGFILE_PATH))
            {
                CreateConfig();
            }

            ReadConfig();
        }

        private void ReadConfig()
        {
            StreamReader cfgReader = File.OpenText(CFGFILE_PATH);
            Dictionary<string, string> settings = new Dictionary<string, string>();

            for (int index = 0; !cfgReader.EndOfStream; index++)
            {
                string settingLine = cfgReader.ReadLine();

                if (!string.IsNullOrWhiteSpace(settingLine))
                {
                    string[] splitSetting = settingLine.Split("=");
                    settings.Add(splitSetting[0], splitSetting[1]);
                }
            }

            if (settings.ContainsKey("SONG_DIRECTORY"))
            {
                SONG_DIRECTORY = settings["SONG_DIRECTORY"];
            }

            if (settings.ContainsKey("VOLUME"))
            {
                MusicElement.Volume = Convert.ToDouble(settings["VOLUME"]);
                VolumeSlider.Value = MusicElement.Volume * 100;
            }

            cfgReader.Close();
        }

        private void CreateConfig()
        {
            StreamWriter cfgWriter = File.CreateText(CFGFILE_PATH);

            string[] cfgDefaults = new string[] { "SONG_DIRECTORY=C:\\Games\\2hu\\Songs\\List\\",
                                                    "VOLUME=0.1"};

            foreach (string setting in cfgDefaults)
            {
                cfgWriter.WriteLine(setting);
            }

            cfgWriter.Close();
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
                PlayButton.Content = "Play";
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
            if (!songList.Contains(songUri))
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
            if (e.Key == Key.Space)
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

        private void Window_Closed(object sender, EventArgs e)
        {
            UpdateConfig();
        }

        private void UpdateConfig()
        {
            StreamWriter cfgUpdater = new StreamWriter(CFGFILE_PATH);
            List<string> latestConfig = new List<string>();

            latestConfig.Add("SONG_DIRECTORY=" + SONG_DIRECTORY);
            latestConfig.Add("VOLUME=" + MusicElement.Volume);

            foreach (string setting in latestConfig)
            {
                cfgUpdater.WriteLine(setting);
            }

            cfgUpdater.Close();
        }
    }
}