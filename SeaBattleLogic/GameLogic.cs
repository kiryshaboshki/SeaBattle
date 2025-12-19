using SeaBattleRepository.DTO;
using SeaBattleRepository.Implement;

namespace SeaBattleLogic
{
    public class GameLogic
    {
        readonly RepositoryGame repositoryGame;
        readonly RepositoryUser repositoryUser;

        public GameLogic(RepositoryGame repositoryGame, RepositoryUser repositoryUser)
        {
            this.repositoryGame = repositoryGame;
            this.repositoryUser = repositoryUser;
        }

        public async Task<GameDTO> CreateGameAsync(int idUser)
        {
            GameDTO gameDTO = new GameDTO
            {
                Creator = await repositoryUser.SearchEntryByConditionAsync(s => s.Id == idUser),
                IdUserNextTurn = idUser,
                FieldUser1 = new byte[1],
                FieldUser2 = new byte[1],
            };
            gameDTO.Id = await repositoryGame.CreateAsync(gameDTO);
            return gameDTO;
        }

        public List<GameDTO> ListFreeGame(int opponentId)
        {
            return repositoryGame.GetByCondition(s => s.Status == 0 && s.CreatorUserId != opponentId).ToList();
        }

        public async Task<bool> JoinGameAsync(int opponentId, int idGame)
        {
            var game = await repositoryGame.SearchEntryByConditionAsync(s => s.Id == idGame && s.CreatorUserId != opponentId);
            if (game.Id == 0)
                throw new Exception($"not found game by id {idGame}");
            var userOpponent = await repositoryUser.SearchEntryByConditionAsync(s => s.Id == opponentId);
            if (userOpponent.Id == 0)
                throw new Exception($"not found opponent by id {opponentId}");
            game.Opponent = userOpponent;
            var field1 = FieldGeneration.Execute();
            game.FieldUser1 = FieldGeneration.GetOneDimensionField(field1);
            var field2 = FieldGeneration.Execute();
            game.FieldUser2 = FieldGeneration.GetOneDimensionField(field2);
            game.Status = 1;
            game.IdUserNextTurn = game.Creator.Id;
            await repositoryGame.UpdateAsync(game);
            await repositoryGame.SaveAsync();
            return true;
        }

        public async Task<GameTurn> CheckTurnAsync(int userId, int idGame)
        {
            var game = await repositoryGame.SearchEntryByConditionAsync(s => s.Id == idGame);
            if (game.Id == 0)
                throw new Exception($"not found game by id {idGame}");
            var userOpponent = await repositoryUser.SearchEntryByConditionAsync(s => s.Id == userId);
            if (userOpponent.Id == 0)
                throw new Exception($"not found opponent by id {userId}");

            if (game.IdUserNextTurn == userId)
            {
                return new GameTurn
                {
                    FieldUser = game.Creator.Id == userId
                        ? game.FieldUser1 : game.FieldUser2,
                    IdUserNextTurn = userId
                };
            }
            else
            {
                return new GameTurn
                {
                    IdUserNextTurn = game.IdUserNextTurn,
                    IdWinner = game.IdUserWinner ?? 0
                };
            }
        }

        public async Task<TurnResult> MakeTurnAsync(int userId, int idGame, int x, int y)
        {
            TurnResult turnResult = TurnResult.Lose;
            var game = await repositoryGame.SearchEntryByConditionAsync(s => s.Id == idGame);
            if (game.Id == 0)
                throw new Exception($"not found game by id {idGame}");
            var user = await repositoryUser.SearchEntryByConditionAsync(s => s.Id == userId);
            if (user.Id == 0)
                throw new Exception($"not found user by id {userId}");

            if (game.IdUserNextTurn != userId)
                throw new Exception($"next turn is not for user id {userId}");

            var fieldTarget = game.Creator.Id == userId ? game.FieldUser2 : game.FieldUser1;
            var targetCell = x + 10 * y;
            if (fieldTarget[targetCell] == 1)
            {
                turnResult = TurnResult.Hit;
                fieldTarget[targetCell] = 2;
                if (fieldTarget.Count(s => s == 1) == 0)
                {
                    turnResult = TurnResult.Winner;
                    game.IdUserWinner = userId;
                    game.Status = 2;
                }
                game.IdUserNextTurn = userId;
            }
            else
            {
                fieldTarget[targetCell] = 3;
                game.IdUserNextTurn = game.Creator.Id == userId ? game.Opponent.Id : game.Creator.Id;
            }
            game.DatetimeLastTurn = DateTime.Now;
            await repositoryGame.UpdateAsync(game);
            await repositoryGame.SaveAsync();

            return turnResult;
        }
    }
}