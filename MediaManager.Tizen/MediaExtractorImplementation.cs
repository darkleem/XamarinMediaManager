using System.IO;
using System.Threading.Tasks;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using Tizen.Multimedia;

namespace Plugin.MediaManager
{
    public class MediaExtractorImplementation : IMediaExtractor
    {
        public async Task<IMediaFile> ExtractMediaInfo(IMediaFile mediaFile)
        {
            if (mediaFile.Availability == ResourceAvailability.Remote)
            {
                MetadataExtractor ex = new MetadataExtractor(mediaFile.Url);

                if (ex != null)
                {
                    SetDataInfo(mediaFile, ex.GetMetadata());
                    SetAlbumArt(mediaFile, ex);
                }
            }

            mediaFile.MetadataExtracted = true;

            return await Task.FromResult(mediaFile);
        }

        void SetAlbumArt(IMediaFile mediaFile, MetadataExtractor ex)
        {
            if (mediaFile.Type == MediaFileType.Video)
            {
                var buffer = ex.GetVideoThumbnail();
                if (buffer.Length > 0)
                {
                    Stream st = new MemoryStream(buffer);
                    mediaFile.Metadata.AlbumArt = st;
                }
            }
            else if (mediaFile.Type == MediaFileType.Audio)
            {
                var artWork = ex.GetArtwork();
                if (artWork != null)
                {
                    Stream st = new MemoryStream(artWork.Data);
                    mediaFile.Metadata.AlbumArt = st;
                }
            }
        }

        void SetDataInfo(IMediaFile mediaFile, Metadata metadata)
        {
            mediaFile.Metadata.Title = metadata.Title;
            mediaFile.Metadata.Artist = metadata.Artist;
            mediaFile.Metadata.Album = metadata.Album;
            mediaFile.Metadata.AlbumArtist = metadata.AlbumArtist;
            mediaFile.Metadata.Artist = metadata.Artist;
            mediaFile.Metadata.Author = metadata.Author;
            mediaFile.Metadata.Duration = metadata.Duration ?? 0;
            mediaFile.Metadata.Genre = metadata.Genre;
        }
    }
}