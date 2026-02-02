using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.API.Service
{
    public class QuickPollApiClient
    {
        private readonly HttpClient _client;

        public QuickPollApiClient()
        {
            string baseUrl = Properties.Settings.Default.QuickPollBase;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                MessageBox.Show(
                    "QuickPollBase setting is missing. Please set it to your backend URL in project settings.",
                    "Configuration Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                baseUrl = "http://127.0.0.1:8000/api/quickpolls/"; // fallback
            }

            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }



        /// <summary>
        /// Assigns or clears the Bearer token for authenticated teacher requests.
        /// Call this right after login.
        /// </summary>
        public void SetBearer(string token)
        {
            if (_client.DefaultRequestHeaders.Contains("Authorization"))
                _client.DefaultRequestHeaders.Remove("Authorization");

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }


        /// <summary>
        /// Create a new poll.
        /// For custom polls, include question text and options in payload.
        /// </summary>
        public async Task<string> CreatePollAsync(string questionType, int optionCount, string pollName)

        {
            var payload = new
            {
                name = pollName,                 // ✅ NEW
                question_type = questionType,
                option_count = optionCount,
                is_active = true
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("create/", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }


        /// <summary>
        /// Submit a vote using poll code + option number.
        /// Student must include their own Bearer token in this case.
        /// </summary>
        public async Task<HttpResponseMessage> VoteAsync(string pollCode, int optionId, string studentEmail, string studentName)
        {
            if (string.IsNullOrWhiteSpace(pollCode))
                throw new ArgumentException("Poll code cannot be null or empty.", nameof(pollCode));

            var payload = new
            {
                option_id = optionId,
                student_email = studentEmail ?? "",
                student_name = studentName ?? ""
            };

            string json = JsonConvert.SerializeObject(payload);
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("Vote payload serialization failed.");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = $"{pollCode}/vote/";
            return await _client.PostAsync(url, content);
        }




        /// <summary>
        /// Get poll results (teacher only).
        /// </summary>
        public async Task<string> GetResultsAsync(string pollCode)
        {
            var response = await _client.GetAsync($"{pollCode}/results/");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Close poll (teacher only).
        /// </summary>
        public async Task<string> ClosePollAsync(string pollCode)
        {
            var response = await _client.PostAsync($"{pollCode}/close/", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }



        public async Task<string> GetResultsByNameAsync(string pollName)
        {
            // ✅ Remove any accidental newline or spaces
            pollName = pollName?.Trim();

            // ✅ Because BaseAddress already points to "http://127.0.0.1:8000/api/quickpolls/"
            var response = await _client.GetAsync($"name/{pollName}/");

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }



    }
}
