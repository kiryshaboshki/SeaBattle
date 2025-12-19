using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleRepository.DTO
{
    public class GameDTO
    {
        public int Id { get; set; }

        public byte[] FieldUser1 { get; set; } = null!;

        public byte[] FieldUser2 { get; set; } = null!;

        /// <summary>
        /// 0 start, 1 process, 2 end
        /// </summary>
        public short Status { get; set; }

        public int IdUserNextTurn { get; set; }

        public DateTime DatetimeStartGame { get; set; }

        public DateTime? DatetimeLastTurn { get; set; }

        public int? IdUserWinner { get; set; }

        public UserDTO Creator { get; set; }
        public UserDTO Opponent { get; set; }
    }
}
