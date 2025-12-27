using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    [GenerateSerializer, Immutable]
    public record UserProfileAudit: UserProfile
    {
        [Id(5)]
        public ActionType ActionType { get; set; } = ActionType.Create; 
        [Id(6)]
        public string GrainKey { get; set; }

    }
}
