//using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OAuthConnection;

namespace simpleOAuthClient.Tests
{
    [TestFixture]
    public class OAuthConnectionTests
    {
        [Test]
        public void FirstConnetionShouldGetGrantCode()
        {
            var thisConnection = new Connection("grc");
            thisConnection.FetchGrantCode();
            Assert.IsTrue(thisConnection.HasGrantCode());
        }
    }
}
