using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace AuthServer.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsActive { get; set; }
        public List<UserClaim> Claims { get; set; }
        public List<UserRole> Roles { get; set; }

        public ApplicationUser()
        {
            Claims = new List<UserClaim>();
            Roles = new List<UserRole>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }

    public class UserClaim : IdentityUserClaim<string>
    {
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UserRole : IdentityUserRole<string>
    {
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
