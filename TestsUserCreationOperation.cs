using NUnit.Framework;
using Newtonsoft.Json;
using Allure.Commons;
using NLog;
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Create Users")]
    public class UserCreationTests
    {
        private HttpClient _clientForReadScope;
        private HttpClient _clientForWriteScope;
        private readonly string _baseUrlUsers = "/users";
        private readonly string _baseUrlZipCodes = "/zip-codes";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            _clientForReadScope = ClientForReadScope.GetInstance().GetHttpClient();
            _clientForWriteScope = ClientForReadScope.GetInstance().GetHttpClient();
        }

        [Test]
        [AllureDescription("Test to add user with all filled fields")]
        public async Task CheckUserWithAvailableZipCodeAddedAndThisZipCodeWasDeleted_Test()
        {
            logger.Info("Starting CheckUserWithAvailableZipCodeAddedAndThisZipCodeWasDeleted_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all available zip codes and select the first one." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync(_baseUrlZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(await getZipCodesResponse.Content.ReadAsStringAsync());
                int initialCount = availableZipCodes.Count;
                string selectedZipCode = availableZipCodes.FirstOrDefault();
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Send created user with available zip code and receive respond." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                var newUser = new
                {
                    age = RandomUserGenerator.GenerateRandomAge(),
                    name = RandomUserGenerator.GenerateRandomName(),
                    sex = RandomUserGenerator.GenerateRandomSex(),
                    zipCode = selectedZipCode
                };

                string newUserJson = JsonConvert.SerializeObject(newUser);
                StringContent content = new StringContent(newUserJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage postResponse = await _clientForWriteScope.PostAsync(_baseUrlUsers, content);
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Verify Status Code of the response and user is added to application and chosen zip code is removed from available zip codes." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);

                // GET request to /users endpoint to check if user is added
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                List<User> userList = JsonConvert.DeserializeObject<List<User>>(await getUsersResponse.Content.ReadAsStringAsync());
                bool userFound = userList.Any(u => u.Age == newUser.age && u.Name == newUser.name && u.Sex == newUser.sex && u.ZipCode == newUser.zipCode);

                Assert.That(userFound, Is.True, "Added user not found in user list.");

                // GET request to /zip-codes endpoint to check if selected zip code is removed
                HttpResponseMessage getUpdatedZipCodesResponse = await _clientForReadScope.GetAsync("/zip-codes");
                List<string> updatedZipCodes = JsonConvert.DeserializeObject<List<string>>(await getUpdatedZipCodesResponse.Content.ReadAsStringAsync());
                int finalCount = updatedZipCodes.Count;

                //The test fails cause of another pre-existing bug (available zip codes contain dublicates)
                Assert.That(!updatedZipCodes.Contains(selectedZipCode) && finalCount < initialCount, Is.True,
                $"Either selected zip code still exists in the list of available zip codes or the number of available zip codes didn't decrease after adding a new user.\n" +
                $"Selected Zip Code: {selectedZipCode}\n" +
                $"Initial Count: {initialCount}, Final Count: {finalCount}");

                logger.Info("CheckUserWithAvailableZipCodeAddedAndThisZipCodeWasDeleted_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }


        [Test]
        [AllureDescription("Test to add user with only required fields filled")]
        public async Task CheckUserWithOnlyRequiredDataWasAdded_Test()
        {
            logger.Info("Starting CheckUserWithOnlyRequiredDataWasAdded_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Send user with only required fields filled and receive respond." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                var newUser = new
                {
                    name = RandomUserGenerator.GenerateRandomName(),
                    sex = RandomUserGenerator.GenerateRandomSex()
                };

                string newUserJson = JsonConvert.SerializeObject(newUser);
                StringContent content = new StringContent(newUserJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage postResponse = await _clientForWriteScope.PostAsync(_baseUrlUsers, content);
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Verify Status Code of the response and user is added to application." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected staсектор приз на барабанеtus code 201 (Created) but received " + (int)postResponse.StatusCode);

                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                List<User> userList = JsonConvert.DeserializeObject<List<User>>(await getUsersResponse.Content.ReadAsStringAsync());
                bool userFound = userList.Any(u => u.Name == newUser.name && u.Sex == newUser.sex);

                Assert.That(userFound, Is.True, "Added user not found in user list.");

                logger.Info("CheckUserWithOnlyRequiredDataWasAdded_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to add user with unavailable zip code.")]
        public async Task CheckUserWithUnavailableZipCodeWasNotAdded_Test()
        {
            logger.Info("Starting CheckUserWithUnavailableZipCodeWasNotAdded_Test.");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all available zip codes and create unavailable." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);

                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync(_baseUrlZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(await getZipCodesResponse.Content.ReadAsStringAsync());

                string unavailableZipCode;
                do
                {
                    unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
                } 
                while (availableZipCodes.Contains(unavailableZipCode));

                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Send created user with unavailable zip code and receive respond." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);

                var newUser = new
                {
                    age = RandomUserGenerator.GenerateRandomAge(),
                    name = RandomUserGenerator.GenerateRandomName(),
                    sex = RandomUserGenerator.GenerateRandomSex(),
                    zipCode = unavailableZipCode
                };

                string newUserJson = JsonConvert.SerializeObject(newUser);
                StringContent content = new StringContent(newUserJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage postResponse = await _clientForWriteScope.PostAsync(_baseUrlUsers, content);
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Verify Status Code of the response and user is not added to application." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);

                Assert.That((int)postResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)postResponse.StatusCode);

                // GET request to /users endpoint to check if user is not added
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                List<User> userList = JsonConvert.DeserializeObject<List<User>>(await getUsersResponse.Content.ReadAsStringAsync());
                bool userFound = userList.Any(u => u.Age == newUser.age && u.Name == newUser.name && u.Sex == newUser.sex && u.ZipCode == newUser.zipCode);

                Assert.That(userFound, Is.False, "User is found in user list.");

                logger.Info("CheckUserWithUnavailableZipCodeWasNotAdded_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to add user with the same name and sex as existing user in the system.")]
        [AllureIssue("BUG: The system displays 201 code after posting user's data with the same Name and Sex as for existing user.")]
        public async Task CheckSentUserWithExistingNameAndSexWasNotAdded_Test()
        {
            logger.Info("Starting CheckUserWithUnavailableZipCodeWasNotAdded_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get existing user and create a new one using its name and sex." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);

                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                List<User> userList = JsonConvert.DeserializeObject<List<User>>(await getUsersResponse.Content.ReadAsStringAsync());
                var initialUserCount = userList.Count;
                var userFound = userList.FirstOrDefault();

                var newUser = new
                {
                    name = userFound.Name,
                    sex = userFound.Sex,
                };

                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Send created user and receive response." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);

                string newUserJson = JsonConvert.SerializeObject(newUser);
                StringContent content = new StringContent(newUserJson, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage postResponse = await _clientForWriteScope.PostAsync(_baseUrlUsers, content);

                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Verify Status Code of the response and user is not added to application." };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);

                Assert.That((int)postResponse.StatusCode, Is.EqualTo(400), "Expected status code 400 but received " + (int)postResponse.StatusCode);

                // Check if sent user is not added
                HttpResponseMessage getUsersResponseUpdated = await _clientForReadScope.GetAsync(_baseUrlUsers);
                List<User> userListUpdated = JsonConvert.DeserializeObject<List<User>>(await getUsersResponseUpdated.Content.ReadAsStringAsync());
                var finalUserCount = userListUpdated.Count;

                Assert.That(initialUserCount, Is.EqualTo(finalUserCount), "Number of users should remain the same.");

                logger.Info("CheckUserWithUnavailableZipCodeWasNotAdded_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occurred: {0}", ex.Message);
            }
        }

        /*Bug: The system displays 201 code after posting user's data with the same Name and Sex as for existing user
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains 
         * user to add with the same name and sex as existing user in the system
         * 
         * Expected result: there is 400 response code
         * Actual result: there is 201(Created) response code 
         * Note: Tne new user wasn't created in the system
         */
    }
}
