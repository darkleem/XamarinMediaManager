using System;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;

namespace Plugin.MediaManager
{
    public class MediaNotificationManagerImplementation : IMediaNotificationManager
    {
        public void StartNotification(IMediaFile mediaFile)
        {
            throw new NotSupportedException("Currently, the Tizen C# API does not support the Minicontrol(Notification Extension) feature.");
        }

        public void StopNotifications()
        {
            throw new NotSupportedException("Currently, the Tizen C# API does not support the Minicontrol(Notification Extension) feature.");
        }

        public void UpdateNotifications(IMediaFile mediaFile, MediaPlayerStatus status)
        {
            throw new NotSupportedException("Currently, the Tizen C# API does not support the Minicontrol(Notification Extension) feature.");
        }
    }
}