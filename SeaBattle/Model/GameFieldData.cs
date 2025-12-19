using System.Collections.Generic;

namespace SeaBattle
{
    public static class GameFieldData
    {
        public static int[,] PlayerField { get; set; }
        public static List<GameFieldShip> PlayerShips { get; set; }
        public static int[,] EnemyField { get; set; }

        static GameFieldData()
        {
            PlayerField = new int[10, 10];
            PlayerShips = new List<GameFieldShip>();
            EnemyField = new int[10, 10];
        }
    }

    // ПЕРЕИМЕНОВАНО из ShipVM
    public class GameFieldShip
    {
        public int Size { get; set; }
        public bool IsHorizontal { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public List<(int, int)> Cells { get; set; }
        public List<(int, int)> HitCells { get; set; }

        public bool IsDestroyed => HitCells.Count == Size;
    }
}