using Microsoft.AspNetCore.Identity;

namespace WebAPI.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
