using System;
using System.Collections.Generic;

namespace SeaBattleRepository.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int Rating { get; set; }
        public List<int> GameIds { get; set; } = new List<int>();
    }
}