using SeaBattleRepository.Models;
using SeaBattleRepository.DTO;
using System.Linq.Expressions;

namespace SeaBattleRepository.Implement
{
    public class RepositoryGame : JsonRepositoryBase<Game, GameDTO>
    {
        public RepositoryGame() 
            : base(
                toDto: GameToDto,
                fromDto: DtoToGame
            )
        {
        }

        protected override string FilePath => "Data/games.json";

        protected override int GetId(Game entity) => entity.Id;
        protected override void SetId(Game entity, int id) => entity.Id = id;

        private static GameDTO GameToDto(Game game)
        {
            if (game == null) return null;
            
            return new GameDTO
            {
                Id = game.Id,
                Status = game.Status,
                IdUserWinner = game.IdUserWinner,
                CreatorUserId = game.CreatorUserId,
                OpponentUserId = game.OpponentUserId,
                FieldUser1 = game.FieldUser1,
                FieldUser2 = game.FieldUser2,
                IdUserNextTurn = game.IdUserNextTurn,
                DatetimeLastTurn = game.DatetimeLastTurn
            };
        }

        private static Game DtoToGame(GameDTO dto)
        {
            if (dto == null) return null;
            
            return new Game
            {
                Id = dto.Id,
                Status = dto.Status,
                IdUserWinner = dto.IdUserWinner,
                CreatorUserId = dto.CreatorUserId,
                OpponentUserId = dto.OpponentUserId,
                FieldUser1 = dto.FieldUser1 ?? new byte[100],
                FieldUser2 = dto.FieldUser2 ?? new byte[100],
                IdUserNextTurn = dto.IdUserNextTurn,
                DatetimeLastTurn = dto.DatetimeLastTurn,
                UserIds = new List<int> { dto.CreatorUserId }
            };
        }

        public async Task<Game> GetGameEntityByIdAsync(int id)
        {
            return _items.FirstOrDefault(g => g.Id == id);
        }

        public async Task AddUserToGameAsync(int gameId, int userId)
        {
            lock (_lock)
            {
                var game = _items.FirstOrDefault(g => g.Id == gameId);
                if (game != null && !game.UserIds.Contains(userId))
                {
                    game.UserIds.Add(userId);
                    SaveData();
                }
            }
        }
    }
}