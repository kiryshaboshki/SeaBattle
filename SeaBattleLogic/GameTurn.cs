using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleLogic
{
    public class GameTurn
    {
        public bool IsMyTurn { get; set; }
        public byte[] FieldUser { get; set; } = Array.Empty<byte>();
        public int IdUserNextTurn { get; set; }
        public int IdWinner { get; set; }
    }
}
