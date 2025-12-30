using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gateway.Grains
{
    public class UserProfileState
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime UpdateDateTime { get; set; } = DateTime.MinValue;

        public ActionType Action { get; set; } = ActionType.Create;

        public UserProfileState Apply(UserProfileEvent evnt)
        {
            FirstName = evnt.FirstName;
            LastName = evnt.LastName;
            Email = evnt.Email;
            UpdateDateTime = DateTime.UtcNow;
            Action = evnt.Action;
            return this;
        }
    }
}
