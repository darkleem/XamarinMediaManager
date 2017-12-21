using System;
using System.Diagnostics;
using ElmSharp;
using Plugin.MediaManager.Abstractions;
using Tizen.Multimedia;

namespace Plugin.MediaManager
{
    public class VideoSurface : Box, IVideoSurface, IDisposable
    {
        public MediaView MediaView { get; set; } = null;

        public VideoSurface(EvasObject parent) : base(parent)
        {
            var mediaView = new global::Tizen.Multimedia.MediaView(parent)
            {
                Geometry = this.Geometry,
            };
            SetMediaView(mediaView);
            SetLayoutCallback(OnLayoutUpdated);

            Show();
        }

        public void SetMediaView(MediaView view)
        {
            UnPackAll();
            MediaView = view;
            if (MediaView != null)
            {
                MediaView.Show();
                PackEnd(view);
            }
        }

        void OnLayoutUpdated()
        {
            //if (MediaView != null)
            //{
            //    Debug.WriteLine($"@@@@@@@@@@@@ VideoSurface.OnLayoutUpdated");
            //    MediaView.Geometry = new Rect(Geometry.X, Geometry.Y, Geometry.Width, Geometry.Height);
            //}
        }

        protected override void OnUnrealize()
        {
            SetLayoutCallback(null);
            base.OnUnrealize();
        }

        #region IDisposable
        public bool IsDisposed => disposed;

        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Debug.WriteLine($"@@@@@@@ Dispose()");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Debug.WriteLine($"@@@@@@@@@@@ Dispose Video Surface");
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            Debug.WriteLine($"@@@@@@@@@@@ Dispose Video Surface unmanaged");

            disposed = true;
        }

        ~VideoSurface()
        {
            Debug.WriteLine($"@@@@@@@  ~VideoSurface()");

            Dispose(false);
        }
        #endregion
    }
}
