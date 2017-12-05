using System;
using ElmSharp;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using Tizen.Multimedia;

namespace Plugin.MediaManager
{
    public class VideoPlayerImplementation : MediaPlayerBase, IVideoPlayer
    {
        private IVideoSurface _renderSurface;

        public VideoPlayerImplementation(IVolumeManager volumeManager) : base(volumeManager)
        {
        }

        public IVideoSurface RenderSurface
        {
            get { return _renderSurface; }
            set
            {
                if (!(value is VideoSurface))
                    throw new ArgumentException("Not a valid video surface");

                _renderSurface = (VideoSurface)value;
                PlayerInitialize();
            }
        }

        public VideoAspectMode AspectMode { get; set; }

        public bool IsReadyRendering => RenderSurface != null && !RenderSurface.IsDisposed;

        public bool IsMuted
        {
            get { return Player.Muted; }
            set { Player.Muted = value; }
        }

        public void SetVolume(float leftVolume, float rightVolume)
        {
            float volume = Math.Max(leftVolume, rightVolume);
            Player.Volume = volume;
        }

        protected override void PlayerInitialize()
        {
            if (Player.State != PlayerState.Idle)
                Player.Unprepare();

            if (RenderSurface is MediaView mediaView)
            {
                // On TV profile, it only works when the Display object type is 'Window'.
                // When creating ElmSharp.MediaView object, if the parent is not set to Window, this would not work properly.
                if (Elementary.GetProfile() == "tv")
                {
                    if (mediaView.Parent is Window window)
                    {
                        Player.DisplaySettings.Mode = PlayerDisplayMode.Roi;
                        if (mediaView.Geometry.Width != 0 && mediaView.Geometry.Height != 0)
                            Player.DisplaySettings.SetRoi(new Tizen.Multimedia.Rectangle(mediaView.Geometry.X, mediaView.Geometry.Y, mediaView.Geometry.Width, mediaView.Geometry.Height));
                        Player.Display = new Display(window);
                    }
                    else
                    {
                        throw new InvalidCastException("Only the Window object can be used in the TV Profile.");
                    }
                }
                else
                {
                    Player.Display = new Display(mediaView);
                }
            }
            else
            {
                throw new InvalidCastException("Only the MediaView object can be used.");
            }
        }
    }
}