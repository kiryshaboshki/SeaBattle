using System;
using System.Collections.Generic;

namespace SeaBattleRepository.Models
{
    public class Game
    {
        public int Id { get; set; }
        public int Status { get; set; } // 0-создана, 1-идёт, 2-завершена
        public int? IdUserWinner { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int CreatorUserId { get; set; }
        public int? OpponentUserId { get; set; }
        public byte[] FieldUser1 { get; set; } = new byte[100];
        public byte[] FieldUser2 { get; set; } = new byte[100];
        public int IdUserNextTurn { get; set; }
        public DateTime? DatetimeLastTurn { get; set; }
    }
}