//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Threading.Tasks;
//using Plugin.MediaManager.Abstractions;
//using Plugin.MediaManager.Abstractions.Enums;
//using Plugin.MediaManager.Abstractions.EventArguments;
//using Tizen.Multimedia;
//using System.Diagnostics;

//namespace Plugin.MediaManager
//{
//    public abstract class MediaPlayerBase : IPlaybackManager
//    {
//        IVolumeManager _volumeManager;
//        Timer _playProgressTimer;
//        Player _player;
//        MediaPlayerStatus _status;
//        IMediaFile _currentMediaFile;
//        int _lastSeekPosition;
//        long _previousSeekTime;

//        protected Player Player
//        {
//            get { return _player; }
//            set
//            {
//                _player = value;
//            }
//        }

//        public MediaPlayerStatus Status
//        {
//            get
//            {
//                return _status;
//            }
//            private set
//            {
//                _status = value;
//                if (_player.State == PlayerState.Playing)
//                {
//                    _playProgressTimer.Change(0, 10);
//                }
//                else
//                {
//                    _playProgressTimer.Change(0, int.MaxValue);
//                }
//                StatusChanged?.Invoke(this, new StatusChangedEventArgs(_status));
//            }
//        }

//        public TimeSpan Position
//        {
//            get
//            {
//                if (_player != null)
//                {
//                    if (_player.State == PlayerState.Paused || _player.State == PlayerState.Playing || _player.State == PlayerState.Ready)
//                        return TimeSpan.FromMilliseconds(_player.GetPlayPosition());
//                }
//                return TimeSpan.Zero;
//            }
//        }

//        public TimeSpan Duration
//        {
//            get
//            {
//                return TimeSpan.FromMilliseconds(_player?.StreamInfo?.GetDuration() ?? 0);
//            }
//        }

//        public TimeSpan Buffered
//        {
//            get
//            {
//                if (_player != null)
//                    return TimeSpan.FromMilliseconds(_player.GetDownloadProgress().Current * _player.StreamInfo.GetDuration() * 0.01);

//                return TimeSpan.Zero;
//            }
//        }

//        public Dictionary<string, string> RequestHeaders { get; set; }

//        public event StatusChangedEventHandler StatusChanged;

//        public event PlayingChangedEventHandler PlayingChanged;

//        public event BufferingChangedEventHandler BufferingChanged;

//        public event MediaFinishedEventHandler MediaFinished;

//        public event MediaFailedEventHandler MediaFailed;

//        public MediaPlayerBase(IVolumeManager volumeManager)
//        {
//            InitializePlayer();

//            _volumeManager = volumeManager;
//            _volumeManager.Muted = _player.Muted;
//            int.TryParse((_player.Volume * 100).ToString(), out int vol);
//            _volumeManager.CurrentVolume = vol;
//            _volumeManager.MaxVolume = 1;
//            _volumeManager.VolumeChanged += VolumeManagerOnVolumeChanged;
//        }

//        void InitializePlayer()
//        {
//            _player = new Player();
//            _playProgressTimer = new Timer(state =>
//            {
//                if (_player.State == PlayerState.Playing)
//                {
//                    //Debug.WriteLine($"@@@@@@@@@@ _playProgressTimer call 1");

//                    Debug.WriteLine($"@@@@@@@@@@ _playProgressTimer call 2 : {Position} / {Duration}");

//                    var progress = Position.TotalSeconds / Duration.TotalSeconds;
//                    //Debug.WriteLine($"@@@@@@@@@@ _playProgressTimer call 3 : {progress}");

//                    if (double.IsInfinity(progress))
//                        progress = 0;
//                    Debug.WriteLine($"@@@@@@@@@@ _playProgressTimer call 4 : {progress}");

//                    //PlayingChanged?.Invoke(this, new PlayingChangedEventArgs(progress, currentPosition, currentDuration));
//                    //Debug.WriteLine($"@@@@@@@@@@ _playProgressTimer call end");
//                }
//            }, null, 0, int.MaxValue);

