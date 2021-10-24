# Music Player
* This is a music player that reads a path to a folder, and makes a playlist of all compatible file types.
* The program uses WPF to handle the app and C# is used to handle the logic.

## Building
* The current version is intended to run on my machine, you would need to modify the folder it reads from.
* To specify a folder, edit line 21 in [MainWindow.xaml.cs](https://github.com/Wiztim/MusicPlayer/blob/main/MainWindow.xaml.cs#L21)
* This project uses .NET Core 3.1

## Current Features
* Current features are a .mp3 and .wav file support, play,skip, back, and shuffle button, a progress bar, a volume slider, loop options, the list of songs in current play order, and a config file.

## Planned Features
* Future features are ~~a config file,~~ more file support (.ogg), selecting folders from the app, saving playlists, and adding songs to a saved playlist.
