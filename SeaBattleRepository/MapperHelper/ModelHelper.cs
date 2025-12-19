using SeaBattleDB.DB;
using SeaBattleRepository.DTO;

namespace SeaBattleRepository.MapperHelper
{
    public static class ModelHelper
    {
        public static Game ToModel(this GameDTO game)
        {
            var gameModel = new Game
            {
                CreatorUserId = game.Creator.Id,
                Id = game.Id,
                DatetimeLastTurn = game.DatetimeLastTurn,
                DatetimeStartGame = game.DatetimeStartGame,
                FieldUser1 = game.FieldUser1,
                FieldUser2 = game.FieldUser2,
                IdUserNextTurn = game.IdUserNextTurn,
                IdUserWinner = game.IdUserWinner,
                Status = game.Status,
                IdUsers = new List<User>() {
                    game.Creator.ToModel()
                }
            };
            if (game.Opponent != null)
                gameModel.IdUsers.Add(game.Opponent.ToModel());
            return gameModel;
        }

        public static User ToModel(this UserDTO user)
        {
            return new User
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Rating = user.Rating
            };
        }
    }
}