//            _player.ErrorOccurred += (sender, args) =>
//            {
//                Debug.WriteLine($"@@@@@@@@@@ ErrorOccurred call : {args.Error}");
//                Status = MediaPlayerStatus.Failed;
//                MediaFailed?.Invoke(this, new MediaFailedEventArgs(args.Error.ToString(), new Exception("Error : MediaPlayerStatus Failed")));
//            };

//            _player.PlaybackInterrupted += _player_PlaybackInterrupted;
//            _player.PlaybackCompleted += (sender, args) =>
//            {
//                Debug.WriteLine($"@@@@@@@@@@ PlaybackCompleted call");
//                Status = MediaPlayerStatus.Stopped;
//                MediaFinished?.Invoke(this, new MediaFinishedEventArgs(_currentMediaFile));
//            };


//            _player.BufferingProgressChanged += (sender, args) =>
//            {
//                //This seems not to be fired at all
//                Debug.WriteLine($"@@@@@@@@@@ BufferingProgressChanged call  : {args.Percent} :: {Position.Ticks}");
//                BufferingChanged?.Invoke(this, new BufferingChangedEventArgs(args.Percent, new TimeSpan(Position.Ticks)));
//            };
//        }

//        private void _player_PlaybackInterrupted(object sender, PlaybackInterruptedEventArgs e)
//        {
//            Debug.WriteLine($"@@@@@@@@@@ _player_PlaybackInterrupted call");
//        }

//        private void VolumeManagerOnVolumeChanged(object sender, Abstractions.EventArguments.VolumeChangedEventArgs e)
//        {
//            Debug.WriteLine($"@@@@@@@@@@ VolumeManagerOnVolumeChanged");
//            float.TryParse((e.NewVolume / 100).ToString(), out float vol);
//            _player.Volume = vol;
//            _player.Muted = e.Muted;
//        }

//        private void DisposePlayer()
//        {
//            if (_player == null) return;

//            if (_player.State != PlayerState.Idle) _player.Stop();

//            //_player.BufferingProgressChanged -= OnBufferingProgressChanged;
//            //_player.ErrorOccurred -= OnErrorOccurred;
//            //_player.PlaybackCompleted -= OnPlaybackCompleted;
//            //_player.PlaybackInterrupted -= OnPlaybackInterrupted;
//            //_player.SubtitleUpdated -= OnSubtitleUpdated;
//            //_player.VideoFrameDecoded -= OnVideoFrameDecoded;
//            //_player.VideoStreamChanged -= OnVideoStreamChanged;

//            _player.Dispose();
//            _player = null;
//        }


//        public Task Pause()
//        {
//            InternalPause();
//            return Task.CompletedTask;
//        }

//        public async Task Play(IMediaFile mediaFile = null)
//        {
//            try
//            {
//                Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play");

//                var sameMediaFile = mediaFile == null || mediaFile.Equals(_currentMediaFile);
//                if (Status == MediaPlayerStatus.Paused && sameMediaFile)
//                {
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play 1");

//                    InternalPlay();
//                    return;
//                }

//                if (mediaFile != null)
//                {
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play 2");

//                    _currentMediaFile = mediaFile;
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play 3 : {mediaFile.Url}");

//                    //InternalSetSource(mediaFile.Url);
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play 3.2");

//                    await InternalPrepareAsync();
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play 4");

//                    InternalPlay();
//                    Debug.WriteLine($"@@@@@@@@@ MediaPlayerBase Play end");
//                }
//            }
//            catch (Exception e)
//            {
//                //Status = MediaPlayerStatus.Stopped;
//                MediaFailed?.Invoke(this, new MediaFailedEventArgs("Unable to start playback", e));
//            }
//        }

//        public async Task Seek(TimeSpan position)
//        {
//            try
//            {
//                if (_lastSeekPosition == position.Milliseconds || Player == null)
//                    return;

//                _lastSeekPosition = position.Milliseconds;

