using MusicPlayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer
{
    public class SongQueue
    {
        private int currentIndex = 0;
        private List<Uri> songList = new();
        private Repeat repeatMode = Repeat.Off;

        public void ImportSongList(List<Uri> copyList)
        {
            songList.AddRange(copyList);
        }

        public Uri GetCurrentSong()
        {
            return songList[currentIndex];
        }

        public Uri GetNextSong()
        {             
            if (repeatMode == Repeat.Playlist && currentIndex == songList.Count - 1)
            {
                currentIndex = 0;
            }
            else if (repeatMode != Repeat.Song && currentIndex != songList.Count - 1)
            {
                currentIndex++;
            }
            else if (currentIndex == songList.Count - 1)
            {
                return null;
            }

            
            return songList[currentIndex];
        }

        public Uri GetPreviousSong()
        {
            if (currentIndex == 0)
            {
                return songList[currentIndex];
            }

            currentIndex--;
            return songList[currentIndex];
        }
        
        public void SetRepeatMode(Repeat mode)
        {
            repeatMode = mode;
        }

        public Uri PlaySpecificSong(Uri songName)
        {
            currentIndex = songList.IndexOf(songName);
            return songList[currentIndex];
        }
    }
}
