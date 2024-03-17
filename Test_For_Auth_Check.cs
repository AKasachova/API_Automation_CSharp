using NUnit.Framework;

namespace APIAutomation
{
    public static class Test_For_Auth_Check
    {
        [Test]
        public static void Test1()
        {
            var request = BasicAuthenticatorForWriteScope.GetInstance("http://localhost:50990", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq", "POST").GetToken().Result;
        }

        [Test]
        public static void Test2()
        {
            var request = BasicAuthenticatorForReadScope.GetInstance("http://localhost:50990", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq", "GET").GetToken().Result;
        }
    }
}
