using NUnit.Framework;
using Newtonsoft.Json;
using NLog;
using NUnit.Allure.Core;
using NUnit.Allure.Attributes;
using Allure.Commons;
using System.Net.Http.Headers;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Filter Users")]
    public class GetAndFilterUsersTests
    {
        private HttpClient _clientForReadScope;
        private string _baseUrlUsers = "/users"; 

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            "GetAndFilterUsersTests".LogInfo("Setting up tests...");

            _clientForReadScope = ClientForReadScope.GetInstance().GetHttpClient();

         }

        [Test]
        [AllureDescription("Test to get all expected users stored in the application for now")]
        public async Task GetAllUsers_ReturnsAllExpectedUsers_Test()
        {
            "GetAllUsers_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                /*StepResult step1 = new StepResult { name = "Step#1: Get all users stored currently" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);*/
                _clientForReadScope.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string responseContent = await response.Content.ReadAsStringAsync();
                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(responseContent);
                //AllureLifecycle.Instance.StopStep();

                /*StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all recieved users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);*/
                var expectedUsers = new List<User>
                {
                    new User { Name = "Emma Jones", Age = 6, Sex = "MALE", ZipCode = "12345" },
                    new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
                    new User { Name = "James Brown", Age = 58, Sex = "MALE", ZipCode = "ABCDE" },
                    new User { Name = "David Smith", Age = 24, Sex = "MALE", ZipCode = null },
                    new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
                };

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");

                });

                "GetAllUsers_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                //AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetAllUsers_ReturnsAllExpectedUsers_Test".LogError($"An error occured: {ex.Message}");
             }
        }

        [Test]
        [AllureDescription("Test to get all users older than set parameter")]
        public async Task GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users older than set parameter" };
                AllureLifecycle.Instance.StartStep("Step#1: Get all users older than set parameter", step1);
                int olderThan = 60;

                HttpResponseMessage response = await _clientForReadScope.GetAsync($"{_baseUrlUsers}?olderThan={olderThan}");

                string responseContent = await response.Content.ReadAsStringAsync();
                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(responseContent);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all received filtered (older than) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep("Step#2: Verify Status Code of the GET response and all received filtered (older than) users correspond to all expected to receive", step2);
                var expectedUsers = new List<User>
                {
                    new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
                };

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });

                "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test".LogError($"Error occurred: {ex.Message}");
            }
        }


        [Test]
        [AllureDescription("Test to get all users younger than set parameter")]
        public async Task GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users younger than set parameter" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                int youngerThan = 1;

                HttpResponseMessage response = await _clientForReadScope.GetAsync($"{_baseUrlUsers}?youngerThan={youngerThan}");

                string responseContent = await response.Content.ReadAsStringAsync();
                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(responseContent);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all received filtered (younger than) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User> { };

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });

                "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test".LogError($"Error occurred: {ex.Message}");
            }
        }

        [Test]
        [AllureDescription("Test to get all users with certain sex as set parameter")]
        public async Task GetFilteredUsersSex_ReturnsAllExpectedUsers_Test()
        {
            "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogInfo("Starting the test...");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users with certain sex as set parameter" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                string sex = "FEMALE";

                HttpResponseMessage response = await _clientForReadScope.GetAsync($"{_baseUrlUsers}?sex={sex}");
  
                string responseContent = await response.Content.ReadAsStringAsync();
                List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(responseContent);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the GET response and all received filtered (by sex) users correspond to all expected to receive" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var expectedUsers = new List<User>
                {
                    new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
                };

                Assert.Multiple(() =>
                {
                    Assert.That((int)response.StatusCode, Is.EqualTo(200));
                    Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
                });

                "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogInfo("The test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                "GetFilteredUsersSex_ReturnsAllExpectedUsers_Test".LogError($"Error occurred: {ex.Message}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            "GetAndFilterUsersTests".LogInfo("Tearing down tests...");
            LogManager.Shutdown();

        }
    }  
}
