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
using Steeltoe.Management.EndpointOwin.Diagnostics;
using System;

namespace Steeltoe.Management.EndpointAutofac.Actuators
{
    public static class DiagnosticSourceContainerBuilderExtensions
    {
        /// <summary>
        /// Register the Diagnostics source OWIN middleware
        /// </summary>
        /// <param name="container">Autofac DI <see cref="ContainerBuilder"/></param>
        public static void RegisterDiagnosticSourceMiddleware(this ContainerBuilder container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            container.RegisterType<DiagnosticSourceOwinMiddleware>().SingleInstance();
        }
    }
}
