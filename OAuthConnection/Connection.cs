using RestSharp;
using RestSharp.Contrib;
using RestSharp.IntegrationTests.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OAuthConnection
{
    public class Connection
    {
        public string Provider { get; set; }

        private string _grantCode = null;

        private readonly Dictionary<string, string> _baseURL = new Dictionary<string, string>()
        {
            {"twitter",  "https://api.twitter.com"},
            {"grc", ""},
            {"box", "https://app.box.com/"}
        };

        private readonly Dictionary<string, string> _connectionCallbackBaseUrl = new Dictionary<string, string>()
        {
            {"twitter", "https://localhost"},
            {"grc", ""},
            {"box", "http://localhost"},
        };

        private readonly Dictionary<string, string> _connectionCallbackEndpoint = new Dictionary<string, string>()
        {
            {"twitter", "/twitterSignin"},
            {"grc", ""},
            {"box", "/boxauthorize"},
        };

        private readonly Dictionary<string, string> _consumerKey = new Dictionary<string, string>()
        {
            {"twitter",  " KEY REMOVED "},
            {"grc", ""},
            {"box", " KEY REMOVED "},
        };

        private readonly Dictionary<string, string> _consumerSecret = new Dictionary<string, string>()
        {
            {"twitter",  " KEY REMOVED "},
            {"grc", ""},
            {"box", " KEY REMOVED "},
        };
        private readonly Dictionary<string, string> _resourceEndpoint = new Dictionary<string, string>()
        {
            {"twitter",  "/oauth/authorize"},
            {"grc", ""},
            {"box", ""},
        };
        private readonly Dictionary<string, string> _authorizeEndpoint = new Dictionary<string, string>()
        {
            {"twitter",  "/oauth/request_token"},
            {"grc", ""},
            {"box", "/api/oauth2/authorize?response_type=code&client_id={0}&state=authenticated&redirect_uri={1}"}
        };

        public Connection(string providerName)
        {
            Provider = providerName;
        }

        public Boolean HasGrantCode()
        {
            return (_grantCode != null);
        }

        public void FetchGrantCode()
        {
            string baseUrl = _baseURL[Provider];
            string authorizeEndpoint = _authorizeEndpoint[Provider];
            string consumerKey = _consumerKey[Provider];

            string callbackUrl = ensureTrailingBackslash(
                    _connectionCallbackBaseUrl[Provider] +
                    ":" +
                    RandomUnusedPort() +
                    _connectionCallbackEndpoint[Provider]
                );

            _grantCode = magicNetworkStuffToGetGrantCode(baseUrl, authorizeEndpoint, consumerKey, callbackUrl);
        }

        private string magicNetworkStuffToGetGrantCode(string baseURL, string requestTokenUrl, string consumerKey, string callbackURL)
        {
            // using RestSharp 
            var client = new RestClient(baseURL);
            var request = new RestRequest(
                string.Format(
                requestTokenUrl,
                consumerKey, 
                callbackURL
                ), Method.POST);

            bool hasUserGrantedAccess = false;

            var url = client.BuildUri(request).ToString();


            // Set up a local HTTP server to accept callback
            string authCode = null;
            var resetEvent = new ManualResetEvent(false);

            using (var svr = SimpleServer.Create(callbackURL, context =>
            {
                var qs = HttpUtility.ParseQueryString(context.Request.RawUrl);
                authCode = qs["code"];

                if (!string.IsNullOrEmpty(authCode))
                {
                    // The user has granted access
                    hasUserGrantedAccess = true;
                }

                // Resume execution...
                resetEvent.Set();

            }))
            {
                // Launch a default browser to get the user's approval
                System.Diagnostics.Process.Start(url);

                // Wait until the user decides whether to grant access
                resetEvent.WaitOne();

            }

            if ( hasUserGrantedAccess)
            {
                return authCode;
            }
            else
            {
                return null;
            }


        }

        private string ensureTrailingBackslash(string url)
        {
            //return new Uri(new Uri(url), "/").ToString(); 
            return (String.CompareOrdinal(url.Substring(url.Length - 1), "/")==0) ? url : url + "/";
        }

        private static int RandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}

