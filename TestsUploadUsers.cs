using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class UploadUsersTests
    {
        private RestClient _clientForReadScope;
        private RestRequest _requestForReadScopeGetUsers;
        private RestRequest _requestForReadScopeGetZipCodes;

        private RestClient _clientForWriteScope;
        private RestRequest _requestForWriteScope;

        [SetUp]
        public void Setup()
        {
            var clientForReadScope = ClientForReadScope.GetInstance();
            _clientForReadScope = clientForReadScope.GetRestClient();
            _requestForReadScopeGetZipCodes = new RestRequest("/zip-codes");
            _requestForReadScopeGetUsers = new RestRequest("/users");

            var clientForWriteScope = ClientForWriteScope.GetInstance();
            _clientForWriteScope = clientForWriteScope.GetRestClient();
            _requestForWriteScope = new RestRequest("/users/upload", Method.Post);
        }

        [Test]
        public void CheckAllUsersAreReplacedWithFiled_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);

            List<User> users = new List<User>
             {
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[1]}
             };
            var usersCountFile = users.Count;

            string currentDirectory = Directory.GetCurrentDirectory();
            string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

            var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            File.WriteAllText(jsonFilePath, usersJson);


            _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);

            RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);
            var jsonResponse = getAllUpdatedUsersResponse.Content;
            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);
            int uploadedUsersCount = responseObject.Count;

            Assert.Multiple(() =>
            {
                Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);
                Assert.That(updatedUsers, Is.EquivalentTo(users), "Received users list doesn't correspond expected one!");
                Assert.That(uploadedUsersCount, Is.EqualTo(usersCountFile), "The number of users in the respond is not equivalent to the number of sent users!");
            });
        }

        [Test]
        public void CheckUsersAreNotAdded_UnavailableZipCode_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);


            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            string unavailableZipCode;
            do
            {
                unavailableZipCode = RandomUserGenerator.GenerateRandomZipCode();
            }
            while (availableZipCodes.Contains(unavailableZipCode));
            List<User> users = new List<User>
             {
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = unavailableZipCode}
             };

            string currentDirectory = Directory.GetCurrentDirectory();
            string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

            var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            File.WriteAllText(jsonFilePath, usersJson);
            _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");

            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
            RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);

            Assert.Multiple(() =>
            {
                //Test fails: Status code is 500
                Assert.That((int)postResponse.StatusCode, Is.EqualTo(424), "Expected status code 424 but received " + (int)postResponse.StatusCode);
                //Test fails: Received list contains the only one user sent in the file (user with available Zip Code)
                Assert.That(updatedUsers, Is.EquivalentTo(initialUsers), "Received users list doesn't correspond expected one!");
            });
        }

        /*Bug: The system displays 500 code after uploading json file with user with unavailable Zip Code
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains 
         * json file with users and at least one user has incorrect (unavailable) zip code
         * 
         * Expected result: there is 424 response code
         * Actual result: there is 500 response code*/


        /*Bug: Received list contains the only one user sent in the file (user with available Zip Code) 
         * after uploading json file with user with unavailable Zip Code
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains json file with
         * two users and 1st user has incorrect (unavailable) zip code, 2nd - available zip code
         * 
         * Expected result: initial list of users wasn't updated
         * Actual result: initial list of users was updated: Received list contains the only one user from the file (user with available Zip Code)*/

        [Test]
        public void CheckUsersAreNotAdded_MissedRequiredField_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);

            List<User> users = new List<User>
                    {
                        new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                        new User{Age = RandomUserGenerator.GenerateRandomAge(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[1]}
                    };

            string currentDirectory = Directory.GetCurrentDirectory();
            string jsonFilePath = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "Test_data", "Users.json");

            var usersJson = JsonConvert.SerializeObject(users, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            File.WriteAllText(jsonFilePath, usersJson);


            _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");
            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
            RestResponse getAllUpdatedUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> updatedUsers = JsonConvert.DeserializeObject<List<User>>(getAllUpdatedUsersResponse.Content);

            Assert.Multiple(() =>
            {
                //Test fails: Status code is 500
                Assert.That((int)postResponse.StatusCode, Is.EqualTo(409), "Expected status code 409 but received " + (int)postResponse.StatusCode);
                //Test fails: Received list contains the only one user sent in the file (user with all required fields)
                Assert.That(updatedUsers, Is.EquivalentTo(initialUsers), "Received users list doesn't correspond expected one!");
            });
        }

        /*Bug: The system displays 500 code after uploading json file with user with missed required field
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains 
         * json file with users and at least one user has missed required field
         * 
         * Expected result: there is 424 response code
         * Actual result: there is 500 response code*/


        /*Bug: Received list contains the only one user sent in the file (user with all correct data) 
         * after uploading json file with user with missed required field
         * 
         * Preconditions:
         * -the user is authorized to write and read content
         * 
         * Steps:
         * 1. Send POST request to /users endpoint and Request body contains json file with
         * two users and 1st user has all correct data, 2nd - missed required field
         * 
         * Expected result: initial list of users wasn't updated
         * Actual result: initial list of users was updated: Received list contains the only one user from the file (user with all correct data)*/
    }
}
