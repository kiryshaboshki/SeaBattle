using SeaBattle.Models;
using System;
using System.Threading.Tasks;
using SeaBattle;

namespace SeaBattle.API
{
    public static class GameAPI
    {
        // Отправка расстановки кораблей на сервер
        public static async Task<ApiResult> SendFieldPlacement(byte[] fieldData)
        {
            try
            {
                // Реальный вызов к серверу
                var result = await Client.Instance.PostAsync("Game/PlaceShips", new
                {
                    Field = fieldData,
                    GameId = CurrentGame.Id // Предполагаем, что есть текущая игра
                });

                return new ApiResult
                {
                    Success = result.Success,
                    Message = result.Response,
                    Data = result.Response
                };
            }
            catch (Exception ex)
            {
                return new ApiResult
                {
                    Success = false,
                    Message = $"Ошибка сети: {ex.Message}"
                };
            }
        }

        // Отправка выстрела
        public static async Task<ShotResult> SendShot(int x, int y)
        {
            try
            {
                var result = await Client.Instance.PostAsync("Game/Shoot", new
                {
                    GameId = CurrentGame.Id,
                    X = x,
                    Y = y
                });

                // Парсим ответ сервера
                // В реальном приложении: JsonSerializer.Deserialize<ShotResult>(result.Response)

                return new ShotResult
                {
                    Success = result.Success,
                    IsHit = result.Success && result.Response.Contains("hit"),
                    IsDestroyed = result.Success && result.Response.Contains("destroyed"),
                    ShipSize = result.Success ? 3 : 0, // Пример
                    Message = result.Response
                };
            }
            catch (Exception ex)
            {
                return new ShotResult
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        // Получение хода противника
        public static async Task<EnemyTurnResult> GetEnemyTurn()
        {
            try
            {
                var result = await Client.Instance.PostAsync($"Game/GetTurn/{CurrentGame.Id}");

                // В реальном приложении парсим ответ
                return new EnemyTurnResult
                {
                    Success = result.Success,
                    X = 0, // Из ответа сервера
                    Y = 0,
                    IsHit = false,
                    Message = result.Response
                };
            }
            catch (Exception ex)
            {
                return new EnemyTurnResult
                {
                    Success = false,
                    Message = $"Ошибка: {ex.Message}"
                };
            }
        }

        // Получение обновления состояния игры
        public static async Task<GameState> GetGameState()
        {
            try
            {
                var result = await Client.Instance.PostAsync($"Game/State/{CurrentGame.Id}");

                // В реальном приложении: десериализация
                return new GameState
                {
                    IsMyTurn = true,
                    MyShipsAlive = 10,
                    EnemyShipsAlive = 10,
                    GameStatus = "InProgress"
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Поллинг для получения обновлений
        public static async Task<bool> CheckForUpdates()
        {
            try
            {
                var result = await Client.Instance.PostAsync($"Game/Updates/{CurrentGame.Id}");
                return result.Success && result.Response.Contains("update");
            }
            catch
            {
                return false;
            }
        }
    }

    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }

    public class ShotResult : ApiResult
    {
        public bool IsHit { get; set; }
        public bool IsDestroyed { get; set; }
        public int ShipSize { get; set; }
    }

    public class EnemyTurnResult : ApiResult
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsHit { get; set; }
    }

    public class GameState
    {
        public bool IsMyTurn { get; set; }
        public int MyShipsAlive { get; set; }
        public int EnemyShipsAlive { get; set; }
        public string GameStatus { get; set; }
    }

    // Статический класс для хранения текущей игры (временное решение)
    //public static class CurrentGame
    //{
    //    public static int Id { get; set; } = 1;
    //    public static bool IsOnline { get; set; } = false;
    //    public static string OpponentName { get; set; } = "Противник";
    //}
}