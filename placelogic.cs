using System;
using System.Collections.Generic;
using System.Linq;

namespace lord13;

public class Ship
{
    public int Size { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsHorizontal { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }

    public static ValidationResult Success() => new ValidationResult { IsValid = true, Message = "OK" };
    public static ValidationResult Fail(string message) => new ValidationResult { IsValid = false, Message = message };
}

public static class ShipPlacementValidator
{
    public static ValidationResult ValidateShips(List<Ship> ships)
    {
        if (ships.Count != 10)
            return ValidationResult.Fail("Должно быть 10 кораблей");
        
        var expectedSizes = new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        var actualSizes = ships.Select(s => s.Size).OrderByDescending(s => s).ToArray();
        
        if (!expectedSizes.SequenceEqual(actualSizes))
            return ValidationResult.Fail("Неверное количество кораблей разных размеров");
        
        byte[,] field = new byte[10, 10];
        
        foreach (var ship in ships)
        {
            if (!CanPlaceShip(field, ship))
                return ValidationResult.Fail($"Корабль размера {ship.Size} не может быть размещён здесь");
            
            PlaceShipOnField(field, ship);
        }
        
        return ValidationResult.Success();
    }
    
    private static bool CanPlaceShip(byte[,] field, Ship ship)
    {
        if (ship.IsHorizontal)
        {
            for (int i = 0; i < ship.Size; i++)
            {
                int x = ship.X + i;
                int y = ship.Y;
                
                if (!CheckCell(field, x, y))
                    return false;
            }
        }
        else
        {
            for (int i = 0; i < ship.Size; i++)
            {
                int x = ship.X;
                int y = ship.Y + i;
                
                if (!CheckCell(field, x, y))
                    return false;
            }
        }
        
        return true;
    }
    
    private static bool CheckCell(byte[,] field, int x, int y)
    {
        if (x < 0 || x > 9 || y < 0 || y > 9)
            return false;
        
        if (field[x, y] != 0)
            return false;
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                {
                    if (field[nx, ny] == 1)
                        return false;
                }
            }
        }
        
        return true;
    }
    
    private static void PlaceShipOnField(byte[,] field, Ship ship)
    {
        if (ship.IsHorizontal)
        {
            for (int i = 0; i < ship.Size; i++)
            {
                int x = ship.X + i;
                int y = ship.Y;
                field[x, y] = 1;
                MarkWaterAround(field, x, y);
            }
        }
        else
        {
            for (int i = 0; i < ship.Size; i++)
            {
                int x = ship.X;
                int y = ship.Y + i;
                field[x, y] = 1;
                MarkWaterAround(field, x, y);
            }
        }
    }
    
    private static void MarkWaterAround(byte[,] field, int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                {
                    if (field[nx, ny] == 0)
                        field[nx, ny] = 2;
                }
            }
        }
    }
}