using System.ComponentModel.DataAnnotations;

namespace BookManagementPortalProxy.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
