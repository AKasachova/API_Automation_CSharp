using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using RestSharp;

namespace APIAutomation.Tests
{
    [TestFixture]
    public class ZipCodeEndpointGETTests
    {
        private RestClient _client;
        private RestRequest _request;

        [SetUp]
        public void Setup()
        {
            var client = ClientForReadScope.GetInstance("http://localhost:8000", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq");
            _client = client.GetRestClient();
            _request = new RestRequest("/zip-codes", Method.Get);
        }

        [Test]
        public void GetAllZipCodes_ReturnsAllAvailableZipCodes()
        {

            _client.AddDefaultHeader("Accept", "application/json");
            RestResponse response = _client.Execute(_request);

            List<string> expectedZipCodes = new List<string> { "12345", "23456", "ABCDE" };
            List<string> actualZipCodes = JsonConvert.DeserializeObject<List<string>>(response.Content);

            Assert.Multiple(() =>
            {
                //The test fails, Status Code for successful Get Method equals 201
                Assert.That((int)response.StatusCode, Is.EqualTo(200));
                Assert.That(expectedZipCodes.Count, Is.EqualTo(actualZipCodes.Count), "Number of zip codes in response doesn't match expected count!");
                Assert.That(actualZipCodes.Distinct().Count(), Is.EqualTo(expectedZipCodes.Count), "Response contains duplicate zip codes.");

                foreach (string zipCode in actualZipCodes)
                {
                    CollectionAssert.Contains(expectedZipCodes, zipCode, $"Unexpected zip code '{zipCode}' found in response.");
                }
            });

            /*Bug: The system displays 201 status code for GET request
             * 
             * Preconditions:
             * -the user is authorized
             * -have  available zip codes in the app
             * 
             * Steps:
             * 1. Send GET request to /zip-codes endpoint 
             * 
             * Expected result: success code 200 is returned
             * Actual result: success code 201 is returned 
             */
        }
    }

    [TestFixture]
    public class ZipCodeEndpointPOSTTests
    {
        private RestClient _client;
        private RestRequest _request;

        [SetUp]
        public void Setup()
        {
            var client = ClientForWriteScope.GetInstance("http://localhost:8000", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq");
            _client = client.GetRestClient();
            _request = new RestRequest("/zip-codes/expand", Method.Post);
        }
        private string[] GenerateUniqueZipCodes(int count)
        {
            List<string> zipCodes = new List<string>();

            for (int i = 0; i < count; i++)
            {
                zipCodes.Add(Guid.NewGuid().ToString().Substring(0, 5));
            }

            return zipCodes.ToArray();
        }

        private static string[] GenerateUniqueDataWithDuplicatesForAvailableZipCodes(int count)
        {
            List<string> zipCodes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string randomZipCode = Guid.NewGuid().ToString().Substring(0, 5);
                zipCodes.Add(randomZipCode);

                if (new Random().Next(2) == 0)
                {
                    zipCodes.Add(randomZipCode);
                }
            }
            return zipCodes.ToArray();
        }

        [Test]
        public void PostZipCodes_ReturnsAllAddedZipCodes()
        {
            // Generate unique zip codes for each request
            string[] sentZipCodes = GenerateUniqueZipCodes(6);
            string requestBody = JsonConvert.SerializeObject(sentZipCodes);
            _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            _client.AddDefaultHeader("Accept", "application/json");

            RestResponse response = _client.Execute(_request);
            string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(201));
                Assert.That(sentZipCodes.All(actualZipCodes.Contains), Is.True, "Sent zip codes are not equal to actual zip codes.");
            });
        }
        
        [Test]
        public void PostZipCodes_SendDuplicatesForAvailableZipCodes()
        {
            // Generate unique zip codes with duplicates for each request
            string[] sentZipCodes = GenerateUniqueDataWithDuplicatesForAvailableZipCodes(6);
            string requestBody = JsonConvert.SerializeObject(sentZipCodes);
            _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            _client.AddDefaultHeader("Accept", "application/json");

            RestResponse response = _client.Execute(_request);
            string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);


            bool hasDuplicates = actualZipCodes.GroupBy(x => x).Any(g => g.Count() > 1);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(201));
                Assert.That(sentZipCodes.All(actualZipCodes.Contains), Is.True, "Sent zip codes are not equal to actual zip codes.");
                Assert.That(hasDuplicates, Is.False, "Duplicate zip codes found among the received zip codes.");
            });
        }

        /*Bug: The system displays dublicates in available zip codes in response when the request has dublicates in available zip codes
         * 
         * Preconditions:
         * -the user is authorized
         * 
         * Steps:
         * 1. Send POST request to /zip-codes endpoint and request body contains list of zip codes 
         * which has duplications for available zip codes
         * 
         * Expected result: there are no duplications in available zip codes for the response 
         * Actual result: there are duplications in available zip codes for the response  
         */

        [Test]
        public void PostZipCodes_SendDuplicatesForExistingZipCodes()
        {
            // Existing and new zip codes
            string[] existingZipCodes = new string[] { "12345", "23456", "ABCDE" };
            string[] newZipCodes = GenerateUniqueZipCodes(3);
            string[] combinedZipCodes = existingZipCodes.Concat(newZipCodes).ToArray();

            string requestBody = JsonConvert.SerializeObject(combinedZipCodes);
            _request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            _client.AddDefaultHeader("Accept", "application/json");

            RestResponse response = _client.Execute(_request);
            string[] actualZipCodes = JsonConvert.DeserializeObject<string[]>(response.Content);


            bool hasDuplicatesForExistingZipCodes = existingZipCodes.Any(zipCode => actualZipCodes.Count(code => code == zipCode) > 1);

            Assert.Multiple(() =>
            {
                Assert.That((int)response.StatusCode, Is.EqualTo(201));
                Assert.That(hasDuplicatesForExistingZipCodes, Is.False, "Duplicate zip codes found among the existing zip codes in the received zip codes.");
            });
        }

        /*Bug: The system displays duplications between available zip codes and already used zip codes (request has duplications for already used zip codes )
         * 
         * Preconditions:
         * -the user is authorized
         * 
         * Steps:
         * 1. Send POST request to /zip-codes endpoint and request body contains list of zip codes 
         * which has duplications for existing zip codes
         * 
         * Expected result: there are no duplications between available zip codes and already used zip codes for the response 
         * Actual result: there are duplications between available zip codes and already used zip codes for the response 
         */
    }
}

