using Microsoft.AspNetCore.Identity;

namespace API_Credit_Card.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
