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

using Autofac;
using Microsoft.Extensions.Configuration;
using Steeltoe.Common.Diagnostics;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Trace;
using Steeltoe.Management.EndpointOwin;
using Steeltoe.Management.EndpointOwin.Trace;
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.EndpointAutofac.Actuators
{
    public static class TraceContainerBuilderExtensions
    {
        /// <summary>
        /// Register a request tracing middleware
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void RegisterTracingMiddleware(this ContainerBuilder container, IConfiguration config)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            container.RegisterType<RequestTracingOwinMiddleware>();
        }

        /// <summary>
        /// Register the Trace endpoint, middleware and options
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        /// <param name="config">Your application's <see cref="IConfiguration"/></param>
        public static void RegisterTraceActuator(this ContainerBuilder container, IConfiguration config)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            container.RegisterType<DiagnosticsManager>().As<IDiagnosticsManager>().SingleInstance();
            container.RegisterInstance(new TraceOptions(config)).As<ITraceOptions>().SingleInstance();
            container.RegisterType<TraceRepository>().As(typeof(ITraceRepository), typeof(TraceRepository)).SingleInstance();
            container.RegisterType<TraceEndpoint>().As<IEndpoint<List<TraceResult>>>().SingleInstance();
            container.RegisterType<EndpointOwinMiddleware<TraceEndpoint, List<TraceResult>>>();
        }
    }
}
