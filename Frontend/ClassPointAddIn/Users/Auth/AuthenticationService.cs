using ClassPointAddIn.Api.Service;
using ClassPointAddIn.API.Service;
using Domain.Users.Entities;
using Domain.Users.ValueObjects;
using System.Threading.Tasks;

namespace ClassPointAddIn.Users.Auth
{
    public class AuthenticationService
    {
        private readonly IUserApiClient _userApiClient;
        private readonly QuickPollApiClient _quickPollApiClient;

        // Store teacher token
        public string Token { get; private set; }

        public AuthenticationService(IUserApiClient userApiClient, QuickPollApiClient quickPollApiClient)
        {
            _userApiClient = userApiClient;
            _quickPollApiClient = quickPollApiClient;
        }

        /// <summary>
        /// Logs in the teacher, stores the access token,
        /// and sets it on the QuickPoll API client for authenticated requests.
        /// </summary>
        public async Task<User> LoginAsync(string username, string password)
        {
            var tokenResponse = await _userApiClient.LoginAsync(username, password);

            // Create domain token
            var token = new JWTToken(tokenResponse.Access, tokenResponse.Refresh);
            Token = token.AccessToken; // save access token locally

            // Create user entity
            var user = new User(username);
            user.AssignToken(token);

            // Apply the Bearer token to QuickPoll API
            _quickPollApiClient.SetBearer(Token);

            // ✅ Store globally so other forms (like StudentVoteForm) can use it
            ThisAddIn.StudentToken = token.AccessToken;

            return user;
        }

    }
}
