using bot_fy.Service;
using Microsoft.Extensions.DependencyInjection;
using YoutubeExplode;

namespace bot_fy.Extensions.Discord
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<YoutubeService>();
            services.AddScoped<YoutubeClient>();
            services.AddScoped<AudioService>();
            services.AddScoped<AudioService>();
            services.AddScoped<HttpClient>();

            return services;
        }
    }
}
