using NAudio.Wave;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace bot_fy.Service
{
    public class AudioService
    {
        private readonly YoutubeClient youtube = new();

        public async Task<WaveStream> ConvertAudioToPcm(string id)
        {
            string url = await GetAudioUrl(id);

            return WaveFormatConversionStream.CreatePcmStream(new MediaFoundationReader(url));
        }

        private async Task<string> GetAudioUrl(string url)
        {
            try
            {
                StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                return streamInfo.Url;
            }
            catch (VideoUnplayableException)
            {
                return await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(url);
            }
        }
    }
}
