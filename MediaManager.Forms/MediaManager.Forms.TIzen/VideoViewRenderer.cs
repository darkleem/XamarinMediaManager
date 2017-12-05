using System;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Forms;
using Plugin.MediaManager.Forms.Tizen;
using Xamarin.Forms.Platform.Tizen;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;

[assembly: ExportRenderer(typeof(VideoView), typeof(VideoViewRenderer))]
namespace Plugin.MediaManager.Forms.Tizen
{
    public class VideoViewRenderer : ViewRenderer<VideoView, VideoSurface>
    {
        /// <summary>
        /// Used for registration with assembly
        /// </summary>
        public static void Init()
        {
            var temp = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<VideoView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                var _videoSurface = new VideoSurface(TForms.Context.MainWindow);
                SetNativeControl(_videoSurface);

                CrossMediaManager.Current.VideoPlayer.AspectMode = (VideoAspectMode.AspectFill);
                CrossMediaManager.Current.VideoPlayer.RenderSurface = _videoSurface;
            }
        }
    }
}
