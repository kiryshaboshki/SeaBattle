using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleLogic
{
    public class GameTurn
    {
        public int IdUserNextTurn { get; set; }
        public int IdWinner { get; set; }
        public byte[] FieldUser { get; set; }
    }
}
