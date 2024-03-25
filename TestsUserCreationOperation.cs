using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class UserCreationTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadScopeGetZipCodes;
        private RestRequest _requestForReadScopeGetUsers;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        [SetUp]
        public void Setup()
        {
            var clientForReadScope = ClientForReadScope.GetInstance("http://localhost:8000", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq");
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadScopeGetZipCodes = new RestRequest("/zip-codes");
            _requestForReadScopeGetUsers = new RestRequest("/users");

            var clientForWriteScope = ClientForWriteScope.GetInstance("http://localhost:8000", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq");
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            _requestForWriteScope = new RestRequest("/users ", Method.Post);
        }

        [Test]
        public void CheckUserWithAvailableZipCodeAddedAndThisZipCodeWasDeleted_Test()
        {
            //Get 1st Zip Code from available Zip Codes
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            int initialCount = availableZipCodes.Count;
            string selectedZipCode = availableZipCodes.FirstOrDefault();

            var newUser = new
            {
                age = RandomUserGenerator.GenerateRandomAge(),
                name = RandomUserGenerator.GenerateRandomName(),
                sex = RandomUserGenerator.GenerateRandomSex(),
                zipCode = selectedZipCode
            };

            _requestForWriteScope.AddJsonBody(newUser);
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);          

            Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);

            // GET request to /users endpoint to check if user is added
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> userList = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = userList.Any(u => u.Age == newUser.age && u.Name == newUser.name && u.Sex == newUser.sex && u.ZipCode == newUser.zipCode);

            Assert.That(userFound, Is.True, "Added user not found in user list.");

            // GET request to /zip-codes endpoint to check if selected zip code is removed
            RestResponse getUpdatedZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> updatedZipCodes = JsonConvert.DeserializeObject<List<string>>(getUpdatedZipCodesResponse.Content);
            int finalCount = updatedZipCodes.Count;

            //The test fails cause of another pre-existing bug (available zip codes contain dublicates)
            Assert.That(!updatedZipCodes.Contains(selectedZipCode) && finalCount < initialCount, Is.True,
            $"Either selected zip code still exists in the list of available zip codes or the number of available zip codes didn't decrease after adding a new user.\n" +
            $"Selected Zip Code: {selectedZipCode}\n" +
            $"Initial Count: {initialCount}, Final Count: {finalCount}");
        }

        [Test]
        public void CheckUserWithOnlyRequiredDataWasAdded_Test()
        {
            var newUser = new
            {
                name = RandomUserGenerator.GenerateRandomName(),
                sex = RandomUserGenerator.GenerateRandomSex()
            };

            _requestForWriteScope.AddJsonBody(newUser);
            _clientForWriteScope.AddDefaultHeader("Accept", "application/json");
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);

            // GET request to /users endpoint to check if user is added
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> userList = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = userList.Any(u => u.Name == newUser.name && u.Sex == newUser.sex);

            Assert.That(userFound, Is.True, "Added user not found in user list.");
        }

        [Test]
        public void CheckUserWithUnavailableZipCodeWasNotAdded_Test()
        {
            //Get available Zip Codes
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);

            string unavailableZipCode;
            do
            {
                unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode(); 
            }
            while (availableZipCodes.Contains(unavailableZipCode));

            var newUser = new
            {
                age = RandomUserGenerator.GenerateRandomAge(),
                name = RandomUserGenerator.GenerateRandomName(),
                sex = RandomUserGenerator.GenerateRandomSex(),
                zipCode = unavailableZipCode
            };

            _requestForWriteScope.AddJsonBody(newUser);
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            Assert.That((int)postResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)postResponse.StatusCode);

            // GET request to /users endpoint to check if user is not added
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> userList = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            bool userFound = userList.Any(u => u.Age == newUser.age && u.Name == newUser.name && u.Sex == newUser.sex && u.ZipCode == newUser.zipCode);

            Assert.That(userFound, Is.False, "User is found in user list.");
        }

        [Test]
        public void CheckSentUserWithExistingNameAndSexWasNotAdded_Test()
        {
            //Get existing users
            RestResponse getUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> userList = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            var initialUserCount = userList.Count;
            var userFound = userList[0];

            var newUser = new
            {
                name = userFound.Name,
                sex = userFound.Sex,
            };

            _requestForWriteScope.AddJsonBody(newUser);
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            //Test fail: status code equals 201(400 is expected)
            Assert.That((int)postResponse.StatusCode, Is.EqualTo(400), "Expected status code 400 but received " + (int)postResponse.StatusCode);

            //Check if sent user is not added
            RestResponse getUsersResponseUpdated = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> userListUpdated = JsonConvert.DeserializeObject<List<User>>(getUsersResponse.Content);
            var finalUserCount = userListUpdated.Count;

            Assert.That(initialUserCount == finalUserCount, Is.True, "Number of users should remain the same.");
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
