using BotFy.Service;

namespace BotFy.Tests
{
    public class UnitTest1
    {
        private readonly YoutubeService youtubeService = new();

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=6n3pFFPSlW4")]
        public async Task Test1(string url)
        {
            var result = await youtubeService.GetVideoAsync(url);

            Assert.NotNull(result);

        }
    }
}