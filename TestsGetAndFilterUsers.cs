using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class GetAndFilterUsersTests
    {
        private RestClient _client;
        private RestRequest _request;

        [SetUp]
        public void Setup()
        {
            var client = ClientForReadScope.GetInstance();
            _client = client.GetRestClient();
            _request = new RestRequest("/users");
        }

        [Test]
        public void GetAllUsers_ReturnsAllExpectedUsers_Test()
        {
            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            var expectedUsers = new List<User>
            {
                new User { Name = "Emma Jones", Age = 6, Sex = "MALE", ZipCode = "12345" },
                new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
                new User { Name = "James Brown", Age = 58, Sex = "MALE", ZipCode = "ABCDE" },
                new User { Name = "David Smith", Age = 24, Sex = "MALE", ZipCode = null },
                new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
            
            });
        }

        [Test]
        public void GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test()
        {
            int olderThan = 60;
            _request.AddParameter("olderThan", olderThan);

            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            var expectedUsers = new List<User>
            {
                new User { Name = "James Davis", Age = 73, Sex = "MALE", ZipCode = "23456" },
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
            });
        }

        [Test]
        public void GetFilteredUsersYoungerThan_ReturnsAllExpectedUsers_Test()
        {
            int youngerThan = 1;
            _request.AddParameter("youngerThan", youngerThan);

            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            var expectedUsers = new List<User> { };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
            });
        }

        [Test]
        public void GetFilteredUsersSex_ReturnsAllExpectedUsers_Test()
        {
            string sex = "FEMALE";
            _request.AddParameter("sex", sex);

            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            var expectedUsers = new List<User>
            {
                new User { Name = "Sophia Miller", Age = 59, Sex = "FEMALE", ZipCode = null }
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(actualUsers, Is.EquivalentTo(expectedUsers), "Received users list doesn't correspond expected one!");
            });
        }
    }  
}
