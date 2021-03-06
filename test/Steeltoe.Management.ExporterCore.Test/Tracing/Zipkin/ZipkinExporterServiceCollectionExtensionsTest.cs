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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Steeltoe.Management.Census.Trace;
using Steeltoe.Management.Census.Trace.Config;
using Steeltoe.Management.Census.Trace.Export;
using Steeltoe.Management.Census.Trace.Propagation;
using Steeltoe.Management.Exporter.Tracing.Zipkin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Management.Exporter.Tracing.Test
{
    public class ZipkinExporterServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddZipkinExporter_ThrowsOnNulls()
        {
            // Arrange
            IServiceCollection services = null;
            IServiceCollection services2 = new ServiceCollection();
            IConfiguration config = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => ZipkinExporterServiceCollectionExtensions.AddZipkinExporter(services, config));
            Assert.Contains(nameof(services), ex.Message);
            var ex2 = Assert.Throws<ArgumentNullException>(() => ZipkinExporterServiceCollectionExtensions.AddZipkinExporter(services2, config));
            Assert.Contains(nameof(config), ex2.Message);
        }

        [Fact]
        public void AddZipkinExporter_AddsCorrectServices()
        {
            ServiceCollection services = new ServiceCollection();
            var config = GetConfiguration();

            services.AddOptions();
            services.AddSingleton<IHostingEnvironment>(new TestHost());
            services.AddSingleton<ITracing>(new TestTracing());

            services.AddZipkinExporter(config);

            var serviceProvider = services.BuildServiceProvider();

            var opts = serviceProvider.GetService<ITraceExporterOptions>();
            Assert.NotNull(opts);
            var tracing = serviceProvider.GetService<ITraceExporter>();
            Assert.NotNull(tracing);
        }

        private IConfiguration GetConfiguration()
        {
            var settings = new Dictionary<string, string>()
            {
                ["management:tracing:exporter:zipkin:serviceName"] = "foobar",
                ["management:tracing:exporter:zipkin:validateCertificates"] = "false",
                ["management:tracing:exporter:zipkin:timeoutSeconds"] = "100",
                ["management:tracing:exporter:zipkin:useShortTraceIds"] = "true",
                ["management:tracing:exporter:zipkin:endpoint"] = "http://foo.com/api/v2/spans"
            };

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(settings);
            return builder.Build();
        }

        private class TestHost : IHostingEnvironment
        {
            public string EnvironmentName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string ApplicationName { get => "foobar"; set => throw new NotImplementedException(); }

            public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        private class TestTracing : ITracing
        {
            public ITracer Tracer => null;

            public IPropagationComponent PropagationComponent => null;

            public IExportComponent ExportComponent => null;

            public ITraceConfig TraceConfig => null;
        }
    }
}
