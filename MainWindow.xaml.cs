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

namespace MusiPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Uri> songList = new();
        string[] validFileExtentions = { ".wav", ".mp3" };
        const string SONG_DIRECTORY = @"D:\Steam\steamapps\common\LobotomyCorp\OST";
        public MainWindow()
        {
            InitializeComponent();

        }

        private void MusicElement_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateSongList();
            MusicElement.Source = songList.First();
            MusicElement.LoadedBehavior = MediaState.Manual;
            MusicElement.UnloadedBehavior = MediaState.Stop;
            MusicElement.Volume = 0.10;
            MusicElement.SpeedRatio = 1;
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
            MusicElement.Play();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MusicElement.Pause();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
