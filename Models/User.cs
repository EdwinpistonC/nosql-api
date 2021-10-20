using System.Collections.Generic;

namespace CouchbaseWebAPI.Models
{
    public class User
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public ICollection<string> Rols { get; set; }

    }
    public class UserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

    }
    public class AuthUser
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
