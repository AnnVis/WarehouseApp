namespace NetFrameworkDemo.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        // Stored hashed password (PBKDF2)
        public string PasswordHash { get; set; }
        // Add roles/claims if needed
        public string Role { get; set; }
    }
}