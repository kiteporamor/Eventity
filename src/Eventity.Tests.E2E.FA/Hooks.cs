using BoDi;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace Eventity.Tests.E2E.FA
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario]
        public void RegisterDependencies()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            var testConfig = new TestConfiguration
            {
                ApiBaseUrl = configuration["TestConfiguration:ApiBaseUrl"] 
                             ?? Environment.GetEnvironmentVariable("EVENTITY_API_URL") 
                             ?? "http://eventity-app:5001",
                TestUserLogin = configuration["TestConfiguration:TestUserLogin"] ?? "techuser",
                TestUserPassword = configuration["TestConfiguration:TestUserPassword"],
                TestUserNewPassword = configuration["TestConfiguration:TestUserNewPassword"],
                TwoFactorTestCode = configuration["TestConfiguration:TwoFactorTestCode"]
                                   ?? Environment.GetEnvironmentVariable("TEST_2FA_CODE")
                                   ?? "123456",
                TwoFactorMaxAttempts = int.Parse(configuration["TestConfiguration:TwoFactorMaxAttempts"] ?? "3"),
                TwoFactorLockoutSeconds = int.Parse(configuration["TestConfiguration:TwoFactorLockoutSeconds"] ?? "2")
            };

            if (string.IsNullOrWhiteSpace(testConfig.TestUserPassword))
            {
                throw new InvalidOperationException(
                    "Test user password is required. Configure TestConfiguration:TestUserPassword via secrets.");
            }

            _objectContainer.RegisterInstanceAs(testConfig);
            _objectContainer.RegisterInstanceAs(new TestContext());
        }

        [AfterScenario]
        public async Task CleanupUsers()
        {
            if (!_objectContainer.IsRegistered<TestContext>() || !_objectContainer.IsRegistered<TestConfiguration>())
            {
                return;
            }

            var context = _objectContainer.Resolve<TestContext>();
            var configuration = _objectContainer.Resolve<TestConfiguration>();

            if (context.UsersToCleanup.Count == 0 || string.IsNullOrWhiteSpace(context.CleanupToken))
            {
                return;
            }

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.ApiBaseUrl ?? "http://eventity-app:5001"),
                Timeout = TimeSpan.FromSeconds(30)
            };

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {context.CleanupToken}");

            foreach (var userId in context.UsersToCleanup)
            {
                try
                {
                    await httpClient.DeleteAsync($"/api/users/{userId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cleanup failed for user {userId}: {ex.Message}");
                }
            }
        }
    }

    public class TestConfiguration
    {
        public string? ApiBaseUrl { get; set; }
        public string? TestUserLogin { get; set; }
        public string? TestUserPassword { get; set; }
        public string? TestUserNewPassword { get; set; }
        public string? TwoFactorTestCode { get; set; }
        public int TwoFactorMaxAttempts { get; set; }
        public int TwoFactorLockoutSeconds { get; set; }
    }

    public class TestContext
    {
        public string? TechnicalUserToken { get; set; }
        public Guid? TechnicalUserId { get; set; }
        public List<Guid> UsersToCleanup { get; } = new();
        public string? CleanupToken { get; set; }
        public bool Is2FAEnabled { get; set; }
        public HttpResponseMessage? LastLoginResponse { get; set; }
        public bool LastLoginRequires2FA { get; set; }
        public Guid? TwoFactorUserId { get; set; }
        public string? LastVerificationCode { get; set; }
        public HttpResponseMessage? LastVerifyResponse { get; set; }
        public string? LastAuthToken { get; set; }
        public Guid? LastUserId { get; set; }
        public bool HasAccessToProtectedResources { get; set; }
        public bool PasswordChangeRequested { get; set; }
        public HttpResponseMessage? LastPasswordChangeResponse { get; set; }
        public string? LastUserLogin { get; set; } = "default";
        public string? TechnicalUserLogin { get; set; }
        public string? TechnicalUserPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
