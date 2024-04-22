using NUnit.Framework;
using Allure.Commons;
using Newtonsoft.Json;
using NLog;
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;
using System.Text;

namespace APIAutomation.Tests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Delete Users")]
    public class DeleteUsersTests
    {
        private HttpClient _clientForReadScope;
        private HttpRequestMessage _requestForReadUsers;
        private HttpRequestMessage _requestForReadZipCodes;

        private HttpClient _clientForWriteScope;
        private HttpRequestMessage _requestForWriteScope;


        private HttpResponseMessage _deleteResponse;
        private User selectedUser;
        private int initialCountUsers;
        private int initialCountZipCodes;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            _clientForReadScope = ClientForReadScope.GetInstance().GetHttpClient();
            _requestForReadUsers = new HttpRequestMessage(HttpMethod.Get, "/users");
            _requestForReadZipCodes = new HttpRequestMessage(HttpMethod.Get, "/zip-codes");

            _clientForWriteScope = ClientForWriteScope.GetInstance().GetHttpClient();
            _requestForWriteScope = new HttpRequestMessage(HttpMethod.Delete, "/users");
        }

        [Test]
        [AllureDescription("Test to delete any user and verify that it was deleted and Zip code is returned in list of available zip codes")]
        public async Task DeleteAnyUser_CheckTheUserWasDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUser_CheckTheUserWasDeleted_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                HttpResponseMessage getAllUsers = await _clientForReadScope.SendAsync(_requestForReadUsers);
                string getAllUsersContent = await getAllUsers.Content.ReadAsStringAsync();
                var allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersContent);
                initialCountUsers = allUsers.Count;
                selectedUser = allUsers[0];
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.SendAsync(_requestForReadZipCodes);
                string getZipCodesContent = await getZipCodesResponse.Content.ReadAsStringAsync();
                var availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesContent);
                initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var content = new StringContent(JsonConvert.SerializeObject(selectedUser), System.Text.Encoding.UTF8, "application/json");
                _requestForWriteScope.Content = content;

                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, JsonConvert.SerializeObject(selectedUser));
                AllureLifecycle.Instance.AddAttachment("Request Payload", "application/json", tempFilePath);

                _deleteResponse = await _clientForWriteScope.SendAsync(_requestForWriteScope);
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User is deleted And Zip code is returned in list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.SendAsync(_requestForReadUsers);
                string getUsersContent = await getUsersResponse.Content.ReadAsStringAsync();
                var updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersContent);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                HttpResponseMessage getZipCodesResponseUpdated = await _clientForReadScope.SendAsync(_requestForReadZipCodes);
                string getZipCodesContentUpdated = await getZipCodesResponseUpdated.Content.ReadAsStringAsync();
                var availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesContentUpdated);
                int finalCountZipCodes = availableZipCodesUpdated.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)_deleteResponse.StatusCode, Is.EqualTo(204), "Expected status code 204 but received " + (int)_deleteResponse.StatusCode);
                    Assert.That(userFound, Is.False, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers - 1, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes + 1, Is.True,
                    $"Zip Code of deleted user wasn't added in list of available Zip Codes.\n" +
                    $"Zip Code needed to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });

                logger.Info("DeleteAnyUser_CheckTheUserWasDeleted_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        [Test]
        [AllureDescription("Test to delete user by it's required data and verify that it was deleted and Zip code is returned in list of available zip codes")]
        [AllureIssue("BUG: The user wasn't deleted by sending delete request with the only required fields.")]
        public async Task DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test");
            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one, take it's required fields data" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                HttpResponseMessage getAllUsers = await _clientForReadScope.GetAsync("/users");
                string getAllUsersContent = await getAllUsers.Content.ReadAsStringAsync();
                var allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersContent);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                var userToDelete = new
                {
                    Name = selectedUser.Name,
                    Sex = selectedUser.Sex
                };
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync("/zip-codes");
                string getZipCodesContent = await getZipCodesResponse.Content.ReadAsStringAsync();
                var availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesContent);
                int initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var jsonContent = JsonConvert.SerializeObject(userToDelete);
                var deleteResponse = await _clientForWriteScope.DeleteAsync("/users");
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User is deleted And Zip code is returned in list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync("/users");
                string getUsersContent = await getUsersResponse.Content.ReadAsStringAsync();
                var updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersContent);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                HttpResponseMessage getZipCodesResponseUpdated = await _clientForReadScope.GetAsync("/zip-codes");
                string getZipCodesContentUpdated = await getZipCodesResponseUpdated.Content.ReadAsStringAsync();
                var availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesContentUpdated);
                int finalCountZipCodes = availableZipCodesUpdated.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(204), "Expected status code 204 but received " + (int)deleteResponse.StatusCode);

                    //The test fails (the user wasn't deleted)
                    Assert.That(userFound, Is.False, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers - 1, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes + 1, Is.True,
                    $"Zip Code of deleted user wasn't added in list of available Zip Codes.\n" +
                    $"Zip Code needed to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });
                logger.Info("DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test completed successfully.");
                AllureLifecycle.Instance.StopStep();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        /*Bug: The user wasn't deleted by sending delete request with the only required fields.
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send DELETE request to /users endpoint and Request body contains 
         * the only required fields (name and sex) for the user sent
         * 
         * Expected result: the user was deleted
         * Actual result: the user wasn't deleted 
         */

        [Test]
        [AllureDescription("Test to delete user(any required field is missed) and verify that it wasn't deleted")]
        public async Task DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test()
        {
            logger.Info("Starting DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test");

            try
            {
                StepResult step1 = new StepResult { name = "Step#1: Get all users, count them and select the first one to take it's data without any required field" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step1);
                HttpResponseMessage getAllUsers = await _clientForReadScope.GetAsync("/users");
                string getAllUsersContent = await getAllUsers.Content.ReadAsStringAsync();
                var allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersContent);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];
                var userToDelete = new
                {
                    Age = selectedUser.Age,
                    Sex = selectedUser.Sex,
                    ZipCode = selectedUser.ZipCode,
                };
                AllureLifecycle.Instance.StopStep();

                StepResult step2 = new StepResult { name = "Step#2: Get all available zip codes, count them" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step2);
                HttpResponseMessage getZipCodesResponse = await _clientForReadScope.GetAsync("/zip-codes");
                string getZipCodesContent = await getZipCodesResponse.Content.ReadAsStringAsync();
                var availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesContent);
                int initialCountZipCodes = availableZipCodes.Count;
                AllureLifecycle.Instance.StopStep();

                StepResult step3 = new StepResult { name = "Step#3: Delete user and receive the response" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step3);
                var jsonContent = JsonConvert.SerializeObject(userToDelete);
                var deleteResponse = await _clientForWriteScope.DeleteAsync("/users");
                AllureLifecycle.Instance.StopStep();

                StepResult step4 = new StepResult { name = "Step#4: Verify Status Code of the Delete response And User isn't deleted And Zip code wasn't deleted from  list of available zip codes" };
                AllureLifecycle.Instance.StartStep(TestContext.CurrentContext.Test.Name, step4);
                HttpResponseMessage getUsersResponse = await _clientForReadScope.GetAsync("/users");
                string getUsersContent = await getUsersResponse.Content.ReadAsStringAsync();
                var updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersContent);
                bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
                int finalCount = updatedUsers.Count;

                HttpResponseMessage getZipCodesResponseUpdated = await _clientForReadScope.GetAsync("/zip-codes");
                string getZipCodesContentUpdated = await getZipCodesResponseUpdated.Content.ReadAsStringAsync();
                var availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesContentUpdated);
                int finalCountZipCodes = availableZipCodesUpdated.Count;

                Assert.Multiple(() =>
                {
                    Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)deleteResponse.StatusCode);
                    Assert.That(userFound, Is.True, "The user wasn't deleted!");
                    Assert.That(finalCount == initialCountUsers, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                    Assert.That(!availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes, Is.True,
                    $"Zip Code of not deleted user was added in list of available Zip Codes.\n" +
                    $"Zip Code needed not to be added in the list: {selectedUser.ZipCode}\n" +
                    $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
                });
                AllureLifecycle.Instance.StopStep();
                logger.Info("DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test completed successfully.");

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured {0}", ex.Message);
            }
        }

        [TearDown]
        public void TearDown()
        {
            logger.Info("Tearing down tests...");
            LogManager.Shutdown();
        }
    }
}
