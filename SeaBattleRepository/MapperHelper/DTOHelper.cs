using SeaBattleDB.DB;
using SeaBattleRepository.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleRepository.MapperHelper
{
    public static class DTOHelper
    {
        public static GameDTO ToDTO(this Game game)
        {
            if (game == null)
                return new GameDTO();
            var creator = game.IdUsers.
                FirstOrDefault(s => s.Id == game.CreatorUserId);
            var opponent = game.IdUsers.
                FirstOrDefault(s => s != creator);
            return new GameDTO
            {
                Id = game.Id,
                Creator = creator.ToDTO(),
                Opponent = opponent?.ToDTO(),
                DatetimeLastTurn = game.DatetimeLastTurn,
                DatetimeStartGame = game.DatetimeStartGame,
                FieldUser1 = game.FieldUser1,
                FieldUser2 = game.FieldUser2,
                IdUserNextTurn = game.IdUserNextTurn,
                IdUserWinner = game.IdUserWinner,
                Status = game.Status
            };
        }

        public static UserDTO ToDTO(this User user)
        {
            if (user == null)
                return new UserDTO();
            return new UserDTO
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Rating = user.Rating,
            };
        }
    }
}
