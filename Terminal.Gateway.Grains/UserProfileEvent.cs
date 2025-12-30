using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    public class UserProfileEvent
    {

        public UserProfileEvent(User profile, ActionType action )
        {
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            UpdateDateTime = DateTime.UtcNow;
            Action = action;
            UserId = profile.UserId;
        }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime UpdateDateTime { get; set; } = DateTime.MinValue;

        public ActionType Action { get; set; } = ActionType.Create;

        public string UserId { get; set; }
    }
}
