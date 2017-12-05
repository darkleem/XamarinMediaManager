using Tizen.Applications;
using ElmSharp;
using Plugin.MediaManager;
using System.Diagnostics;
using System;

namespace MediaSample.Tizen
{
    class App : CoreUIApplication
    {
        Label TitleLabel, ArtistLabel, AlbumLabel, StateLabel;
        Button PlayButton, PauseButton, StopButton, SkipPlusButton, SkipMinusButton;

        protected override void OnCreate()
        {
            base.OnCreate();
            Initialize();

            CrossMediaManager.Current.PlayingChanged += OnPlayingChanged;
            CrossMediaManager.Current.BufferingChanged += OnBufferingChanged;
            CrossMediaManager.Current.StatusChanged += OnStatusChanged;
            CrossMediaManager.Current.VideoPlayer.RenderSurface = VideoCanvas;
            CrossMediaManager.Current.MediaFileChanged += CurrentOnMediaFileChanged;
        }

        private void OnBufferingChanged(object sender, BufferingChangedEventArgs bufferingChangedEventArgs)
        {
            var bufferingProgress = bufferingChangedEventArgs?.BufferProgress ?? 0;
            var bufferingTime = bufferingChangedEventArgs?.BufferedTime;
            Debug.WriteLine($"buffering progress: {bufferingProgress}, buffering time: {bufferingTime}");
        }

        private void CurrentOnMediaFileChanged(object sender, MediaFileChangedEventArgs mediaFileChangedEventArgs)
        {
            var mediaFile = mediaFileChangedEventArgs.File;
            Title.Text = mediaFile.Metadata.Title ?? "";
            Artist.Text = mediaFile.Metadata.Artist ?? "";
            Album.Text = mediaFile.Metadata.Album ?? "";
            switch (mediaFile.Type)
            {
                case MediaFileType.Audio:
                    if (mediaFile.Metadata.AlbumArt != null)
                    {
                        CoverArt.Source = (ImageSource)mediaFile.Metadata.AlbumArt;
                    }
                    break;
                case MediaFileType.Video:
                    break;
            }
        }

        private async void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        PlayerState.Text = Enum.GetName(typeof(MediaPlayerStatus), e.Status);
                        switch (CrossMediaManager.Current.Status)
                        {
                            case MediaPlayerStatus.Stopped:
                                Progress.Value = 0;
                                break;
                            case MediaPlayerStatus.Paused:
                                break;
                            case MediaPlayerStatus.Playing:
                                Progress.Maximum = 1;
                                break;
                            case MediaPlayerStatus.Buffering:
                                break;
                            case MediaPlayerStatus.Loading:
                                break;
                            case MediaPlayerStatus.Failed:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
        }

