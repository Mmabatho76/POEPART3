using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace POEPART3
{
    public static class AudioPlayer
    {
        public static void PlayGreeting(string filePath)
        {
            // Makes sure the audio file exists first
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Audio file missing: " + filePath);

                return;
            }

            try
            {
                // Loads and plays the greeting
                SoundPlayer player = new SoundPlayer("greeting.wav");

                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Audio playback failed: " + ex.Message);
            }
        }
    }
}
