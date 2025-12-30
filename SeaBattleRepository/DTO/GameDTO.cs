using System;
using System.Collections.Generic;

namespace SeaBattleRepository.DTO
{
    public class GameDTO
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public int? IdUserWinner { get; set; }
        public int CreatorUserId { get; set; }
        public int? OpponentUserId { get; set; }
        public byte[] FieldUser1 { get; set; } = new byte[100];
        public byte[] FieldUser2 { get; set; } = new byte[100];
        public int IdUserNextTurn { get; set; }
        public DateTime? DatetimeLastTurn { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
    }
}