using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class DeleteUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadUsers;
        private RestRequest _requestForReadZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        [SetUp]
        public void Setup()
        {
            var clientForReadScope = ClientForReadScope.GetInstance();
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadUsers = new RestRequest("/users");
            _requestForReadZipCodes = new RestRequest("/zip-codes");

            var clientForWriteScope = ClientForWriteScope.GetInstance();
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            _requestForWriteScope = new RestRequest("/users ", Method.Delete);
        }

        [Test]
        public void DeleteAnyUser_CheckTheUserWasDeleted_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
            int initialCountUsers = allUsers.Count;
            User selectedUser = allUsers[0];

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            int initialCountZipCodes = availableZipCodes.Count;

            _requestForWriteScope.AddJsonBody(selectedUser);
            RestResponse deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            // check if user is deleted
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
            int finalCount = updatedUsers.Count;

            //check if zip code for deleted user is available
            RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
            int finalCountZipCodes = availableZipCodesUpdated.Count;

            Assert.Multiple(() =>
            {
                Assert.That((int)deleteResponse.StatusCode, Is.EqualTo(204), "Expected status code 204 but received " + (int)deleteResponse.StatusCode);
                Assert.That(userFound, Is.False, "The user wasn't deleted!");
                Assert.That(finalCount == initialCountUsers - 1, Is.True, "Unexpected number of users after deletion:" + finalCount + ", expected number:" + initialCountUsers);
                Assert.That(availableZipCodesUpdated.Contains(selectedUser.ZipCode) && finalCountZipCodes == initialCountZipCodes + 1, Is.True,
                $"Zip Code of deleted user wasn't added in list of available Zip Codes.\n" +
                $"Zip Code needed to be added in the list: {selectedUser.ZipCode}\n" +
                $"Number of available Zipcodes before deletion: {initialCountZipCodes}, after deletion: {finalCountZipCodes}");
            });

        }

        [Test]
        public void DeleteAnyUserByRequiredFieldsOnly_CheckTheUserWasDeleted_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
            int initialCountUsers = allUsers.Count;
            User selectedUser = allUsers[0];
            var userToDelete = new
            {
                Name = selectedUser.Name,
                Sex = selectedUser.Sex
            };

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            int initialCountZipCodes = availableZipCodes.Count;

            _requestForWriteScope.AddJsonBody(userToDelete);
            RestResponse deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            // check if user is deleted
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
            int finalCount = updatedUsers.Count;

            //check if zip code for deleted user is available
            RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
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
        }
        /*Bug: The user wasn't deleted by sending delete request with the only required fields
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
        public void DeleteAnyUserWithMissedRequiredField_CheckTheUserWasNotDeleted_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsers = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> allUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsers.Content);
            int initialCountUsers = allUsers.Count;
            User selectedUser = allUsers[0];
            var userToDelete = new
            {
                Age = selectedUser.Age,
                Sex = selectedUser.Sex,
                ZipCode = selectedUser.ZipCode,
            };

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            int initialCountZipCodes = availableZipCodes.Count;

            _requestForWriteScope.AddJsonBody(userToDelete);
            RestResponse deleteResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            // check if user is not deleted
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = updatedUsers.Any(u => u.Age == selectedUser.Age && u.Name == selectedUser.Name && u.Sex == selectedUser.Sex && u.ZipCode == selectedUser.ZipCode);
            int finalCount = updatedUsers.Count;

            //check if zip code for not deleted user isn't available
            RestResponse getZipCodesResponseUpdated = _clientForReadScope.Execute(_requestForReadZipCodes);
            List<string> availableZipCodesUpdated = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponseUpdated.Content);
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
        }
    }
}
