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

using Microsoft.Extensions.Logging;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Security;
using System.Web;

namespace Steeltoe.Management.Endpoint.Handler
{
    public class HealthHandler : ActuatorHandler<HealthEndpoint, HealthCheckResult>
    {
        public HealthHandler(IEndpoint<HealthCheckResult> endpoint, ISecurityService securityService, ILogger<HealthHandler> logger = null)
            : base(endpoint, securityService, null, true, logger)
        {
        }

        public override void HandleRequest(HttpContext context)
        {
            _logger?.LogTrace("Processing {SteeltoeEndpoint} request", typeof(HealthHandler));
            var result = _endpoint.Invoke();
            context.Response.Headers.Set("Content-Type", "application/vnd.spring-boot.actuator.v1+json");
            context.Response.Write(Serialize(result));
            context.Response.StatusCode = ((HealthEndpoint)_endpoint).GetStatusCode(result);
        }
    }
}
