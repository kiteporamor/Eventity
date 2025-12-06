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
                TestUserLogin = configuration["TestConfiguration:TestUserLogin"] ?? "testuser",
                TestUserPassword = configuration["TestConfiguration:TestUserPassword"] ?? "TestPass123!",
                UseSecretsFromVault = bool.Parse(configuration["TestConfiguration:UseSecretsFromVault"] ?? "false")
            };

            _objectContainer.RegisterInstanceAs(testConfig);
            _objectContainer.RegisterInstanceAs(new TestContext());
        }
    }

    public class TestConfiguration
    {
        public string? ApiBaseUrl { get; set; }
        public string? TestUserLogin { get; set; }
        public string? TestUserPassword { get; set; }
        public bool UseSecretsFromVault { get; set; }
    }

    public class TestContext
    {
        public string? TechnicalUserToken { get; set; }
        public Guid? TechnicalUserId { get; set; }
        public List<string> UsersToCleanup { get; } = new();
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
    }
}