using System.Collections.Generic;

namespace CouchbaseWebAPI.Models
{
    public class RolDto
    {
        public string Email {  get; set; }
        public string Password { get; set; }
        public string RolName { get; set; }
    }
    public class RolsDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public ICollection<string> Rols { get; set; }
    }


}
