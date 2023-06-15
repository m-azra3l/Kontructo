using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kontructo.Models
{
    public class Product
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string? Name { get; set; }

        [Required, StringLength(20)]
        public string? Url { get; set; }

        [Required, StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string? Price { get; set; }

        [StringLength(50)]
        public string? Vendor { get; set; }

        public byte[]? ProductImage { get; set; }

        public virtual ProductType? ProductType { get; set; }

        [Required, ForeignKey("ProductType")]
        public int ProductTypeId { get; set; }

    }
}
