using Tizen.Applications;
using ElmSharp;
using Plugin.MediaManager;
using System.Diagnostics;
using System;
using Plugin.MediaManager.Abstractions.EventArguments;
using Plugin.MediaManager.Abstractions.Implementations;
using Plugin.MediaManager.Abstractions.Enums;
using Tizen.Multimedia;

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
            TitleLabel.Text = mediaFile.Metadata.Title ?? "";
            ArtistLabel.Text = mediaFile.Metadata.Artist ?? "";
            AlbumLabel.Text = mediaFile.Metadata.Album ?? "";
            switch (mediaFile.Type)
            {
                case MediaFileType.Audio:
                    if (mediaFile.Metadata.AlbumArt != null)
                    {
                        //CoverArt.Source = (ImageSource)mediaFile.Metadata.AlbumArt;
                    }
                    break;
                case MediaFileType.Video:
                    break;
            }
        }

        private async void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            StateLabel.Text = Enum.GetName(typeof(MediaPlayerStatus), e.Status);
            switch (CrossMediaManager.Current.Status)
            {
                case MediaPlayerStatus.Stopped:
                    //Progress.Value = 0;
                    break;
                case MediaPlayerStatus.Paused:
                    break;
                case MediaPlayerStatus.Playing:
                    //Progress.Maximum = 1;
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
        }

        private double CurrentStreamingPosition = 0;
        private void OnPlayingChanged(object sender, PlayingChangedEventArgs e)
        {

        }

        private MediaFile mediaFile;
        private VideoSurface MediaSurface;

        private void PlayUrl(object sender, EventArgs e)
        {
            if (mediaFile == null)
            {
                mediaFile = new MediaFile("https://www.android-examples.com/wp-content/uploads/2016/04/Thunder-rumble.mp3", MediaFileType.Audio);
            }
            //await CrossMediaManager.Current.Play(mediaFile);
            //var file = await KnownFolders.VideosLibrary.GetFileAsync("big_buck_bunny.mp4");
            //await CrossMediaManager.Current.Play(file.Path, MediaFileType.VideoFile);
            Debug.WriteLine($"@@@@@@@@@ play media");
            CrossMediaManager.Current.VideoPlayer.RenderSurface = MediaSurface;
            CrossMediaManager.Current.Play($"{App.Current.DirectoryInfo.Resource}a.mp4", MediaFileType.Video);
            Debug.WriteLine($"@@@@@@@@@ play media end");
        }

        private async void Pause(object sender, EventArgs e)
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

            window.StatusBarMode = StatusBarMode.Transparent;
            var rootBox = new Box(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                WeightY = 1,
            };
            rootBox.Show();
            bg.SetContent(rootBox);

            MediaSurface = new VideoSurface(window)
            {
                AlignmentX = -1,
                AlignmentY = -1,
                WeightX = 1,
                WeightY = 1,
            };
            MediaSurface.Show();

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
                MinimumWidth = 150,
            };
            PlayButton.Show();
            PlayButton.Clicked += PlayUrl;

            PauseButton = new Button(window)
            {
                Text = "Pause",
                MinimumWidth = 150,
            };
            PauseButton.Show();
            StopButton = new Button(window)
            {
                Text = "Stop",
                MinimumWidth = 150,
            };
            StopButton.Show();
            SkipPlusButton = new Button(window)
            {
                Text = "Skip+10",
                MinimumWidth = 150,
            };
            SkipPlusButton.Show();
            SkipMinusButton = new Button(window)
            {
                Text = "Skip-10",
                MinimumWidth = 150,
            };
            SkipMinusButton.Show();
            buttonBox.PackEnd(PlayButton);
            buttonBox.PackEnd(PauseButton);
            buttonBox.PackEnd(StopButton);
            buttonBox.PackEnd(SkipPlusButton);
            buttonBox.PackEnd(SkipMinusButton);


            rootBox.PackEnd(MediaSurface);
            rootBox.PackEnd(titleBox);
            rootBox.PackEnd(artistBox);
            rootBox.PackEnd(albumBox);
            rootBox.PackEnd(stateBox);
            rootBox.PackEnd(progress);
            rootBox.PackEnd(buttonBox);

            //MediaSurface.Resized += (() =>
            //{
            //    MediaSurface.Resize(500, 500);
            //    PlayUrl(null, null);
            //});
        }

        Box CreateHorizontalBoxInLabel(EvasObject parent, string text)
        {
            var box = new Box(parent)
            {
                IsHorizontal = true,
            };
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