//                var nowTicks = DateTime.Now.Ticks;
//                if (_previousSeekTime == -1L)
//                    _previousSeekTime = nowTicks;

//                var diffInMilliseconds = (nowTicks - _previousSeekTime) / TimeSpan.TicksPerMillisecond;
//                if (diffInMilliseconds < 1000)
//                    await Task.Delay(TimeSpan.FromMilliseconds(2000));

//                if (Player == null)
//                    return;

//                _previousSeekTime = nowTicks;

//                if (_lastSeekPosition != position.Milliseconds)
//                    return;

//                await Player.SetPlayPositionAsync(_lastSeekPosition, false);
//            }
//            catch
//            {
//                //_controller?.SetMediaPlayState(MediaPlayState.Error);
//                throw;
//            }
//        }

//        public Task Stop()
//        {
//            InternalStop();
//            return Task.CompletedTask;
//        }

//        protected virtual void PlayerInitialize()
//        {
//        }

//        async Task<bool> InternalPrepareAsync()
//        {
//            //Status = MediaPlayerStatus.Loading;
//            //if (_player.State == PlayerState.Idle)
//            //{
//            //    PlayerInitialize();
//            //    //await _player.PrepareAsync();
//            //}

//            if (_currentMediaFile == null || _player == null) return false;
//            try
//            {
//                _lastSeekPosition = -1;
//                //_controller?.SetMediaPlayState(MediaPlayState.Preparing);
//                Debug.WriteLine($"@@@@@@@@@ {_currentMediaFile.Url}");
//                //_player.SetSource(new MediaUriSource(_currentMediaFile.Url));
//                InternalSetSource(_currentMediaFile.Url);
//                //_player.DisplaySettings.Mode = PlayerDisplayMode.CroppedFull;
//                await _player.PrepareAsync();
//                //_controller?.SetMediaPlayState(MediaPlayState.Prepared);
//            }
//            catch
//            {
//                //_controller?.SetMediaPlayState(MediaPlayState.Error);
//                return false;
//            }
//            return true;
//        }

//        void InternalSetSource(string uri)
//        {
//            Debug.WriteLine($"@@@@@@@@@ InternalSetSource : {uri}");

//            if (_player.State != PlayerState.Idle)
//            {
//                _player.Unprepare();
//            }

//            Debug.WriteLine($"@@@@@@@@@ InternalSetSource 2 : {uri}");

//            //Status = MediaPlayerStatus.Loading;
//            try
//            {
//                Debug.WriteLine($"@@@@@@@@@ InternalSetSource 3 : {uri}");

//                _player.SetSource(new MediaUriSource(uri));
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine($"@@@@@@@@@ InternalSetSource Exception : {e.ToString()}");

//            }
//        }

//        void InternalPlay()
//        {
//            if (_player.State == PlayerState.Ready || _player.State == PlayerState.Paused)
//            {
//                Debug.WriteLine($"@@@@@@@@ internal Play");
//                _player.Start();
//                if (Position.TotalMilliseconds <= 0)
//                {
//                    Status = MediaPlayerStatus.Stopped;
//                }
//                else
//                {
//                    Status = MediaPlayerStatus.Playing;
//                }
//                Debug.WriteLine($"@@@@@@@@ internal end");
//            }
//        }

//        void InternalPause()
//        {
//            if (_player.State == PlayerState.Playing)
//            {
//                _player.Pause();
//                Status = MediaPlayerStatus.Paused;
//            }
//        }

//        void InternalStop()
//        {
//            if (_player.State == PlayerState.Playing || _player.State == PlayerState.Paused)
//            {
//                _player.Stop();
//                Status = MediaPlayerStatus.Stopped;
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.EventArguments;
using Tizen.Multimedia;

namespace Plugin.MediaManager
{
    public abstract class MediaPlayerBase : IPlaybackManager
    {
        readonly IVolumeManager _volumeManager;
        readonly Timer _playProgressTimer;
        Player _player = null;
        MediaPlayerStatus _status;
        IMediaFile _currentMediaFile;

