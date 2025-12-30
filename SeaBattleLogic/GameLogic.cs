using SeaBattleRepository.Models;
using SeaBattleRepository.Implement;
using SeaBattleRepository.DTO;

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

        public async Task<GameDTO> CreateGameAsync(int userId)
        {
            var gameDto = new GameDTO
            {
                CreatorUserId = userId,
                IdUserNextTurn = userId,
                Status = 0,
                FieldUser1 = new byte[100],
                FieldUser2 = new byte[100],
                UserIds = new List<int> { userId }
            };

            var id = await _repositoryGame.CreateAsync(gameDto);
            gameDto.Id = id;
            return gameDto;
        }

        public List<GameDTO> ListFreeGame(int opponentId)
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
                
            var opponent = await _repositoryUser.GetByIdAsync(opponentId);
            if (opponent == null || opponent.Id == 0)
                throw new Exception($"Opponent not found: {opponentId}");

            game.OpponentUserId = opponentId;
            game.UserIds.Add(opponentId);
            
            var field2D_1 = FieldGeneration.Execute();
            var field2D_2 = FieldGeneration.Execute();
            
            game.FieldUser1 = FieldGeneration.GetOneDimensionField(field2D_1);
            game.FieldUser2 = FieldGeneration.GetOneDimensionField(field2D_2);
            
            game.Status = 1;
            game.IdUserNextTurn = game.CreatorUserId;
            
            await _repositoryGame.UpdateAsync(game);
            
            return true;
        }

        public async Task<GameTurn> CheckTurnAsync(int userId, int gameId)
        {
            var game = await _repositoryGame.SearchEntryByConditionAsync(s => s.Id == gameId);
            if (game.Id == 0)
                throw new Exception($"Game not found: {gameId}");
                
            var user = await _repositoryUser.GetByIdAsync(userId); // ← GetByIdAsync
            if (user == null || user.Id == 0)
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

            if (x < 0 || x >= 10 || y < 0 || y >= 10)
                throw new Exception($"Invalid coordinates: x={x}, y={y}");

            var targetField = game.CreatorUserId == userId ? game.FieldUser2 : game.FieldUser1;
            var targetCell = x + 10 * y;
            
            if (targetCell < 0 || targetCell >= 100)
                throw new Exception($"Invalid cell index: {targetCell}");

            if (targetField[targetCell] == 1)
            {
                targetField[targetCell] = 2;
                
                bool allShipsSunk = !targetField.Any(cell => cell == 1);
                
                if (allShipsSunk)
                {
                    game.IdUserWinner = userId;
                    game.Status = 2;
                    await _repositoryGame.UpdateAsync(game);
                    return TurnResult.Winner;
                }
                
                game.IdUserNextTurn = userId;
                game.DatetimeLastTurn = DateTime.Now;
                await _repositoryGame.UpdateAsync(game);
                return TurnResult.Hit;
            }
            else if (targetField[targetCell] == 0)
            {
                targetField[targetCell] = 3;
                game.IdUserNextTurn = game.CreatorUserId == userId ? 
                    (game.OpponentUserId ?? 0) : game.CreatorUserId;
                game.DatetimeLastTurn = DateTime.Now;
                await _repositoryGame.UpdateAsync(game);
                return TurnResult.Miss;
            }
            else
            {
                throw new Exception($"Cell already attacked: x={x}, y={y}");
            }
        }
    }
}