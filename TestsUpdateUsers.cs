using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using NUnit.Allure.Core;
using NUnit.Allure.Attributes;
using Allure.Commons;
using System.Text;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Update Users")]
    public class UpdateUsersTests
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
            _clientForWriteScope = ClientForWriteScope.GetInstance().GetHttpClient();
        }

        [Test]
        [AllureDescription("Test to update user with all new values")]
        public async Task UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                HttpResponseMessage getAllUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string jsonResponseUsers = await getAllUsersResponse.Content.ReadAsStringAsync();
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(jsonResponseUsers);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync(_baseUrlZipCodes);
                string jsonResponseZipCodes = await getZipCodesResponse.Content.ReadAsStringAsync();
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(jsonResponseZipCodes);
                string availableZipCode = availableZipCodes[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(contains all new correct data) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        name = RandomUserGenerator.GenerateRandomName(),
                        sex = RandomUserGenerator.GenerateRandomSex(),
                        zipCode = availableZipCode
                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };
                string requestBody = JsonConvert.SerializeObject(updateUser);
                StringContent content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = await _clientForWriteScope.PutAsync(_baseUrlUsers, content);

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, requestBody);
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string jsonResponseUpdatedUsers = await getUsersResponse.Content.ReadAsStringAsync();
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(jsonResponseUpdatedUsers);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == updateUser.userNewValues.name && u.Sex == updateUser.userNewValues.sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(200), "Expected status code 200 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.True, "The user wasn't updated!");
                    Assert.That(oldUserFound, Is.False, "The user needed to be updated is still in the list of available users!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to update user with all new values and unavailable zip code")]
        [AllureIssue("BUG: The user needed not to be updated is deleted.")]
        public async Task UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);

                HttpResponseMessage getAllUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string allUsersJson = await getAllUsersResponse.Content.ReadAsStringAsync();
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(allUsersJson);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes and create unavailable" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync(_baseUrlZipCodes);
                string availableZipCodesJson = await getZipCodesResponse.Content.ReadAsStringAsync();
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(availableZipCodesJson);
                string unavailableZipCode;
                do
                {
                    unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
                }
                while (availableZipCodes.Contains(unavailableZipCode));
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(contains unavailable zip code) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        name = RandomUserGenerator.GenerateRandomName(),
                        sex = RandomUserGenerator.GenerateRandomSex(),
                        zipCode = unavailableZipCode

                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };

                string updateUserJson = JsonConvert.SerializeObject(updateUser);
                StringContent content = new StringContent(updateUserJson, Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = await _clientForWriteScope.PutAsync(_baseUrlUsers, content);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was not updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string updatedUsersJson = await getUsersResponse.Content.ReadAsStringAsync();
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(updatedUsersJson);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == updateUser.userNewValues.name && u.Sex == updateUser.userNewValues.sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.False, "The user was updated!");
                    //The test fails, the user needed not to be updated is deleted!
                    Assert.That(oldUserFound, Is.True, "The user needed not to be updated was updated or deleted!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The user needed not to be updated is deleted
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send PUT/PATCH request to /users endpoint and Request body contains 
         * user to update and new values and zip code is unavailable
         * 
         * Expected result: the user is not updated
         * Actual result: the user is deleted 
         */

        [Test]
        [AllureDescription("Test to update user with all new values and missed required fields")]
        [AllureIssue("BUG: The user needed not to be updated is deleted.")]
        public async Task UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);

                HttpResponseMessage getAllUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                string allUsersJson = await getAllUsersResponse.Content.ReadAsStringAsync();
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(allUsersJson);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync(_baseUrlZipCodes);
                string availableZipCodesJson = await getZipCodesResponse.Content.ReadAsStringAsync();
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(availableZipCodesJson);
                string availableZipCode = availableZipCodes[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Update selected user with new one(doesn't contain required fields) and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var updateUser = new
                {
                    userNewValues = new
                    {
                        age = RandomUserGenerator.GenerateRandomAge(),
                        zipCode = availableZipCode

                    },
                    userToChange = new
                    {
                        age = selectedUser.Age,
                        name = selectedUser.Name,
                        sex = selectedUser.Sex,
                        zipCode = selectedUser.ZipCode
                    }
                };

                string updateUserJson = JsonConvert.SerializeObject(updateUser);
                StringContent content = new StringContent(updateUserJson, Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = await _clientForWriteScope.PutAsync(_baseUrlUsers, content);
                updateResponse.EnsureSuccessStatusCode();
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the response and selected user was not updated with the new data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync(_baseUrlUsers);
                getUsersResponse.EnsureSuccessStatusCode();
                string updatedUsersJson = await getUsersResponse.Content.ReadAsStringAsync();
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(updatedUsersJson);
                bool updatedUserFound = updatedUsers.Any(u => u.Age == updateUser.userNewValues.age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == updateUser.userNewValues.zipCode);
                bool oldUserFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCountUsers = updatedUsers.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)updateResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)updateResponse.StatusCode);
                    Assert.That(updatedUserFound, Is.False, "The user was updated!");
                    //The test fails, the user needed not to be updated is deleted!
                    Assert.That(oldUserFound, Is.True, "The  user needed not to be updated was updated or deleted!");
                    Assert.That(finalCountUsers == initialCountUsers, Is.True, "Unexpected number of users after update:" + finalCountUsers + ", expected number:" + initialCountUsers);
                });
                logger.Info("UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        /*Bug: The user needed not to be updated is deleted
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send PUT/PATCH request to /users endpoint and Request body contains 
         * user to update and required fields are missed
         * 
         * Expected result: the user is not updated
         * Actual result: the user is deleted 
         */

        [TearDown]
        public void TearDown()
        {
            logger.Info("Tearing down tests...");
            LogManager.Shutdown();
        }
    }
}
