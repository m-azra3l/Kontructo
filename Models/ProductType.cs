using System.ComponentModel.DataAnnotations;

namespace Kontructo.Models
{
    public class ProductType
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string? Tag { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
