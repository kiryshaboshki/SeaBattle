using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleLogic
{
    internal class FieldGeneration
    {
        static Random random = new Random();
        // 0 - незаполненные ячейки
        // 1 - корабль
        // корабли не изгибаются, не стоят рядом друг с другом
        public static byte[,] Execute()
        {
            byte[,] field = new byte[10, 10];
            
            DrawShip(field, 4);
            DrawShip(field, 3);
            DrawShip(field, 3);
            DrawShip(field, 2);
            DrawShip(field, 2);
            DrawShip(field, 2);
            DrawShip(field, 1);
            DrawShip(field, 1);
            DrawShip(field, 1);
            DrawShip(field, 1);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                    if (field[i, j] == 2)
                        field[i, j] = 0;
            }
            return field;
        }

        private static void DrawShip(byte[,] field, int length)
        {
            bool draw = false;
            do
            {
                draw = false;
                int x = random.Next(0, 10);
                int y = random.Next(0, 10);
                if (field[x, y] == 0)
                {
                    var direction = random.Next(0, 2);
                    if (TestDirection(field, direction, x, y, length))
                    {
                        Draw(field, direction, x, y, length);
                        draw = true;
                    }
                }
            }
            while (!draw);
        }

        private static void Draw(byte[,] field, int direction, int x, int y, int length)
        {
            switch (direction)
            {
                case 0: // направо
                    DrawWaterH(field, x - 1, y);
                    for (int i = 0; i < length; i++)
                        DrawSideH(field, x + i, y);
                    DrawWaterH(field, x + length, y);
                    break;
                case 1:
                    DrawWaterV(field, x, y - 1);
                    for (int i = 0; i < length; i++)
                        DrawSideV(field, x, y + i);
                    DrawWaterV(field, x, y + length);
                    break;
            }
        }

        static bool TestDirection(byte[,] field, int direction, int x, int y, int length)
        {
            switch (direction)
            {
                case 0: // направо
                    for (int i = -1; i < length + 1; i++)
                        if (!CheckSideH(field, x + i, y))
                            return false;
                    break;
                case 1:
                    for (int i = -1; i < length + 1; i++)
                        if (!CheckSideV(field, x, y + i))
                            return false;
                    break;
            }
            return true;
        }

        static void DrawSideH(byte[,] field, int x, int y)
        {
            if (x >= 0 && x <= 9 && y >= 0 && y <= 9)
                field[x, y] = 1;
            if (y - 1 >= 0)
                field[x, y - 1] = 2;
            if (y + 1 < 10)
                field[x, y + 1] = 2;
        }

        static void DrawSideV(byte[,] field, int x, int y)
        {
            if (x >= 0 && x <= 9 && y >= 0 && y <= 9)
                field[x, y] = 1;
            if (x - 1 >= 0)
                field[x - 1, y] = 2;
            if (x + 1 < 10)
                field[x + 1, y] = 2;
        }

        static void DrawWaterH(byte[,] field, int x, int y)
        {
            if (x >= 0 && x <= 9 && y >= 0 && y <= 9)
                field[x, y] = 2;
            if (x >= 0 && x <= 9 && y - 1 >= 0)
                field[x, y - 1] = 2;
            if (x >= 0 && x <= 9 && y + 1 < 10)
                field[x, y + 1] = 2;
        }

        static void DrawWaterV(byte[,] field, int x, int y)
        {
            if (x >= 0 && x <= 9 && y >= 0 && y <= 9)
                field[x, y] = 2;
            if (x - 1 >= 0 && x - 1 <= 9 && y >= 0)
                field[x - 1, y] = 2;
            if (x + 1 >= 0 && x + 1 <= 9 && y < 10)
                field[x + 1, y] = 2;
        }

        static bool CheckSideH(byte[,] field, int x, int y)
        {
            if (x < 0 || x > 9 || y < 0 || y > 9)
                return false;

            if (field[x, y] != 0)
                return false;

            if (y - 1 >= 0 && field[x, y - 1] == 1)
                return false;

            if (y + 1 < 10 && field[x, y + 1] == 1)
                return false;

            return true;
        }

        static bool CheckSideV(byte[,] field, int x, int y)
        {
            if (x < 0 || x > 9 || y < 0 || y > 9)
                return false;

            if (field[x, y] != 0)
                return false;

            if (x - 1 >= 0 && field[x - 1, y] == 1)
                return false;

            if (x + 1 < 10 && field[x + 1, y] == 1)
                return false;

            return true;
        }

        public static byte[] GetOneDimensionField(byte[,] field)
        {
            var field1_1d = new byte[100];
            for (int i = 0, t = 0; i < 10; i++)
                for (int j = 0; j < 10; j++, t++)
                    field1_1d[t] = field[i, j];
            return field1_1d;
        }
    }
}
