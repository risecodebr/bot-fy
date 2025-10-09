using System.Net;
using System.Text.Json;
using System.Diagnostics;

namespace bot_fy.Service
{
    public class YoutubeAuthService
    {
        private const string COOKIES_FILE_PATH = "youtube_cookies.json";
        
        private List<Cookie> _cachedCookies = new();

        /// <summary>
        /// Obtém os cookies do YouTube. Se não existirem cookies válidos em cache, solicita autenticação manual.
        /// </summary>
        /// <returns>Lista de cookies do YouTube</returns>
        public async Task<IReadOnlyList<Cookie>> GetYoutubeCookiesAsync()
        {
            // Primeiro tenta carregar cookies do arquivo
            var cookies = await LoadCookiesFromFileAsync();
            if (cookies.Any() && AreCookiesValid(cookies))
            {
                _cachedCookies = cookies.ToList();
                return _cachedCookies;
            }

            // Se não há cookies válidos, solicita autenticação manual
            Console.WriteLine("=== AUTENTICAÇÃO YOUTUBE NECESSÁRIA ===");
            Console.WriteLine("Para acessar vídeos e playlists privadas, você precisa fornecer cookies do YouTube.");
            Console.WriteLine("1. Abra seu navegador e vá para https://www.youtube.com");
            Console.WriteLine("2. Faça login na sua conta");
            Console.WriteLine("3. Abra as ferramentas do desenvolvedor (F12)");
            Console.WriteLine("4. Vá para a aba 'Application' ou 'Storage'");
            Console.WriteLine("5. Encontre 'Cookies' -> 'https://www.youtube.com'");
            Console.WriteLine("6. Copie os valores dos cookies importantes (HSID, SSID, APISID, SAPISID)");
            Console.WriteLine("7. Cole os cookies no arquivo 'youtube_cookies.json' conforme o exemplo:");
            
            var exampleCookies = new List<SerializableCookie>
            {
                new() { Name = "HSID", Value = "SEU_VALOR_AQUI", Domain = ".youtube.com", Path = "/" },
                new() { Name = "SSID", Value = "SEU_VALOR_AQUI", Domain = ".youtube.com", Path = "/" },
                new() { Name = "APISID", Value = "SEU_VALOR_AQUI", Domain = ".youtube.com", Path = "/" },
                new() { Name = "SAPISID", Value = "SEU_VALOR_AQUI", Domain = ".youtube.com", Path = "/" }
            };

            var exampleJson = JsonSerializer.Serialize(exampleCookies, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(exampleJson);
            Console.WriteLine("Após salvar o arquivo, use o comando de autenticação novamente.");

            return new List<Cookie>();
        }

        /// <summary>
        /// Força uma nova autenticação, ignorando cookies em cache
        /// </summary>
        /// <returns>Lista de cookies do YouTube</returns>
        public async Task<IReadOnlyList<Cookie>> ReauthenticateAsync()
        {
            Console.WriteLine("=== REAUTENTICAÇÃO SOLICITADA ===");
            Console.WriteLine("Atualize o arquivo 'youtube_cookies.json' com novos cookies e tente novamente.");
            
            // Limpa cache e tenta carregar novamente
            _cachedCookies.Clear();
            return await GetYoutubeCookiesAsync();
        }

        /// <summary>
        /// Verifica se há cookies válidos em cache
        /// </summary>
        /// <returns>True se há cookies válidos</returns>
        public async Task<bool> HasValidCookiesAsync()
        {
            if (_cachedCookies.Any() && AreCookiesValid(_cachedCookies))
                return true;

            var cookies = await LoadCookiesFromFileAsync();
            return cookies.Any() && AreCookiesValid(cookies);
        }

        /// <summary>
        /// Limpa os cookies em cache e do arquivo
        /// </summary>
        public async Task ClearCookiesAsync()
        {
            _cachedCookies.Clear();
            
            if (File.Exists(COOKIES_FILE_PATH))
            {
                File.Delete(COOKIES_FILE_PATH);
            }
            
            Console.WriteLine("Cookies removidos.");
        }

        /// <summary>
        /// Abre o navegador para facilitar a obtenção de cookies
        /// </summary>
        public void OpenBrowserForAuthentication()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://www.youtube.com",
                    UseShellExecute = true
                };
                Process.Start(psi);
                Console.WriteLine("Navegador aberto para https://www.youtube.com");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Não foi possível abrir o navegador: {ex.Message}");
                Console.WriteLine("Acesse manualmente: https://www.youtube.com");
            }
        }

        private async Task<IReadOnlyList<Cookie>> LoadCookiesFromFileAsync()
        {
            if (!File.Exists(COOKIES_FILE_PATH))
                return new List<Cookie>();

            try
            {
                var json = await File.ReadAllTextAsync(COOKIES_FILE_PATH);
                var cookieData = JsonSerializer.Deserialize<List<SerializableCookie>>(json);
                
                if (cookieData == null)
                    return new List<Cookie>();

                return cookieData.Select(c => new Cookie(c.Name, c.Value, c.Path, c.Domain)
                {
                    Secure = c.Secure,
                    HttpOnly = c.HttpOnly,
                    Expires = c.Expires ?? DateTime.MinValue
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar cookies: {ex.Message}");
                return new List<Cookie>();
            }
        }

        private async Task SaveCookiesToFileAsync(IEnumerable<Cookie> cookies)
        {
            try
            {
                var cookieData = cookies.Select(c => new SerializableCookie
                {
                    Name = c.Name,
                    Value = c.Value,
                    Path = c.Path,
                    Domain = c.Domain,
                    Secure = c.Secure,
                    HttpOnly = c.HttpOnly,
                    Expires = c.Expires == DateTime.MinValue ? null : c.Expires
                }).ToList();

                var json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(COOKIES_FILE_PATH, json);
                Console.WriteLine($"Cookies salvos em {COOKIES_FILE_PATH}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar cookies: {ex.Message}");
            }
        }

        private static bool AreCookiesValid(IEnumerable<Cookie> cookies)
        {
            if (!cookies.Any())
                return false;

            // Verifica se há cookies importantes do YouTube que não expiraram
            var now = DateTime.Now;
            var importantCookies = new[] { "HSID", "SSID", "APISID", "SAPISID", "LOGIN_INFO" };
            
            return cookies.Any(c => 
                importantCookies.Contains(c.Name) && 
                (c.Expires == DateTime.MinValue || c.Expires > now) &&
                !string.IsNullOrEmpty(c.Value));
        }

        private class SerializableCookie
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string Domain { get; set; } = string.Empty;
            public bool Secure { get; set; }
            public bool HttpOnly { get; set; }
            public DateTime? Expires { get; set; }
        }
    }
}