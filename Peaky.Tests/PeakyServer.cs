using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Peaky.Tests
{
    public class PeakyServer
    {
        private readonly HttpClient client;

        public TestServer CreateTestServer() =>
            new TestServer(new WebHostBuilder());

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
            client.SendAsync(request);
    }
}