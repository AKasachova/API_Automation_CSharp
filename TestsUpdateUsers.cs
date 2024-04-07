using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using NLog;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class UpdateUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadUsers;
        private RestRequest _requestForReadZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Setup()
        {
            logger.Info("Setting up tests...");

            var clientForReadScope = ClientForReadScope.GetInstance();
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadUsers = new RestRequest("/users");
            _requestForReadZipCodes = new RestRequest("/zip-codes");

            var clientForWriteScope = ClientForWriteScope.GetInstance();
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            string requestMethodParam = TestContext.Parameters["requestMethodParamForUpdateUsers"];
            Method requestMethod = Method.Put;
            if (requestMethodParam == "Patch")
            {
                requestMethod = Method.Patch; 
            }
            _requestForWriteScope = new RestRequest("/users", requestMethod);
        }

        [Test]
        public void UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithAllValidData_CheckTheUserWasUpdated_Test");

            try
            {
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];

                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string availableZipCode = availableZipCodes[0];

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
                _requestForWriteScope.AddJsonBody(updateUser);
                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);

                // check if user is updated
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
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
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occured: {0}", ex.Message);
            }
        }

        [Test]
        public void UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateUserWithUnavailableZipCode_CheckTheUserWasNotUpdated_Test");

            try
            {
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];

                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string unavailableZipCode;
                do
                {
                    unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
                }
                while (availableZipCodes.Contains(unavailableZipCode));

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
                _requestForWriteScope.AddJsonBody(updateUser);
                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);

                // check if user is not updated
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
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
        public void UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test()
        {
            logger.Info("Starting UpdateAnyUserWithoutRequiredFields_CheckTheUserWasNotUpdated_Test");

            try
            {
                _clientForReadScope.AddDefaultHeader("Accept", "application/json");
                RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
                int initialCountUsers = allUsers.Count;
                User selectedUser = allUsers[0];

                RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
                List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
                string availableZipCode = availableZipCodes[0];

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
                _requestForWriteScope.AddJsonBody(updateUser);
                RestResponse updateResponse = _clientForWriteScope.Execute(_requestForWriteScope);

                // check if user is not updated
                RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
                List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
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
    }
}
