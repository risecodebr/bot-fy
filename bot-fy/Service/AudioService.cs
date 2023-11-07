using bot_fy.Extensions;
using CliWrap;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace bot_fy.Service
{
    public class AudioService(YoutubeClient youtube)
    {
        public async Task ConvertToPcmStreamAsync(IVideo video, VoiceTransmitSink destination, CancellationToken cancellationToken)
        {
            PipeTarget toTransmit = CreatePipeTargetToVoiceTransmit(destination);

            string input = await GetInputAudio(video, cancellationToken);

            Command command = await GetCommand(video, input, toTransmit, cancellationToken);

            await command.ExecuteAsync(cancellationToken);
            return;
        }

        private async Task<Command> GetCommand(IVideo video, string input, PipeTarget pipe, CancellationToken cancellationToken)
        {
            NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream("ffmpeg", PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            Command command = Cli.Wrap("ffmpeg")
                .WithArguments(arguments =>
                {
                    arguments.Add("-re");
                    arguments.Add("-i").Add(input);
                    arguments.Add("-vn");
                    arguments.Add("-ac").Add("2");
                    arguments.Add("-f").Add("s16le");
                    arguments.Add("-ar").Add("48000");
                    arguments.Add("pipe:1");
                })
                .WithStandardOutputPipe(pipe)
                .WithStandardErrorPipe(PipeTarget.ToDelegate(Console.WriteLine))
                .WithValidation(CommandResultValidation.None);

            if (!video.IsLive())
            {
                Stream stream = await GetAudioStreamAsync(video, cancellationToken);
                command.WithStandardInputPipe(PipeSource.FromStream(stream));
            }
            return command;
        }

        private async Task<string> GetInputAudio(IVideo video, CancellationToken cancellationToken)
        {
            if (video.IsLive())
            {
                return await GetAudioUrlLiveStreamAsync(video, cancellationToken);
            }
            
            return "-";
        }

        private async Task<Stream> GetAudioStreamAsync(IVideo video, CancellationToken cancellationToken)
        {
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id, cancellationToken);
            var StreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
            return await youtube.Videos.Streams.GetAsync(StreamInfo, cancellationToken);
        }

        private async Task<string> GetAudioUrlLiveStreamAsync(IVideo video, CancellationToken cancellationToken)
        {
            return await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(video.Id, cancellationToken);
        }

        private PipeTarget CreatePipeTargetToVoiceTransmit(VoiceTransmitSink destination)
        {
            return PipeTarget.Create(async (origin, cancellationToken) =>
            {
                await origin.CopyToAsync(destination, null, cancellationToken);
            });
        }
    }
}
