using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using bot_fy.Service;

namespace bot_fy.Commands
{
    public class AuthCommand : ApplicationCommandModule
    {
        private readonly YoutubeService youtubeService;

        public AuthCommand(YoutubeService youtubeService)
        {
            this.youtubeService = youtubeService;
        }

        [SlashCommand("auth", "Comandos de autenticação do YouTube")]
        public async Task AuthCommands(InteractionContext ctx,
            [Choice("login", "login")]
            [Choice("status", "status")]
            [Choice("logout", "logout")]
            [Choice("help", "help")]
            [Option("action", "Ação a ser executada")] string action)
        {
            await ctx.DeferAsync();

            switch (action.ToLower())
            {
                case "login":
                    await HandleLoginAsync(ctx);
                    break;
                
                case "status":
                    await HandleStatusAsync(ctx);
                    break;
                
                case "logout":
                    await HandleLogoutAsync(ctx);
                    break;
                
                case "help":
                    await HandleHelpAsync(ctx);
                    break;
                
                default:
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("Ação inválida. Use: login, status, logout ou help"));
                    break;
            }
        }

        private async Task HandleLoginAsync(InteractionContext ctx)
        {
            try
            {
                bool isAuthenticated = await youtubeService.IsAuthenticatedAsync();
                
                if (isAuthenticated)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("? Já está autenticado no YouTube!"));
                    return;
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("?? Verificando autenticação...\n\n" +
                               "Para acessar conteúdo privado do YouTube, você precisa configurar cookies manualmente.\n" +
                               "Use `/auth help` para ver as instruções detalhadas."));

                await youtubeService.InitializeWithAuthenticationAsync();

                bool newAuthStatus = await youtubeService.IsAuthenticatedAsync();
                
                if (newAuthStatus)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("? Autenticação realizada com sucesso! Agora você pode acessar vídeos e playlists privadas."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("? Nenhum cookie válido encontrado. Configure os cookies conforme as instruções em `/auth help`."));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"? Erro durante a autenticação: {ex.Message}"));
            }
        }

        private async Task HandleStatusAsync(InteractionContext ctx)
        {
            try
            {
                bool isAuthenticated = await youtubeService.IsAuthenticatedAsync();
                
                var embed = new DiscordEmbedBuilder()
                {
                    Title = "Status da Autenticação YouTube",
                    Color = isAuthenticated ? DiscordColor.Green : DiscordColor.Red
                };

                if (isAuthenticated)
                {
                    embed.Description = "? **Autenticado**\n" +
                                      "Você pode acessar vídeos e playlists privadas.";
                    embed.AddField("Funcionalidades Disponíveis", 
                                 "• Vídeos públicos\n" +
                                 "• Vídeos privados/não listados\n" +
                                 "• Playlists públicas\n" +
                                 "• Playlists privadas", true);
                }
                else
                {
                    embed.Description = "? **Não Autenticado**\n" +
                                      "Apenas conteúdo público está disponível.";
                    embed.AddField("Funcionalidades Disponíveis", 
                                 "• Vídeos públicos\n" +
                                 "• Playlists públicas", true);
                    embed.AddField("Para Autenticar", 
                                 "Use `/auth help` para ver as instruções", true);
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"? Erro ao verificar status: {ex.Message}"));
            }
        }

        private async Task HandleLogoutAsync(InteractionContext ctx)
        {
            try
            {
                await youtubeService.ClearAuthenticationAsync();
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("? Logout realizado com sucesso! A autenticação foi removida."));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"? Erro durante o logout: {ex.Message}"));
            }
        }

        private async Task HandleHelpAsync(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = "?? Como Configurar Autenticação YouTube",
                Color = DiscordColor.Cyan,
                Description = "Para acessar vídeos e playlists privadas, siga estes passos:"
            };

            embed.AddField("**1. Acesse o YouTube**", 
                         "Vá para https://www.youtube.com e faça login na sua conta", false);

            embed.AddField("**2. Abra as Ferramentas do Desenvolvedor**", 
                         "Pressione `F12` ou clique com o botão direito ? `Inspecionar`", false);

            embed.AddField("**3. Navegue até os Cookies**", 
                         "Vá para `Application` ? `Storage` ? `Cookies` ? `https://www.youtube.com`", false);

            embed.AddField("**4. Copie os Cookies Importantes**", 
                         "Encontre e copie os valores de: `HSID`, `SSID`, `APISID`, `SAPISID`", false);

            embed.AddField("**5. Crie o Arquivo de Cookies**", 
                         "Crie um arquivo `youtube_cookies.json` na pasta do bot com o formato:\n" +
                         "```json\n" +
                         "[\n" +
                         "  {\n" +
                         "    \"Name\": \"HSID\",\n" +
                         "    \"Value\": \"seu_valor_aqui\",\n" +
                         "    \"Domain\": \".youtube.com\",\n" +
                         "    \"Path\": \"/\"\n" +
                         "  },\n" +
                         "  {\n" +
                         "    \"Name\": \"SSID\",\n" +
                         "    \"Value\": \"seu_valor_aqui\",\n" +
                         "    \"Domain\": \".youtube.com\",\n" +
                         "    \"Path\": \"/\"\n" +
                         "  }\n" +
                         "]\n" +
                         "```", false);

            embed.AddField("**6. Teste a Autenticação**", 
                         "Use `/auth login` para verificar se a configuração está correta", false);

            embed.WithFooter("?? Mantenha seus cookies seguros e não os compartilhe!");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed));
        }
    }
}