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

        [SlashCommand("auth", "Comandos de autentica��o do YouTube")]
        public async Task AuthCommands(InteractionContext ctx,
            [Choice("login", "login")]
            [Choice("status", "status")]
            [Choice("logout", "logout")]
            [Choice("help", "help")]
            [Option("action", "A��o a ser executada")] string action)
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
                        .WithContent("A��o inv�lida. Use: login, status, logout ou help"));
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
                        .WithContent("? J� est� autenticado no YouTube!"));
                    return;
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("?? Verificando autentica��o...\n\n" +
                               "Para acessar conte�do privado do YouTube, voc� precisa configurar cookies manualmente.\n" +
                               "Use `/auth help` para ver as instru��es detalhadas."));

                await youtubeService.InitializeWithAuthenticationAsync();

                bool newAuthStatus = await youtubeService.IsAuthenticatedAsync();
                
                if (newAuthStatus)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("? Autentica��o realizada com sucesso! Agora voc� pode acessar v�deos e playlists privadas."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("? Nenhum cookie v�lido encontrado. Configure os cookies conforme as instru��es em `/auth help`."));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"? Erro durante a autentica��o: {ex.Message}"));
            }
        }

        private async Task HandleStatusAsync(InteractionContext ctx)
        {
            try
            {
                bool isAuthenticated = await youtubeService.IsAuthenticatedAsync();
                
                var embed = new DiscordEmbedBuilder()
                {
                    Title = "Status da Autentica��o YouTube",
                    Color = isAuthenticated ? DiscordColor.Green : DiscordColor.Red
                };

                if (isAuthenticated)
                {
                    embed.Description = "? **Autenticado**\n" +
                                      "Voc� pode acessar v�deos e playlists privadas.";
                    embed.AddField("Funcionalidades Dispon�veis", 
                                 "� V�deos p�blicos\n" +
                                 "� V�deos privados/n�o listados\n" +
                                 "� Playlists p�blicas\n" +
                                 "� Playlists privadas", true);
                }
                else
                {
                    embed.Description = "? **N�o Autenticado**\n" +
                                      "Apenas conte�do p�blico est� dispon�vel.";
                    embed.AddField("Funcionalidades Dispon�veis", 
                                 "� V�deos p�blicos\n" +
                                 "� Playlists p�blicas", true);
                    embed.AddField("Para Autenticar", 
                                 "Use `/auth help` para ver as instru��es", true);
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
                    .WithContent("? Logout realizado com sucesso! A autentica��o foi removida."));
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
                Title = "?? Como Configurar Autentica��o YouTube",
                Color = DiscordColor.Cyan,
                Description = "Para acessar v�deos e playlists privadas, siga estes passos:"
            };

            embed.AddField("**1. Acesse o YouTube**", 
                         "V� para https://www.youtube.com e fa�a login na sua conta", false);

            embed.AddField("**2. Abra as Ferramentas do Desenvolvedor**", 
                         "Pressione `F12` ou clique com o bot�o direito ? `Inspecionar`", false);

            embed.AddField("**3. Navegue at� os Cookies**", 
                         "V� para `Application` ? `Storage` ? `Cookies` ? `https://www.youtube.com`", false);

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

            embed.AddField("**6. Teste a Autentica��o**", 
                         "Use `/auth login` para verificar se a configura��o est� correta", false);

            embed.WithFooter("?? Mantenha seus cookies seguros e n�o os compartilhe!");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed));
        }
    }
}