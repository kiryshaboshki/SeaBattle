using SeaBattleRepository.Models;
using SeaBattleRepository.DTO;
using System.Linq.Expressions;

namespace SeaBattleRepository.Implement
{
    public class RepositoryUser : JsonRepositoryBase<User, UserDTO>
    {
        public RepositoryGame GameRepository { get; set; }

        public RepositoryUser(RepositoryGame gameRepository = null) 
            : base(
                toDto: UserToDto,
                fromDto: DtoToUser
            )
        {
            GameRepository = gameRepository;
        }

        protected override string FilePath => "Data/users.json";

        protected override int GetId(User entity) => entity.Id;
        protected override void SetId(User entity, int id) => entity.Id = id;

        private static UserDTO UserToDto(User user)
        {
            if (user == null) return new UserDTO();
            
            return new UserDTO
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Rating = user.Rating
            };
        }

        private static User DtoToUser(UserDTO dto)
        {
            if (dto == null) return null;
            
            return new User
            {
                Id = dto.Id,
                Login = dto.Login,
                Password = dto.Password,
                Rating = dto.Rating,
                GameIds = new List<int>()
            };
        }

        public async Task<User> GetUserEntityByLoginAsync(string login)
        {
            return _items.FirstOrDefault(u => u.Login == login);
        }

        public async Task AddGameToUserAsync(int userId, int gameId)
        {
            lock (_lock)
            {
                var user = _items.FirstOrDefault(u => u.Id == userId);
                if (user != null && !user.GameIds.Contains(gameId))
                {
                    user.GameIds.Add(gameId);
                    SaveData();
                }
            }
        }

        public async Task<List<int>> GetUserGameIdsAsync(int userId)
        {
            var user = _items.FirstOrDefault(u => u.Id == userId);
            return user?.GameIds ?? new List<int>();
        }
    }
}