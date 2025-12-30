using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    public class HealthCheckGrain : Grain, IHealthCheckGrain
    {
        public Task<bool> CheckHealthAsync()
        {
            // Add additional checks here if necessary (e.g., database connectivity)
            return Task.FromResult(true);
        }
    }
}
