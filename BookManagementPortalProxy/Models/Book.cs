namespace BookManagementPortalProxy.Models
{
    public class Book
    {
        public int BookID { get; set; }

        public string BookName { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public double BookPrice { get; set; }
    }
}