        protected Player Player => _player;

        public MediaPlayerStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                //if (_player.State == PlayerState.Playing)
                //{
                //    _playProgressTimer.Change(0, 100);
                //}
                //else
                //{
                //    _playProgressTimer.Change(0, int.MaxValue);
                //}
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(_status));
            }
        }

        public TimeSpan Position
        {
            get
            {
                try
                {
                    if (_player != null && (_player.State == PlayerState.Paused || _player.State == PlayerState.Playing || _player.State == PlayerState.Ready))
                        return TimeSpan.FromMilliseconds(_player.GetPlayPosition());
                    return TimeSpan.Zero;
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                try
                {
                    int tick = _player?.StreamInfo?.GetDuration() ?? 0;
                    return TimeSpan.FromMilliseconds(tick);
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public TimeSpan Buffered
        {
            get
            {
                try
                {
                    return TimeSpan.FromMilliseconds(_player.GetDownloadProgress().Current * _player.StreamInfo.GetDuration() * 0.01);
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
        }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public event StatusChangedEventHandler StatusChanged;

        public event PlayingChangedEventHandler PlayingChanged;

        public event BufferingChangedEventHandler BufferingChanged;

        public event MediaFinishedEventHandler MediaFinished;

        public event MediaFailedEventHandler MediaFailed;

        public MediaPlayerBase(IVolumeManager volumeManager)
        {
            Debug.WriteLine($"@@@@@@@@@@@@@ MediaPlayerBase ctor");

            _volumeManager = volumeManager;

            InitializePlayer();

            //_playProgressTimer = new Timer(state =>
            //{
            //    if (_player.State == PlayerState.Playing)
            //    {
            //        var currentPosition = Position;
            //        var currentDuration = Duration;
            //        var progress = currentPosition.TotalSeconds / currentDuration.TotalSeconds;
            //        if (double.IsInfinity(progress))
            //            progress = 0;
            //        PlayingChanged?.Invoke(this, new PlayingChangedEventArgs(progress, currentPosition, currentDuration));
            //    }
            //}, null, 0, int.MaxValue);

            //_player.ErrorOccurred += (sender, args) =>
            //{
            //    Status = MediaPlayerStatus.Failed;
            //    MediaFailed?.Invoke(this, new MediaFailedEventArgs(args.Error.ToString(), new Exception("Error : MediaPlayerStatus Failed")));
            //};

            //_player.PlaybackCompleted += (sender, args) =>
            //{
            //    MediaFinished?.Invoke(this, new MediaFinishedEventArgs(_currentMediaFile));
            //};


            //_player.BufferingProgressChanged += (sender, args) =>
            //{
            //    //This seems not to be fired at all
            //    BufferingChanged?.Invoke(this, new BufferingChangedEventArgs(args.Percent, new TimeSpan(_player.GetPlayPosition())));
            //};

            //_volumeManager = volumeManager;
            //_volumeManager.Muted = _player.Muted;
            //int.TryParse((_player.Volume * 100).ToString(), out int vol);
            //_volumeManager.CurrentVolume = vol;
            //_volumeManager.MaxVolume = 1;
            //_volumeManager.VolumeChanged += VolumeManagerOnVolumeChanged;

            Debug.WriteLine($"@@@@@@@@@@@@@ MediaPlayerBase End");
        }

        private void VolumeManagerOnVolumeChanged(object sender, Abstractions.EventArguments.VolumeChangedEventArgs e)
        {
            float.TryParse((e.NewVolume / 100).ToString(), out float vol);
            _player.Volume = vol;
            _player.Muted = e.Muted;
        }

        public Task Pause()
        {
            InternalPause();
            return Task.CompletedTask;
        }

        public async Task Play(IMediaFile mediaFile = null)
        {
            try
            {
                var sameMediaFile = mediaFile == null || mediaFile.Equals(_currentMediaFile);
                if (Status == MediaPlayerStatus.Paused && sameMediaFile)
                {
                    Debug.WriteLine($"@@@@@@@@@@ Play(MediaFile) 1 : {mediaFile.Url}");
                    InternalPlay();
                    return;
                }

                if (mediaFile != null)
                {
                    Debug.WriteLine($"@@@@@@@@@@ Play(MediaFile) 2 : {mediaFile.Url}");
                    _currentMediaFile = mediaFile;

                    InternalSetSource(mediaFile.Url);
                    await InternalPrepareAsync();
                    InternalPlay();
                }
            }
            catch (Exception e)
            {
                Status = MediaPlayerStatus.Stopped;
                MediaFailed?.Invoke(this, new MediaFailedEventArgs("Unable to start playback", e));
            }
        }

        public Task Seek(TimeSpan position)
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            InternalStop();
            return Task.CompletedTask;
        }

        protected virtual void PlayerInitialize()
        {
        }

        async Task<bool> InternalPrepareAsync()
        {
            Status = MediaPlayerStatus.Loading;
            try
            {
                //_lastRequestedSeekPosition = -1;
                await _player.PrepareAsync();
            }
            catch
            {
                Debug.WriteLine($"@@@@@@ test 1234");
                return false;
            }
            return true;
        }

        void InternalSetSource(string url)
        {
            Debug.WriteLine($"@@@@@@@@@@ InternalSetSource {url} : {_player?.State}");
            if (_player.State != PlayerState.Idle)
            {
                _player.Unprepare();
            }

            Status = MediaPlayerStatus.Loading;
            _player.SetSource(new MediaUriSource(url));
        }

        void InternalPlay()
        {
            try
            {
                Debug.WriteLine($"@@@@@@@ internal play start");
                _player?.Start();
                if (Position.TotalMilliseconds <= 0)
                {
                    Status = MediaPlayerStatus.Stopped;
                }
                else
                {
                    Status = MediaPlayerStatus.Playing;
                }
            }
            catch
            {
                Debug.WriteLine($"@@@@@@@ internal play error");
                Status = MediaPlayerStatus.Failed;
                throw;
            }
        }

        void InternalPause()
        {
            //if (_player.State == PlayerState.Playing)
            //{
            //    _player.Pause();
            //    Status = MediaPlayerStatus.Paused;
            //}

            try
            {
                Debug.WriteLine($"@@@@@@@ InternalPause");

                _player?.Pause();
                Status = MediaPlayerStatus.Paused;
            }
            catch
            {
                Debug.WriteLine($"@@@@@@@ InternalPause error");
                Status = MediaPlayerStatus.Failed;
                throw;
            }
        }

        void InternalStop()
        {
            try
            {
                Debug.WriteLine($"@@@@@@@ InternalStop");

                _player?.Stop();
                Status = MediaPlayerStatus.Stopped;
            }
            catch
            {
                Debug.WriteLine($"@@@@@@@ InternalStop error");
                Status = MediaPlayerStatus.Failed;
                throw;
            }
        }

        private void InitializePlayer()
        {
            Debug.WriteLine($"@@@@@@@ InitializePlayer");

            _player = new Player();
            //if (CrossMediaManager.Current.VideoPlayer.RenderSurface is VideoSurface surface)
            //{
            //    _player.Display = new Display(surface.MediaView);
            //}

            //_player.BufferingProgressChanged += OnBufferingProgressChanged;
            //_player.ErrorOccurred += OnErrorOccurred;
            //_player.PlaybackCompleted += OnPlaybackCompleted;
            //_player.PlaybackInterrupted += OnPlaybackInterrupted;
        }
        private void DisposePlayer()
        {
            if (_player == null) return;

            if (_player.State != PlayerState.Idle) _player.Stop();

            _player.Dispose();
            _player = null;
        }

        private void ResetPlayer()
        {
            DisposePlayer();
            InitializePlayer();
        }
    }
}