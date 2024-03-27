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
                new User { Name = "Michael Davis", Age = 3, Sex = "MALE", ZipCode = "12345" },
                new User { Name = "Olivia Johnson", Age = null, Sex = "FEMALE", ZipCode = null }
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(expectedUsers.Count, Is.EqualTo(actualUsers.Count), "Number of users in response doesn't match expected count!");
                Assert.That(actualUsers.Distinct().Count(), Is.EqualTo(expectedUsers.Count), "Response contains duplicates for users.");

                foreach (var actualUser in actualUsers)
                {
                    bool userFound = expectedUsers.Any(expectedUser =>
                        actualUser.Name == expectedUser.Name &&
                        actualUser.Age == expectedUser.Age &&
                        actualUser.Sex == expectedUser.Sex &&
                        actualUser.ZipCode == expectedUser.ZipCode);

                    Assert.That(userFound, Is.True, $"User Name:'{actualUser.Name}', Age: '{actualUser.Age}', Sex:'{actualUser.Sex}', Zip Code: '{actualUser.ZipCode}' " +
                        $"  is not found in the expected users list.");
                }
            });
        }

        [Test]
        public void GetFilteredUsersOlderThan_ReturnsAllExpectedUsers_Test()
        {
            int olderThan = 1;
            _request.AddParameter("olderThan", olderThan);

            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            var expectedUsers = new List<User>
            {
            new User { Name = "Michael Davis", Age = 3, Sex = "MALE", ZipCode = "12345" }
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(expectedUsers.Count, Is.EqualTo(actualUsers.Count), "Number of users in response doesn't match expected count!");
                Assert.That(actualUsers.Distinct().Count(), Is.EqualTo(expectedUsers.Count), "Response contains duplicates for users.");

                foreach (var actualUser in actualUsers)
                {
                    bool userFound = expectedUsers.Any(expectedUser =>
                        actualUser.Name == expectedUser.Name &&
                        actualUser.Age == expectedUser.Age &&
                        actualUser.Sex == expectedUser.Sex &&
                        actualUser.ZipCode == expectedUser.ZipCode);

                    Assert.That(userFound, Is.True, $"User Name:'{actualUser.Name}', Age: '{actualUser.Age}', Sex:'{actualUser.Sex}', Zip Code: '{actualUser.ZipCode}' " +
                        $"  is not found in the expected users list.");
                }
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
                Assert.That(expectedUsers.Count, Is.EqualTo(actualUsers.Count), "Number of users in response doesn't match expected count!");
                Assert.That(actualUsers.Distinct().Count(), Is.EqualTo(expectedUsers.Count), "Response contains duplicates for users.");

                foreach (var actualUser in actualUsers)
                {
                    bool userFound = expectedUsers.Any(expectedUser =>
                        actualUser.Name == expectedUser.Name &&
                        actualUser.Age == expectedUser.Age &&
                        actualUser.Sex == expectedUser.Sex &&
                        actualUser.ZipCode == expectedUser.ZipCode);

                    Assert.That(userFound, Is.True, $"User Name:'{actualUser.Name}', Age: '{actualUser.Age}', Sex:'{actualUser.Sex}', Zip Code: '{actualUser.ZipCode}' " +
                        $"  is not found in the expected users list.");
                }
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
                new User { Name = "Olivia Johnson", Age = null, Sex = "FEMALE", ZipCode = null }
            };

            List<User> actualUsers = JsonConvert.DeserializeObject<List<User>>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(expectedUsers.Count, Is.EqualTo(actualUsers.Count), "Number of users in response doesn't match expected count!");
                Assert.That(actualUsers.Distinct().Count(), Is.EqualTo(expectedUsers.Count), "Response contains duplicates for users.");

                foreach (var actualUser in actualUsers)
                {
                    bool userFound = expectedUsers.Any(expectedUser =>
                        actualUser.Name == expectedUser.Name &&
                        actualUser.Age == expectedUser.Age &&
                        actualUser.Sex == expectedUser.Sex &&
                        actualUser.ZipCode == expectedUser.ZipCode);

                    Assert.That(userFound, Is.True, $"User Name:'{actualUser.Name}', Age: '{actualUser.Age}', Sex:'{actualUser.Sex}', Zip Code: '{actualUser.ZipCode}' " +
                        $"  is not found in the expected users list.");
                }
            });
        }
    }  
}
