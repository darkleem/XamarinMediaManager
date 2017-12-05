using Plugin.MediaManager.Abstractions;

namespace Plugin.MediaManager
{
    public class AudioPlayerImplementation : MediaPlayerBase, IAudioPlayer
    {
        public AudioPlayerImplementation(IVolumeManager volumeManager) : base(volumeManager)
        {
        }
    }
}