        private double CurrentStreamingPosition = 0;
        private async void OnPlayingChanged(object sender, PlayingChangedEventArgs e)
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Progress.Value = e.Progress;
                        CurrentStreamingPosition = e.Position.TotalSeconds;
                    });
        }

        private MediaFile mediaFile;
        private async void PlayUrl(object sender, RoutedEventArgs e)
        {
            if (mediaFile == null)
            {
                mediaFile = new MediaFile("http://www.montemagno.com/sample.mp3", MediaFileType.Audio);
            }
            //await CrossMediaManager.Current.Play(mediaFile);
            //var file = await KnownFolders.VideosLibrary.GetFileAsync("big_buck_bunny.mp4");
            //await CrossMediaManager.Current.Play(file.Path, MediaFileType.VideoFile);
            await CrossMediaManager.Current.Play(@"http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4", MediaFileType.Video);
        }

        private async void Pause(object sender, RoutedEventArgs e)
        {
            await CrossMediaManager.Current.Pause();
        }

        private async void Stop(object sender, EventArgs e)
        {
            await CrossMediaManager.Current.Stop();
        }

        private async void Skip10Seconds(object sender, EventArgs e)
        {
            if (CurrentStreamingPosition > 0)
            {
                Debug.WriteLine($"Before Skipping 10 seconds, Current Position: {CurrentStreamingPosition}");
                CurrentStreamingPosition += 10;
                await CrossMediaManager.Current.PlaybackController.SeekTo(CurrentStreamingPosition);
                await CrossMediaManager.Current.PlaybackController.Play();
                Debug.WriteLine($"After Skipping 10 seconds, Current Position: {CurrentStreamingPosition}");
            }
        }

        private async void Skip30Seconds(object sender, EventArgs e)
        {
            if (CurrentStreamingPosition > 0)
            {
                Debug.WriteLine($"Before Skipping 30 seconds, Current Position: {CurrentStreamingPosition}");
                CurrentStreamingPosition += 30;
                await CrossMediaManager.Current.PlaybackController.SeekTo(CurrentStreamingPosition);
                await CrossMediaManager.Current.PlaybackController.Play();
                Debug.WriteLine($"After Skipping 30 seconds, Current Position: {CurrentStreamingPosition}");
            }
        }

        void Initialize()
        {
            Window window = new Window("MediaSampleTest")
            {
                AvailableRotations = DisplayRotation.Degree_0 | DisplayRotation.Degree_180 | DisplayRotation.Degree_270 | DisplayRotation.Degree_90
            };
            window.BackButtonPressed += (s, e) =>
            {
                Exit();
            };
            window.Show();

            var bg = new Background(window)
            {
                Color = Color.White
            };

            var conformant = new Conformant(window);
            conformant.Show();
            conformant.SetContent(bg);

            var rootBox = new Box(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                WeightY = 1,
            };
            rootBox.Show();
            bg.SetContent(rootBox);

            var mediaSurface = new VideoSurface(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                WeightY = 1,
            };
            mediaSurface.Show();

            var titleBox = CreateHorizontalBoxInLabel(window, "Title : ");
            TitleLabel = new Label(window)
            {
                Text = "",
                Color = Color.Black
            };
            TitleLabel.Show();
            titleBox.PackEnd(TitleLabel);

            var artistBox = CreateHorizontalBoxInLabel(window, "Artist : ");
            ArtistLabel = new Label(window)
            {
                Text = "",
                Color = Color.Black
            };
            ArtistLabel.Show();
            titleBox.PackEnd(ArtistLabel);

            var albumBox = CreateHorizontalBoxInLabel(window, "Title : ");
            AlbumLabel = new Label(window)
            {
                Text = "",
                Color = Color.Black
            };
            AlbumLabel.Show();
            albumBox.PackEnd(AlbumLabel);

            var stateBox = CreateHorizontalBoxInLabel(window, "Player State : ");
            StateLabel = new Label(window)
            {
                Text = "",
                Color = Color.Black
            };
            StateLabel.Show();
            stateBox.PackEnd(StateLabel);

            ProgressBar progress = new ProgressBar(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
            };
            progress.Show();

            var buttonBox = new Box(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                IsHorizontal = true,
            };
            buttonBox.Show();
            PlayButton = new Button(window)
            {
                Text = "Play",
                MinimumWidth = 200,
            };
            PlayButton.Show();
            PauseButton = new Button(window)
            {
                Text = "Pause",
                MinimumWidth = 200,
            };
            PauseButton.Show();
            StopButton = new Button(window)
            {
                Text = "Stop",
                MinimumWidth = 200,
            };
            StopButton.Show();
            SkipPlusButton = new Button(window)
            {
                Text = "Skip+10",
                MinimumWidth = 200,
            };
            SkipPlusButton.Show();
            SkipMinusButton = new Button(window)
            {
                Text = "Skip-10",
                MinimumWidth = 200,
            };
            SkipMinusButton.Show();
            buttonBox.PackEnd(PlayButton);
            buttonBox.PackEnd(PauseButton);
            buttonBox.PackEnd(StopButton);
            buttonBox.PackEnd(SkipPlusButton);
            buttonBox.PackEnd(SkipMinusButton);


            rootBox.PackEnd(mediaSurface);
            rootBox.PackEnd(titleBox);
            rootBox.PackEnd(artistBox);
            rootBox.PackEnd(albumBox);
            rootBox.PackEnd(stateBox);
            rootBox.PackEnd(progress);
            rootBox.PackEnd(buttonBox);

            CrossMediaManager.Current.VideoPlayer.RenderSurface = mediaSurface;
        }

        Box CreateHorizontalBoxInLabel(EvasObject parent, string text)
        {
            var box = new Box(parent)
            {
                IsHorizontal = true,
            };
            box.Resize(500, 100);
            box.Show();
            var label = new Label(box)
            {
                Text = text,
                MinimumWidth = 300,
                Color = Color.Black,
            };
            label.Show();

            box.PackEnd(label);
            return box;
        }

        static void Main(string[] args)
        {
            Elementary.Initialize();
            Elementary.ThemeOverlay();
            App app = new App();
            app.Run(args);
        }
    }
}
