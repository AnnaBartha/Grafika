using NAudio.Wave;

namespace Szeminarium1_24_02_17_2
{
    internal class DroneSound
    {
        private static WaveOutEvent outputDevice;
        private static AudioFileReader audioFile;
        private static WaveOutEvent droneOutputDevice;
        private static AudioFileReader droneAudioFile;
        private static bool isPlayingSound;

        public static void Initialize()
        {
            PlayDroneSound("drone_sound.wav");
        }

        private static void PlayDroneSound(string filePath)
        {
            if (droneOutputDevice != null)
            {
                droneOutputDevice.Dispose();
            }
            if (droneAudioFile != null)
            {
                droneAudioFile.Dispose();
            }

            droneAudioFile = new AudioFileReader(filePath);
            droneOutputDevice = new WaveOutEvent();
            droneOutputDevice.Init(droneAudioFile);
            droneOutputDevice.Play();

            // Handle the PlaybackStopped event to loop the sound
            droneOutputDevice.PlaybackStopped += (s, e) =>
            {
                // Restart the drone sound
                droneAudioFile.Position = 0; // Rewind to the beginning
                droneOutputDevice.Play();
            };
        }
    }
}
