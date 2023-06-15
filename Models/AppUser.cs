using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Kontructo.Models
{
    public class AppUser : IdentityUser
    {
        [Required,StringLength(30)]
        public string Name { get; set; }

        [Required,StringLength(150)]
        public string Address { get; set; }

        [Required,StringLength(10)]
        public string DOB { get; set; }

        [Required, StringLength(30)]
        public string Occupation { get; set; }

        [Required,StringLength(10)]
        public string Gender { get; set; }
       
        public byte[] Avatar { get; set; }


        [NotMapped]
        public bool IsAdmin { get; set; }

        [NotMapped]
        public bool IsSuperAdmin { get; set; }

        [NotMapped]
        public bool IsMaster { get; set; }

        [NotMapped]
        public bool IsMember { get; set; }


        public virtual ICollection<Product> Products { get; set; }

    }
}
