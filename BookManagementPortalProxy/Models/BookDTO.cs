using System.ComponentModel.DataAnnotations;

namespace BookManagementPortalProxy.Models
{
    public class BookDTO
    {
        [Required]
        public string BookName { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public double BookPrice { get; set; }
    }
}
