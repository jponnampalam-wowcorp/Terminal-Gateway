using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    public interface IHealthCheckGrain : IGrainWithGuidKey
    {
        Task<bool> CheckHealthAsync();
    }
}
