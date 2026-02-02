using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPointAddIn.Api.Service
{
    public class StudentService
    {
        private readonly HttpClient _client;
        public string _token;

        public StudentService(string token)
            
        {
            _token = token;
            _client = new HttpClient();
            _client.BaseAddress = new System.Uri("http://127.0.0.1:8000/");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // ✅ Get all students
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var response = await _client.GetAsync("api/students/students/");
            if (!response.IsSuccessStatusCode)
                return new List<Student>();

            var json = await response.Content.ReadAsStringAsync();
            System.IO.File.WriteAllText(@"C:\temp\jwt.txt", _token);

            


            // ✅ Expect a pure array now, not { "students": [...] }
            return JsonConvert.DeserializeObject<List<Student>>(json);
        }



        // ✅ Add student (fixed _http -> _client)
        public async Task<string> AddStudentAsync(string fullName, string email)
        {
            var payload = new { full_name = fullName, email = email };
            var json = JsonConvert.SerializeObject(payload);
            var resp = await _client.PostAsync(
                "api/students/students/",
                new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            );

            // Read full response text
            var responseBody = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
                return "OK"; // success
            else
                return $"ERROR: {resp.StatusCode}\n{responseBody}";
        }

    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; } // optional
    }
}
