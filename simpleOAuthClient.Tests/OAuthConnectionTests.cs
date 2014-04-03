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
            var thisConnection = new Connection("box");
            thisConnection.FetchGrantCode();
            Assert.IsTrue(thisConnection.HasGrantCode());
        }
    }
}
