using System;
using System.Collections.Generic;
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
        readonly Player _player = new Player();
        MediaPlayerStatus _status;
        IMediaFile _currentMediaFile;
        int _lastRequestedSeekPosition;
        long _previousSeekTime = -1L;

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
                if (_player.State == PlayerState.Playing)
                {
                    _playProgressTimer.Change(0, 100);
                }
                else
                {
                    _playProgressTimer.Change(0, int.MaxValue);
                }
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(_status));
            }
        }

        public TimeSpan Position
        {
            get
            {
                try
                {
                    return TimeSpan.FromMilliseconds(_player.GetPlayPosition());
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
                    return TimeSpan.FromMilliseconds(_player.StreamInfo.GetDuration());
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
            _volumeManager = volumeManager;
            _playProgressTimer = new Timer(state =>
            {
                if (_player.State == PlayerState.Playing)
                {
                    var currentPosition = Position;
                    var currentDuration = Duration;
                    var progress = currentPosition.TotalSeconds / currentDuration.TotalSeconds;
                    if (double.IsInfinity(progress))
                        progress = 0;
                    PlayingChanged?.Invoke(this, new PlayingChangedEventArgs(progress, currentPosition, currentDuration));
                }
            }, null, 0, int.MaxValue);

            _player.ErrorOccurred += (sender, args) =>
            {
                Status = MediaPlayerStatus.Failed;
                MediaFailed?.Invoke(this, new MediaFailedEventArgs(args.Error.ToString(), new Exception("Error : MediaPlayerStatus Failed")));
            };

            _player.PlaybackCompleted += (sender, args) =>
            {
                MediaFinished?.Invoke(this, new MediaFinishedEventArgs(_currentMediaFile));
            };


            _player.BufferingProgressChanged += (sender, args) =>
            {
                //This seems not to be fired at all
                BufferingChanged?.Invoke(this, new BufferingChangedEventArgs(args.Percent, new TimeSpan(_player.GetPlayPosition())));
            };

            _volumeManager.Muted = _player.Muted;
            int.TryParse((_player.Volume * 100).ToString(), out int vol);
            _volumeManager.CurrentVolume = vol;
            _volumeManager.VolumeChanged += VolumeManagerOnVolumeChanged;
        }

        private void VolumeManagerOnVolumeChanged(object sender, Abstractions.EventArguments.VolumeChangedEventArgs e)
        {
            int.TryParse((e.NewVolume * 100).ToString(), out int vol);
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
                    InternalPlay();
                    return;
                }

                if (mediaFile != null)
                {
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

        public async Task Seek(TimeSpan position)
        {
            int second = (int)position.TotalSeconds;
            if (_lastRequestedSeekPosition == second || _player == null)
                return;

            var nowTicks = DateTime.Now.Ticks;
            _lastRequestedSeekPosition = second;

            if (_previousSeekTime == -1L)
                _previousSeekTime = nowTicks;
            var diffInMilliseconds = (nowTicks - _previousSeekTime) / TimeSpan.TicksPerMillisecond;

            if (diffInMilliseconds < 1000)
                await Task.Delay(TimeSpan.FromMilliseconds(2000));

            _previousSeekTime = nowTicks;

            if (_lastRequestedSeekPosition != second)
                return;

            await _player.SetPlayPositionAsync(_lastRequestedSeekPosition, false);
        }

        public Task Stop()
        {
            InternalStop();
            return Task.CompletedTask;
        }

        protected virtual void PlayerInitialize()
        {
        }

        async Task InternalPrepareAsync()
        {
            Status = MediaPlayerStatus.Loading;
            if (_player.State == PlayerState.Idle)
            {
                PlayerInitialize();
                _lastRequestedSeekPosition = -1;
                await _player.PrepareAsync();
            }
        }

        void InternalSetSource(string uri)
        {
            if (_player.State != PlayerState.Idle)
            {
                _player.Unprepare();
            }

            Status = MediaPlayerStatus.Loading;
            _player.SetSource(new MediaUriSource(uri));
        }

        void InternalPlay()
        {
            if (_player.State == PlayerState.Ready || _player.State == PlayerState.Paused)
            {
                _player.Start();
                if (Position.TotalMilliseconds <= 0)
                {
                    Status = MediaPlayerStatus.Stopped;
                }
                else
                {
                    Status = MediaPlayerStatus.Playing;
                }
            }
        }

        void InternalPause()
        {
            if (_player.State == PlayerState.Playing)
            {
                _player.Pause();
                Status = MediaPlayerStatus.Paused;
            }
        }

        void InternalStop()
        {
            if (_player.State == PlayerState.Playing || _player.State == PlayerState.Paused)
            {
                _player.Stop();
                Status = MediaPlayerStatus.Stopped;
            }
        }
    }
}