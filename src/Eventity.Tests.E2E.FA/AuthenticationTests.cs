using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Eventity.Domain.Enums;
using Eventity.Web.Dtos;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Eventity.Tests.E2E.FA
{
    [Binding]
    public class AuthenticationSteps
    {
        private readonly HttpClient _httpClient;
        private readonly TestContext _testContext;
        private readonly TestConfiguration _configuration;

        public AuthenticationSteps(TestContext testContext, TestConfiguration configuration)
        {
            _testContext = testContext;
            _configuration = configuration;

            var baseUrl = Environment.GetEnvironmentVariable("EVENTITY_API_URL")
                ?? configuration.ApiBaseUrl
                ?? "http://eventity-app:5001";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        [Given(@"a technical user exists")]
        public async Task GivenATechnicalUserExists()
        {
            if (string.IsNullOrWhiteSpace(_configuration.TestUserPassword))
            {
                throw new InvalidOperationException("Test user password is not configured.");
            }

            var loginBase = _configuration.TestUserLogin ?? "techuser";
            var login = $"{loginBase}-{DateTime.UtcNow.Ticks}";
            var password = _configuration.TestUserPassword;

            _testContext.TechnicalUserLogin = login;
            _testContext.TechnicalUserPassword = password;

            var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
            {
                name = $"Technical User {DateTime.UtcNow.Ticks}",
                email = $"{login}@test.eventity.local",
                login,
                password,
                role = UserRoleEnum.Admin
            });

            if (registerResponse.IsSuccessStatusCode)
            {
                var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
                _testContext.TechnicalUserId = registerResult?.Id;
                _testContext.CleanupToken = registerResult?.Token;
                if (registerResult?.Id != null)
                {
                    _testContext.UsersToCleanup.Add(registerResult.Id);
                }
                return;
            }

            if (registerResponse.StatusCode != HttpStatusCode.Conflict)
            {
                var error = await registerResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create technical user: {error}");
            }

            var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                login,
                password
            });

            if (!loginResponse.IsSuccessStatusCode)
            {
                var error = await loginResponse.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to authenticate technical user: {error}");
            }

            var content = await loginResponse.Content.ReadAsStringAsync();
            if (content.Contains("requires2FA", StringComparison.OrdinalIgnoreCase))
            {
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("userId", out var userIdElement))
                {
                    _testContext.TechnicalUserId = userIdElement.GetGuid();
                }
            }
            else
            {
                var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
                _testContext.TechnicalUserId = authResult?.Id;
                _testContext.CleanupToken = authResult?.Token;
            }
        }

        [Given(@"two-factor authentication is enabled")]
        public void GivenTwoFactorAuthenticationIsEnabled()
        {
            _testContext.Is2FAEnabled = true;
        }

        [When(@"the user attempts to log in")]
        public async Task WhenTheUserAttemptsToLogIn()
        {
            if (string.IsNullOrWhiteSpace(_testContext.TechnicalUserLogin) ||
                string.IsNullOrWhiteSpace(_testContext.TechnicalUserPassword))
            {
                throw new InvalidOperationException("Technical user credentials are missing.");
            }

            _testContext.LastLoginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                login = _testContext.TechnicalUserLogin,
                password = _testContext.TechnicalUserPassword
            });
        }

        [When(@"the user attempts to log in with the new password")]
        public async Task WhenTheUserAttemptsToLogInWithTheNewPassword()
        {
            if (string.IsNullOrWhiteSpace(_testContext.TechnicalUserLogin) ||
                string.IsNullOrWhiteSpace(_testContext.NewPassword))
            {
                throw new InvalidOperationException("New password is not available for login.");
            }

            _testContext.LastLoginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                login = _testContext.TechnicalUserLogin,
                password = _testContext.NewPassword
            });
        }

        [Then(@"a verification code is required")]
        public async Task ThenAVerificationCodeIsRequired()
        {
            _testContext.LastLoginResponse.Should().NotBeNull();
            _testContext.LastLoginResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await _testContext.LastLoginResponse.Content.ReadAsStringAsync();
            content.Should().Contain("requires2FA");

            var json = JsonDocument.Parse(content);
            if (json.RootElement.TryGetProperty("userId", out var userIdElement))
            {
                _testContext.TwoFactorUserId = userIdElement.GetGuid();
            }
            else
            {
                throw new InvalidOperationException("2FA response did not include userId.");
            }
        }

        [Then(@"a verification code is delivered via email")]
        public void ThenAVerificationCodeIsDeliveredViaEmail()
        {
            _testContext.LastVerificationCode = _configuration.TwoFactorTestCode;
        }

        [When(@"the user verifies the code")]
        public async Task WhenTheUserVerifiesTheCode()
        {
            if (_testContext.TwoFactorUserId == null)
            {
                throw new InvalidOperationException("TwoFactorUserId is not set.");
            }

            var code = _configuration.TwoFactorTestCode;
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("2FA test code is not configured.");
            }

            _testContext.LastVerifyResponse = await _httpClient.PostAsJsonAsync("/api/auth/verify-2fa", new
            {
                userId = _testContext.TwoFactorUserId,
                code
            });
        }

        [Then(@"authentication succeeds and a JWT token is issued")]
        public async Task ThenAuthenticationSucceedsAndAJwtTokenIsIssued()
        {
            _testContext.LastVerifyResponse.Should().NotBeNull();
            _testContext.LastVerifyResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

            var authResult = await _testContext.LastVerifyResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            authResult.Should().NotBeNull();
            authResult!.Token.Should().NotBeNullOrEmpty();

            _testContext.LastAuthToken = authResult.Token;
            _testContext.LastUserId = authResult.Id;
        }

        [Then(@"access to protected resources is granted")]
        public async Task ThenAccessToProtectedResourcesIsGranted()
        {
            if (string.IsNullOrWhiteSpace(_testContext.LastAuthToken))
            {
                throw new InvalidOperationException("No auth token available.");
            }

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_testContext.LastAuthToken}");

            var response = await _httpClient.GetAsync("/api/users/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [When(@"the user enters an invalid verification code for the maximum number of attempts")]
        public async Task WhenTheUserEntersAnInvalidVerificationCodeForTheMaximumNumberOfAttempts()
        {
            if (_testContext.TwoFactorUserId == null)
            {
                throw new InvalidOperationException("TwoFactorUserId is not set.");
            }

            var invalidCode = GetInvalidCode();
            for (var attempt = 0; attempt < _configuration.TwoFactorMaxAttempts; attempt++)
            {
                _testContext.LastVerifyResponse = await _httpClient.PostAsJsonAsync("/api/auth/verify-2fa", new
                {
                    userId = _testContext.TwoFactorUserId,
                    code = invalidCode
                });
            }
        }

        [Then(@"the user is locked out from 2FA verification")]
        public async Task ThenTheUserIsLockedOutFrom2FAVerification()
        {
            _testContext.LastVerifyResponse.Should().NotBeNull();
            _testContext.LastVerifyResponse!.StatusCode.Should().Be(HttpStatusCode.Locked);

            var content = await _testContext.LastVerifyResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Too many verification attempts");
        }

        [When(@"the lockout period has elapsed")]
        public async Task WhenTheLockoutPeriodHasElapsed()
        {
            var delay = TimeSpan.FromSeconds(_configuration.TwoFactorLockoutSeconds + 1);
            await Task.Delay(delay);
        }

        [When(@"the user changes the password to a new value")]
        public async Task WhenTheUserChangesThePasswordToANewValue()
        {
            if (string.IsNullOrWhiteSpace(_testContext.LastAuthToken))
            {
                throw new InvalidOperationException("No auth token available for password change.");
            }

            var newPassword = _configuration.TestUserNewPassword;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                newPassword = $"NewPass-{Guid.NewGuid():N}!";
            }

            _testContext.NewPassword = newPassword;

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_testContext.LastAuthToken}");

            _testContext.LastPasswordChangeResponse = await _httpClient.PostAsJsonAsync("/api/auth/change-password", new
            {
                currentPassword = _testContext.TechnicalUserPassword,
                newPassword
            });
        }

        [Then(@"the password change is successful")]
        public void ThenThePasswordChangeIsSuccessful()
        {
            _testContext.LastPasswordChangeResponse.Should().NotBeNull();
            _testContext.LastPasswordChangeResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private string GetInvalidCode()
        {
            var correctCode = _configuration.TwoFactorTestCode;
            if (!string.Equals(correctCode, "000000", StringComparison.Ordinal))
            {
                return "000000";
            }

            return "111111";
        }
    }
}
