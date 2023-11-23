using YoutubeExplode.Videos;

namespace bot_fy.Extensions;

public static class YoutubeExplodeExtension
{
    public static bool IsLive(this IVideo video)
    {
        return !video.Duration.HasValue || video.Duration == TimeSpan.Zero;
    }
}
