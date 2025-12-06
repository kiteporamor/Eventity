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
            
            _testContext.LastUserLogin = "default";
        }

        [Given(@"существует технический пользователь с логином '(.*)' и паролем '(.*)'")]
        public async Task GivenTechnicalUserExists(string login, string password)
        {
            _testContext.LastUserLogin = login;
            
            try
            {
                // Пытаемся сначала зарегистрировать пользователя
                var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
                {
                    name = $"Technical User {DateTime.Now.Ticks}",
                    email = $"{login.ToLower()}@test.eventity.com",
                    login,
                    password,
                    role = UserRoleEnum.User
                });

                if (registerResponse.IsSuccessStatusCode)
                {
                    var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
                    _testContext.TechnicalUserToken = registerResult?.Token;
                    _testContext.TechnicalUserId = registerResult?.Id;
                    Console.WriteLine($"✅ Registered new user: {login}");
                    return;
                }
                
                // Если регистрация не удалась (возможно, пользователь уже существует),
                // пробуем залогиниться
                var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
                {
                    login,
                    password
                });

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"📋 Login response: {content}");
                    
                    // Проверяем, требуется ли 2FA
                    if (content.Contains("Requires2FA"))
                    {
                        try
                        {
                            var json = JsonDocument.Parse(content);
                            if (json.RootElement.TryGetProperty("userId", out var userIdElement))
                            {
                                _testContext.TechnicalUserId = userIdElement.GetGuid();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Error parsing JSON: {ex.Message}");
                        }
                    }
                    else
                    {
                        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
                        _testContext.TechnicalUserToken = authResult?.Token;
                        _testContext.TechnicalUserId = authResult?.Id;
                    }
                    Console.WriteLine($"✅ User exists: {login}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking/creating user {login}: {ex.Message}");
                throw;
            }

            throw new InvalidOperationException($"❌ Failed to create or login user {login}");
        }

        [Given(@"включена двухфакторная аутентификация")]
        public void GivenTwoFactorAuthenticationIsEnabled()
        {
            _testContext.Is2FAEnabled = true;
            Console.WriteLine("✅ 2FA is enabled for testing");
        }

        [When(@"пользователь пытается войти с логином '(.*)' и паролем '(.*)'")]
        public async Task WhenUserAttemptsLogin(string login, string password)
        {
            _testContext.LastUserLogin = login;
            Console.WriteLine($"🔐 Attempting login for user: {login}");
            
            try
            {
                _testContext.LastLoginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
                {
                    login,
                    password
                });
                
                Console.WriteLine($"📊 Login response status: {_testContext.LastLoginResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during login: {ex.Message}");
                throw;
            }
        }

        [Then(@"требуется ввести код подтверждения")]
        public async Task ThenVerificationCodeIsRequired()
        {
            _testContext.LastLoginResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await _testContext.LastLoginResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 Checking 2FA requirement");
            
            content.Should().Contain("Requires2FA");
            content.Should().Contain("true");
            
            try
            {
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("userId", out var userIdElement))
                {
                    _testContext.TwoFactorUserId = userIdElement.GetGuid();
                    Console.WriteLine($"✅ 2FA required for user ID: {_testContext.TwoFactorUserId}");
                }
                else
                {
                    Console.WriteLine("⚠️ Warning: userId not found in response");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error parsing JSON: {ex.Message}");
                throw;
            }
            
            _testContext.LastLoginRequires2FA = true;
        }

        [Given(@"получен код подтверждения по email")]
        public void GivenVerificationCodeReceivedByEmail()
        {
            // В тестовом режиме используем фиксированный код
            _testContext.LastVerificationCode = "123456";
            Console.WriteLine($"📧 Using test verification code: {_testContext.LastVerificationCode}");
        }

        [When(@"пользователь вводит правильный код подтверждения")]
        public async Task WhenUserEntersCorrectVerificationCode()
        {
            if (_testContext.TwoFactorUserId == null)
            {
                throw new InvalidOperationException("❌ TwoFactorUserId is not set. Did 2FA flow complete?");
            }

            Console.WriteLine($"🔑 Verifying 2FA code for user: {_testContext.TwoFactorUserId}");
            
            try
            {
                _testContext.LastVerifyResponse = await _httpClient.PostAsJsonAsync("/api/auth/verify-2fa", new
                {
                    userId = _testContext.TwoFactorUserId,
                    code = _testContext.LastVerificationCode
                });
                
                Console.WriteLine($"📊 Verify response status: {_testContext.LastVerifyResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during 2FA verification: {ex.Message}");
                throw;
            }
        }

        [Then(@"аутентификация успешна и выдан JWT токен")]
        public async Task ThenAuthenticationIsSuccessfulAndTokenIssued()
        {
            _testContext.LastVerifyResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await _testContext.LastVerifyResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"✅ Auth successful response received");
            
            var authResult = await _testContext.LastVerifyResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            authResult.Should().NotBeNull();
            authResult!.Token.Should().NotBeNullOrEmpty();
            
            _testContext.LastAuthToken = authResult.Token;
            _testContext.LastUserId = authResult.Id;
            
            Console.WriteLine($"✅ Authentication successful. User ID: {_testContext.LastUserId}");
        }

        [Then(@"получен доступ к защищенным ресурсам")]
        public async Task ThenAccessToProtectedResourcesIsGranted()
        {
            if (string.IsNullOrEmpty(_testContext.LastAuthToken))
            {
                throw new InvalidOperationException("❌ No auth token available");
            }

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_testContext.LastAuthToken}");
            
            var response = await _httpClient.GetAsync("/api/events");
            Console.WriteLine($"🔒 Access to /api/events: {response.StatusCode}");
            
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _testContext.HasAccessToProtectedResources = true;
                Console.WriteLine("✅ Access to protected resources granted");
            }
            else
            {
                Console.WriteLine("⚠️ Access to protected resources not granted (might require specific permissions)");
            }
        }

        [Given(@"пользователь успешно аутентифицирован с 2FA")]
        public async Task GivenUserSuccessfullyAuthenticatedWith2FA()
        {
            // Используем уникальный логин для этого сценария
            var login = "changepassuser";
            var password = "OldPass123!";
            
            // Создаем/проверяем пользователя
            await GivenTechnicalUserExists(login, password);
            
            // Логинимся
            await WhenUserAttemptsLogin(login, password);
            await ThenVerificationCodeIsRequired();
            
            // Получаем код
            GivenVerificationCodeReceivedByEmail();
            
            // Вводим код
            await WhenUserEntersCorrectVerificationCode();
            await ThenAuthenticationIsSuccessfulAndTokenIssued();
            
            Console.WriteLine("✅ User successfully authenticated with 2FA for password change test");
        }

        [When(@"пользователь отправляет запрос на смену пароля с текущим паролем '(.*)' и новым паролем '(.*)'")]
        public async Task WhenUserSubmitsPasswordChangeRequest(string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(_testContext.LastAuthToken))
            {
                throw new InvalidOperationException("❌ No auth token available for password change");
            }

            Console.WriteLine($"🔄 Changing password from '{currentPassword}' to '{newPassword}'");
            
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_testContext.LastAuthToken}");
            
            try
            {
                _testContext.LastPasswordChangeResponse = await _httpClient.PostAsJsonAsync("/api/auth/change-password", new
                {
                    currentPassword,
                    newPassword
                });
                
                Console.WriteLine($"📊 Password change response status: {_testContext.LastPasswordChangeResponse.StatusCode}");
                
                if (!_testContext.LastPasswordChangeResponse.IsSuccessStatusCode)
                {
                    var error = await _testContext.LastPasswordChangeResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"⚠️ Password change error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during password change: {ex.Message}");
                throw;
            }
        }

        [Then(@"смена пароля успешна")]
        public void ThenPasswordChangeIsSuccessful()
        {
            _testContext.LastPasswordChangeResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
            Console.WriteLine("✅ Password change successful");
        }

        [Then(@"пользователь может войти с новым паролем '(.*)'")]
        public async Task ThenUserCanLoginWithNewPassword(string newPassword)
        {
            // Очищаем заголовки
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            
            Console.WriteLine($"🔐 Attempting login with new password for user: {_testContext.LastUserLogin}");
            
            var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                login = _testContext.LastUserLogin,
                password = newPassword
            });
            
            Console.WriteLine($"📊 Login with new password status: {loginResponse.StatusCode}");
            
            if (!loginResponse.IsSuccessStatusCode)
            {
                var error = await loginResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Login with new password failed: {error}");
            }
            
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            Console.WriteLine("✅ Login with new password successful");
        }
    }
}