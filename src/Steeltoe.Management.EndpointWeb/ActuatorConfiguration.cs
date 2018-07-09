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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.Diagnostics;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Env;
using Steeltoe.Management.Endpoint.Handler;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Health.Contributor;
using Steeltoe.Management.Endpoint.HeapDump;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Info.Contributor;
using Steeltoe.Management.Endpoint.Loggers;
using Steeltoe.Management.Endpoint.Metrics;
using Steeltoe.Management.Endpoint.Metrics.Observer;
using Steeltoe.Management.Endpoint.Refresh;
using Steeltoe.Management.Endpoint.Security;
using Steeltoe.Management.Endpoint.ThreadDump;
using Steeltoe.Management.Endpoint.Trace;
using Steeltoe.Management.Endpoint.Trace.Observer;
using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Steeltoe.Management.Endpoint
{
    public static class ActuatorConfiguration
    {
        public static ILoggerFactory LoggerFactory { get; set; }

        public static void ConfigureForCloudFoundry(IConfiguration configuration, ILoggerProvider dynamicLogger, IApiExplorer apiExplorer = null, ILoggerFactory loggerFactory = null)
        {
            ConfigureSecurityService(configuration, null, loggerFactory);
            ConfigureCloudFoundryEndpoint(configuration, loggerFactory);
            ConfigureHealthEndpoint(configuration, null, null, loggerFactory);
            ConfigureHeapDumpEndpoint(configuration, null, loggerFactory);
            ConfigureThreadDumpEndpoint(configuration, null, loggerFactory);
            ConfigureInfoEndpoint(configuration, null, loggerFactory);
            ConfigureLoggerEndpoint(configuration, dynamicLogger, loggerFactory);
            ConfigureTraceEndpoint(configuration, null, loggerFactory);
        }

        public static void ConfigureAll(IConfiguration configuration, ILoggerProvider dynamicLogger, IApiExplorer apiExplorer = null, ILoggerFactory loggerFactory = null)
        {
            ConfigureForCloudFoundry(configuration, dynamicLogger, apiExplorer, loggerFactory);
            ConfigureEnvEndpoint(configuration, null, loggerFactory);
            ConfigureRefreshEndpoint(configuration, loggerFactory);
            ConfigureMetricsEndpoint(configuration, loggerFactory);
        }

        public static void ConfigureSecurityService(IConfiguration configuration, ISecurityService securityService, ILoggerFactory loggerFactory = null)
        {
            SecurityService = securityService ?? new CloudFoundrySecurity(new CloudFoundryOptions(configuration), CreateLogger<CloudFoundrySecurity>(loggerFactory));
        }

        public static void ConfigureCloudFoundryEndpoint(IConfiguration configuration, ILoggerFactory loggerFactory = null)
        {
            var ep = new CloudFoundryEndpoint(new CloudFoundryOptions(configuration), CreateLogger<CloudFoundryEndpoint>(loggerFactory));
            var handler = new CloudFoundryHandler(ep, CreateLogger<CloudFoundryHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureHeapDumpEndpoint(IConfiguration configuration, IHeapDumper heapDumper = null, ILoggerFactory loggerFactory = null)
        {
            var options = new HeapDumpOptions(configuration);
            heapDumper = heapDumper ?? new HeapDumper(options);
            var ep = new HeapDumpEndpoint(options, heapDumper, CreateLogger<HeapDumpEndpoint>(loggerFactory));
            var handler = new HeapDumpHandler(ep, CreateLogger<HeapDumpHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureHealthEndpoint(IConfiguration configuration, IHealthAggregator healthAggregator = null, IEnumerable<IHealthContributor> contributors = null, ILoggerFactory loggerFactory = null)
        {
            var options = new HealthOptions(configuration);
            healthAggregator = healthAggregator ?? new DefaultHealthAggregator();
            contributors = contributors ?? new List<IHealthContributor>() { new DiskSpaceContributor(new DiskSpaceContributorOptions(configuration)) };
            var ep = new HealthEndpoint(options, healthAggregator, contributors, CreateLogger<HealthEndpoint>(loggerFactory));
            var handler = new HealthHandler(ep, CreateLogger<HealthHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureInfoEndpoint(IConfiguration configuration, IEnumerable<IInfoContributor> contributors = null, ILoggerFactory loggerFactory = null)
        {
            var options = new InfoOptions(configuration);
            contributors = contributors ?? new List<IInfoContributor>() { new GitInfoContributor(), new AppSettingsInfoContributor(configuration) };
            var ep = new InfoEndpoint(options, contributors, CreateLogger<InfoEndpoint>(loggerFactory));
            var handler = new InfoHandler(ep, CreateLogger<InfoHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureLoggerEndpoint(IConfiguration configuration, ILoggerProvider loggerProvider, ILoggerFactory loggerFactory = null)
        {
            var ep = new LoggersEndpoint(new LoggersOptions(configuration), loggerProvider, CreateLogger<LoggersEndpoint>(loggerFactory));
            var handler = new LoggersHandler(ep, CreateLogger<LoggersHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureThreadDumpEndpoint(IConfiguration configuration, IThreadDumper threadDumper = null, ILoggerFactory loggerFactory = null)
        {
            var options = new ThreadDumpOptions(configuration);
            threadDumper = threadDumper ?? new ThreadDumper(options);
            var ep = new ThreadDumpEndpoint(options, threadDumper, CreateLogger<ThreadDumpEndpoint>(loggerFactory));
            var handler = new ThreadDumpHandler(ep, CreateLogger<ThreadDumpHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureTraceEndpoint(IConfiguration configuration, ITraceRepository traceRepository, ILoggerFactory loggerFactory = null)
        {
            var options = new TraceOptions(configuration);
            traceRepository = traceRepository ?? new TraceDiagnosticObserver(options, CreateLogger<TraceDiagnosticObserver>(loggerFactory));
            DiagnosticsManager.Instance.Observers.Add((IDiagnosticObserver)traceRepository);
            var ep = new TraceEndpoint(options, traceRepository, CreateLogger<TraceEndpoint>(loggerFactory));
            var handler = new TraceHandler(ep, CreateLogger<TraceHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureRefreshEndpoint(IConfiguration configuration, ILoggerFactory loggerFactory = null)
        {
            var ep = new RefreshEndpoint(new RefreshOptions(configuration), configuration, CreateLogger<RefreshEndpoint>(loggerFactory));
            var handler = new RefreshHandler(ep, CreateLogger<RefreshHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureEnvEndpoint(IConfiguration configuration, IHostingEnvironment hostingEnvironment = null, ILoggerFactory loggerFactory = null)
        {
            var options = new EnvOptions(configuration);
            hostingEnvironment = hostingEnvironment ?? new DefaultHostingEnvironment("development");
            var ep = new EnvEndpoint(options, configuration, hostingEnvironment, CreateLogger<EnvEndpoint>(loggerFactory));
            var handler = new EnvHandler(ep, CreateLogger<EnvHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        public static void ConfigureMetricsEndpoint(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var options = new MetricsOptions(configuration);

            var hostObserver = new AspNetHostingObserver(options, OpenCensusStats.Instance, OpenCensusTags.Instance, CreateLogger<AspNetHostingObserver>(loggerFactory));
            var clrObserver = new CLRRuntimeObserver(options, OpenCensusStats.Instance, OpenCensusTags.Instance, CreateLogger<CLRRuntimeObserver>(loggerFactory));
            DiagnosticsManager.Instance.Observers.Add((IDiagnosticObserver)hostObserver);
            DiagnosticsManager.Instance.Observers.Add((IDiagnosticObserver)clrObserver);

            var clrSource = new CLRRuntimeSource();
            DiagnosticsManager.Instance.Sources.Add(clrSource);
            var ep = new MetricsEndpoint(options, OpenCensusStats.Instance, CreateLogger<MetricsEndpoint>(loggerFactory));
            var handler = new MetricsHandler(ep, CreateLogger<MetricsHandler>(loggerFactory));
            ConfiguredEndpoints.Add(ep, handler);
        }

        internal static ISecurityService SecurityService { get; set; }

        internal static IDictionary<IEndpoint, IActuatorHandler> ConfiguredEndpoints { get; } = new Dictionary<IEndpoint, IActuatorHandler>();

        private static ILogger<T> CreateLogger<T>(ILoggerFactory loggerFactory)
        {
            return loggerFactory != null ? loggerFactory.CreateLogger<T>() : LoggerFactory?.CreateLogger<T>();
        }

        public class DefaultHostingEnvironment : IHostingEnvironment
        {
            private readonly string profile;

            public DefaultHostingEnvironment(string profile)
            {
                this.profile = profile;
            }

            public string EnvironmentName { get => profile; set => throw new NotImplementedException(); }

            public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
