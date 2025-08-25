using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Revit.Automation.Services
{
    public class JiraXrayService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://xray.cloud.getxray.app/api/v2";
        private string? _bearerToken;
        private DateTime? _tokenExpiryTime;

        public JiraXrayService(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetTokenAsync()
        {
            // Kiểm tra token còn hạn không
            if (!string.IsNullOrEmpty(_bearerToken) && DateTime.Now < _tokenExpiryTime)
            {
                return _bearerToken!;
            }

            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    client_id = _clientId,
                    client_secret = _clientSecret
                });

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/authenticate",
                    new StringContent(body, Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();
                string token = await response.Content.ReadAsStringAsync();
                _bearerToken = token.Trim('"'); // Loại bỏ dấu ngoặc kép từ JSON response
                
                // Token có thời hạn 1 giờ, set expiry time sớm hơn 5 phút để đảm bảo an toàn
                _tokenExpiryTime = DateTime.Now.AddHours(1).AddMinutes(-5);
                
                TestContext.Progress.WriteLine($"✅ Xác thực Xray thành công, token hết hạn lúc: {_tokenExpiryTime:HH:mm:ss}");
                return _bearerToken;
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"❌ Lỗi xác thực Xray: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PushResultAsync(string executionKey, string testKey, string status, string token)
        {
            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    testExecutionKey = executionKey,
                    tests = new[]
                    {
                        new
                        {
                            testKey = testKey,
                            status = status,
                            start = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                            finish = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+07:00"),
                            comment = "Automated test execution from Revit UI Tests"
                        }
                    }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/import/execution");
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TestContext.Progress.WriteLine($"✅ Đẩy kết quả test lên Jira thành công: {responseContent}");
                    return true;
                }
                else
                {
                    TestContext.Progress.WriteLine($"❌ Lỗi khi đẩy kết quả test: {response.StatusCode} - {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                TestContext.Progress.WriteLine($"❌ Lỗi khi đẩy kết quả test: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
