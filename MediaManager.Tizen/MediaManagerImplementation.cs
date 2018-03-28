using System.Diagnostics;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Implementations;

namespace Plugin.MediaManager
{
    /// <summary>
    ///     Implementation for Feature
    /// </summary>
    public class MediaManagerImplementation : MediaManagerBase
    {
        private IAudioPlayer _audioPlayer = null;
        private IVideoPlayer _videoPlayer = null;

        public override IAudioPlayer AudioPlayer
        {
            get { return _audioPlayer ?? (_audioPlayer = new AudioPlayerImplementation(VolumeManager)); }
            set { _audioPlayer = value; }
        }

        public override IVideoPlayer VideoPlayer
        {
            get
            {
                Debug.WriteLine($"@@@@@@@@ VideoPlayer instance");
                return _videoPlayer ?? (_videoPlayer = new VideoPlayerImplementation(VolumeManager));
            }
            set { _videoPlayer = value; }
        }

        public override IMediaNotificationManager MediaNotificationManager { get; set; } = new MediaNotificationManagerImplementation();

        public override IMediaExtractor MediaExtractor { get; set; } = new MediaExtractorImplementation();

        public override IVolumeManager VolumeManager { get; set; } = new VolumeManagerImplementation();
    }
}