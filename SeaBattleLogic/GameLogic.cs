using SeaBattleRepository.Models;
using SeaBattleRepository.Implement;

namespace SeaBattleLogic
{
    public class GameLogic
    {
        readonly RepositoryGame _repositoryGame;
        readonly RepositoryUser _repositoryUser;

        public GameLogic(RepositoryGame repositoryGame, RepositoryUser repositoryUser)
        {
            _repositoryGame = repositoryGame;
            _repositoryUser = repositoryUser;
        }

        public async Task<Game> CreateGameAsync(int userId)
        {
            var game = new Game
            {
                CreatorUserId = userId,
                IdUserNextTurn = userId,
                Status = 0, // создана
                FieldUser1 = new byte[100], // пока пустое поле
                FieldUser2 = new byte[100],
                UserIds = new List<int> { userId }
            };
            
            return await _repositoryGame.CreateAsync(game);
        }

        public List<Game> ListFreeGame(int opponentId)
        {
            return _repositoryGame.GetByCondition(s => 
                s.Status == 0 && 
                s.CreatorUserId != opponentId &&
                (s.OpponentUserId == null || s.OpponentUserId == 0)
            ).ToList();
        }

        public async Task<bool> JoinGameAsync(int opponentId, int gameId)
        {
            var game = await _repositoryGame.SearchEntryByConditionAsync(s => 
                s.Id == gameId && 
                s.CreatorUserId != opponentId && 
                s.Status == 0);
                
            if (game.Id == 0)
                throw new Exception($"Game not found or not available: {gameId}");
                
            var opponent = await _repositoryUser.GetUserByIdAsync(opponentId);
            if (opponent == null)
                throw new Exception($"Opponent not found: {opponentId}");

            // Обновляем игру
            game.OpponentUserId = opponentId;
            game.UserIds.Add(opponentId);
            
            // Генерируем поля для обоих игроков
            var field2D_1 = FieldGeneration.Execute();
            var field2D_2 = FieldGeneration.Execute();
            
            game.FieldUser1 = FieldGeneration.GetOneDimensionField(field2D_1);
            game.FieldUser2 = FieldGeneration.GetOneDimensionField(field2D_2);
            
            game.Status = 1; // игра началась
            game.IdUserNextTurn = game.CreatorUserId;
            
            await _repositoryGame.UpdateAsync(game);
            await _repositoryUser.AddGameToUserAsync(opponentId, gameId);
            
            return true;
        }

        public async Task<GameTurn> CheckTurnAsync(int userId, int gameId)
        {
            var game = await _repositoryGame.SearchEntryByConditionAsync(s => s.Id == gameId);
            if (game.Id == 0)
                throw new Exception($"Game not found: {gameId}");
                
            var user = await _repositoryUser.GetUserByIdAsync(userId);
            if (user == null)
                throw new Exception($"User not found: {userId}");

            if (game.IdUserNextTurn == userId)
            {
                return new GameTurn
                {
                    IsMyTurn = true,
                    FieldUser = game.CreatorUserId == userId ? game.FieldUser1 : game.FieldUser2,
                    IdUserNextTurn = userId
                };
            }
            else
            {
                return new GameTurn
                {
                    IsMyTurn = false,
                    IdUserNextTurn = game.IdUserNextTurn,
                    IdWinner = game.IdUserWinner ?? 0
                };
            }
        }

        public async Task<TurnResult> MakeTurnAsync(int userId, int gameId, int x, int y)
        {
            var game = await _repositoryGame.SearchEntryByConditionAsync(s => s.Id == gameId);
            if (game.Id == 0)
                throw new Exception($"Game not found: {gameId}");
                
            if (game.IdUserNextTurn != userId)
                throw new Exception($"Not your turn. Next turn is for user: {game.IdUserNextTurn}");

            // Проверяем координаты
            if (x < 0 || x >= 10 || y < 0 || y >= 10)
                throw new Exception($"Invalid coordinates: x={x}, y={y} (must be 0-9)");

            // Определяем какое поле атакуем
            var targetField = game.CreatorUserId == userId ? game.FieldUser2 : game.FieldUser1;
            var targetCell = x + 10 * y; // поле 10x10
            
            if (targetCell < 0 || targetCell >= 100)
                throw new Exception($"Invalid cell index: {targetCell}");

            if (targetField[targetCell] == 1) // попадание в корабль
            {
                targetField[targetCell] = 2; // помечаем попадание
                
                // Проверяем победу (все корабли потоплены)
                bool allShipsSunk = !targetField.Any(cell => cell == 1);
                
                if (allShipsSunk)
                {
                    game.IdUserWinner = userId;
                    game.Status = 2; // игра завершена
                    await _repositoryGame.UpdateAsync(game);
                    
                    return TurnResult.Winner;
                }
                
                game.IdUserNextTurn = userId; // повторный ход при попадании
                game.DatetimeLastTurn = DateTime.Now;
                await _repositoryGame.UpdateAsync(game);
                
                return TurnResult.Hit;
            }
            else if (targetField[targetCell] == 0) // промах (пустая клетка)
            {
                targetField[targetCell] = 3; // помечаем промах
                // Передаём ход другому игроку
                game.IdUserNextTurn = game.CreatorUserId == userId ? 
                    (game.OpponentUserId ?? 0) : game.CreatorUserId;
                game.DatetimeLastTurn = DateTime.Now;
                await _repositoryGame.UpdateAsync(game);
                
                return TurnResult.Miss;
            }
            else // уже стреляли сюда
            {
                throw new Exception($"Cell already attacked: x={x}, y={y}");
            }
        }
    }
}