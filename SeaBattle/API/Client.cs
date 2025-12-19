using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeaBattle.API
{
    public class Client
    {
        private readonly HttpClient httpClient;

        private static Client instance = new();
        public static Client Instance => instance;

        private Client()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(@"https://localhost:7138/api/")
            };
        }

        public async Task<(bool Success, string Response)> PostAsync(string path, object content = null)
        {
            try
            {
                HttpResponseMessage response;

                if (content != null)
                {
                    var json = JsonSerializer.Serialize(content);
                    response = await httpClient.PostAsync(
                        path,
                        new StringContent(json, Encoding.UTF8, "application/json"));
                }
                else
                {
                    response = await httpClient.PostAsync(path, null);
                }

                var responseText = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, responseText);
            }
            catch (Exception)
            {
                return (false, "Ошибка соединения");
            }
        }

        public void SetToken(string token)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }
}