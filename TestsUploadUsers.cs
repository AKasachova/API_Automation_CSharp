using Newtonsoft.Json;
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
            _requestForWriteScope = new RestRequest("/users/upload", Method.Put);
        }

        [Test]
        public void CheckAllUsersAreReplacedWithFiled_Test()
        {
            _clientForReadScope.AddDefaultHeader("Accept", "application/json");
            RestResponse getAllUsersResponse = _clientForReadScope.Execute(_requestForReadScopeGetUsers);
            List<User> initialUsers = JsonConvert.DeserializeObject<List<User>>(getAllUsersResponse.Content);
            int initialCount = initialUsers.Count;

            RestResponse getZipCodesResponse = _clientForReadScope.Execute(_requestForReadScopeGetZipCodes);
            List<string> availableZipCodes = JsonConvert.DeserializeObject<List<string>>(getZipCodesResponse.Content);
            int initialCountZipCodes = availableZipCodes.Count;

            /* List<User> users = new List<User>
             {   
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[0]},
                 new User{Age = RandomUserGenerator.GenerateRandomAge(), Name = RandomUserGenerator.GenerateRandomName(), Sex = RandomUserGenerator.GenerateRandomSex(), ZipCode = availableZipCodes[1]}
             };

             string jsonFilePath = "C:\\Users\\kosac_xmbzi23\\OneDrive\\Рабочий стол\\API_Automation_CSharp\\API_C_sharp\\Test_data\\Users.json";
             string jsonContent = JsonConvert.SerializeObject(users);
             using (StreamWriter streamWriter = File.CreateText(jsonFilePath))
             {
                 streamWriter.Write(jsonContent);
             }*/

            string jsonFilePath = "C:\\Users\\kosac_xmbzi23\\OneDrive\\Рабочий стол\\API_Automation_CSharp\\API_C_sharp\\Test_data\\Users.json";
            _requestForWriteScope.AddFile("file", jsonFilePath, "multipart/form-data");

            RestResponse postResponse = _clientForWriteScope.Execute(_requestForWriteScope);
            
            //File.WriteAllText(jsonFilePath, string.Empty);

            Assert.That((int)postResponse.StatusCode, Is.EqualTo(201), "Expected status code 201 (Created) but received " + (int)postResponse.StatusCode);
            
        }
    }
}