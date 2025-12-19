using System;

namespace SeaBattle.Models
{
    public class GameDTO
    {
        public int Id { get; set; }
        public byte[] FieldUser1 { get; set; } = null!;
        public byte[] FieldUser2 { get; set; } = null!;
        public short Status { get; set; }
        public int IdUserNextTurn { get; set; }
        public DateTime DatetimeStartGame { get; set; }
        public DateTime? DatetimeLastTurn { get; set; }
        public int? IdUserWinner { get; set; }
        public UserDTO Creator { get; set; }
        public UserDTO Opponent { get; set; }
    }
}