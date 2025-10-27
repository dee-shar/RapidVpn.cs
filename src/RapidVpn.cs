using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace RapidVpnApi
{
    public class RapidVpn
    {
        private string token;
        private readonly HttpClient httpClient;
        private static readonly Random random = new();
        private const string decryptionKey = "fuck_snsslmm_bslznw";
        private static readonly string characters = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly string apiUrl = "https://api.vpnrapid.net";
        private readonly string deviceUser = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                chid = random.Next(1000000, 2000000).ToString(),
                subchid = "1",
                vc = "144",
                packageName = "com.rapidconn.android",
                ab_info = new[] { "HJ3", "HI2", "HH1", "HF2", "GA5" },
                pkg = "com.rapidconn.android",
                vn = "2.3.5",
                os_type = "Android"
            })));

        public RapidVpn()
        {
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("okhttp/4.12.0");
            httpClient.DefaultRequestHeaders.Add("device-user", deviceUser);
            httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
        }

        private string DecryptToken(string encryptedBase64Token, string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] decryptedBytes = Convert.FromBase64String(encryptedBase64Token);
                foreach (byte keyByte in keyBytes)
                {
                    for (int i = 0; i < decryptedBytes.Length; i++)
                    {
                        decryptedBytes[i] = (byte)(decryptedBytes[i] ^ keyByte);
                    }
                }
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
        }

        public static string GenerateData()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(18)).Replace("\n", "").Replace("\r", "").Substring(0, 24) + "==";
        }

        public async Task<string> Register()
        {
            var data = new StringContent(GenerateData(), Encoding.UTF8, "text/plain");
            var response = await httpClient.PostAsync($"{apiUrl}/register", data);
            var content = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("token", out var tokenElement))
            {
                string encodedToken = tokenElement.GetString();
                string decryptedVToken = DecryptToken(encodedToken, decryptionKey);
                if (string.IsNullOrEmpty(decryptedVToken))
                {
                    Console.WriteLine("Error: Decrypted token was null or empty. Cannot set v-token header.");
                    return content;
                }
                token = decryptedVToken;
                if (!httpClient.DefaultRequestHeaders.Contains("v-token"))
                {
                    httpClient.DefaultRequestHeaders.Add("v-token", token);
                }
            }
            return content;
        }

        public async Task<string> GetServers()
        {
            var data = new StringContent("page=0&pagesize=999&tab=0&new_user=1&bt=1&node_type=0", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await httpClient.PostAsync($"{apiUrl}/v2/accnode", data);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
