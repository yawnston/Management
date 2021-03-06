﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Steeltoe.Management.Endpoint.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xunit;

namespace Steeltoe.Management.Endpoint.CloudFoundry.Test
{
    public class EndpointMiddlewareTest : BaseTest
    {
        private Dictionary<string, string> appSettings = new Dictionary<string, string>()
        {
            ["management:endpoints:enabled"] = "true",
            ["management:endpoints:sensitive"] = "false",
            ["management:endpoints:path"] = "/cloudfoundryapplication",
            ["management:endpoints:info:enabled"] = "true",
            ["management:endpoints:info:sensitive"] = "false",
            ["info:application:name"] = "foobar",
            ["info:application:version"] = "1.0.0",
            ["info:application:date"] = "5/1/2008",
            ["info:application:time"] = "8:30:52 AM",
            ["info:NET:type"] = "Core",
            ["info:NET:version"] = "2.0.0",
            ["info:NET:ASPNET:type"] = "Core",
            ["info:NET:ASPNET:version"] = "2.0.0"
        };

        [Fact]
        public async void HandleCloudFoundryRequestAsync_ReturnsExpected()
        {
            var opts = new CloudFoundryOptions();
            var ep = new TestCloudFoundryEndpoint(opts);
            var middle = new CloudFoundryEndpointMiddleware(null, ep);
            var context = CreateRequest("GET", "/");
            await middle.HandleCloudFoundryRequestAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            StreamReader rdr = new StreamReader(context.Response.Body);
            string json = await rdr.ReadToEndAsync();
            Assert.Equal("{\"type\":\"steeltoe\",\"_links\":{}}", json);
        }

        [Fact]
        public async void CloudFoundryEndpointMiddleware_ReturnsExpectedData()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) => config.AddInMemoryCollection(appSettings));

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var json = await result.Content.ReadAsStringAsync();
                Assert.NotNull(json);
                var links = JsonConvert.DeserializeObject<Links>(json);
                Assert.NotNull(links);
                Assert.True(links._links.ContainsKey("self"));
                Assert.Equal("http://localhost/cloudfoundryapplication", links._links["self"].href);
                Assert.True(links._links.ContainsKey("info"));
                Assert.Equal("http://localhost/cloudfoundryapplication/info", links._links["info"].href);
            }
        }

        [Fact]
        public async void CloudFoundryEndpointMiddleware_ServiceContractNotBroken()
        {
            // arrange a server and client
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) => config.AddInMemoryCollection(appSettings));

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                // send the request
                var result = await client.GetAsync("http://localhost/cloudfoundryapplication");
                var json = await result.Content.ReadAsStringAsync();

                // assert
                Assert.Equal("{\"type\":\"steeltoe\",\"_links\":{\"self\":{\"href\":\"http://localhost/cloudfoundryapplication\"},\"info\":{\"href\":\"http://localhost/cloudfoundryapplication/info\"}}}", json);
            }
        }

        [Fact]
        public void CloudFoundryEndpointMiddleware_PathAndVerbMatching_ReturnsExpected()
        {
            var opts = new CloudFoundryOptions();
            var ep = new CloudFoundryEndpoint(opts);
            var middle = new CloudFoundryEndpointMiddleware(null, ep);

            Assert.True(middle.RequestVerbAndPathMatch("GET", "/"));
            Assert.False(middle.RequestVerbAndPathMatch("PUT", "/"));
            Assert.False(middle.RequestVerbAndPathMatch("GET", "/badpath"));
        }

        private HttpContext CreateRequest(string method, string path)
        {
            HttpContext context = new DefaultHttpContext();
            context.TraceIdentifier = Guid.NewGuid().ToString();
            context.Response.Body = new MemoryStream();
            context.Request.Method = method;
            context.Request.Path = new PathString(path);
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            return context;
        }
    }
}