namespace SeaBattle.Models
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Rating { get; set; }
    }
}