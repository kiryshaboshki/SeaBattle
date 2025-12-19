using SeaBattleRepository.DTO;
using SeaBattleRepository.Models;
using System.Text.Json;

namespace SeaBattleRepository.Implement
{
    public class RepositoryUser
    {
        private readonly string _usersFilePath = "Data/users.json";
        private List<User> _users;
        private readonly object _lock = new object();

        public RepositoryUser()
        {
            LoadData();
        }

        private void LoadData()
        {
            lock (_lock)
            {
                if (File.Exists(_usersFilePath))
                {
                    var json = File.ReadAllText(_usersFilePath);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
                else
                {
                    _users = new List<User>();
                    SaveUsers();
                }
            }
        }

        public async Task<UserDTO> SearchEntryByConditionAsync(Func<UserDTO, bool> predicate)
        {
            var userDTOs = _users.Select(u => new UserDTO
            {
                Id = u.Id,
                Login = u.Login,
                Password = u.Password,
                Rating = u.Rating
            }).ToList();

            return userDTOs.FirstOrDefault(predicate) ?? new UserDTO();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<User> GetUserByLoginAsync(string login)
        {
            return _users.FirstOrDefault(u => u.Login == login);
        }

        public async Task CreateAsync(UserDTO userDTO)
        {
            lock (_lock)
            {
                var newId = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
                
                var user = new User
                {
                    Id = newId,
                    Login = userDTO.Login,
                    Password = userDTO.Password,
                    Rating = userDTO.Rating
                };

                _users.Add(user);
                SaveUsers();
            }
        }

        public async Task UpdateAsync(User user)
        {
            lock (_lock)
            {
                var existing = _users.FirstOrDefault(u => u.Id == user.Id);
                if (existing != null)
                {
                    existing.Login = user.Login;
                    existing.Password = user.Password;
                    existing.Rating = user.Rating;
                    existing.GameIds = user.GameIds;
                    SaveUsers();
                }
            }
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }

        public async Task AddGameToUserAsync(int userId, int gameId)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user != null && !user.GameIds.Contains(gameId))
                {
                    user.GameIds.Add(gameId);
                    SaveUsers();
                }
            }
        }

        public async Task<List<int>> GetUserGameIdsAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            return user?.GameIds ?? new List<int>();
        }

        private void SaveUsers()
        {
            var directory = Path.GetDirectoryName(_usersFilePath);
            if (!Directory.Exists(directory) && directory != null)
                Directory.CreateDirectory(directory);
                
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_usersFilePath, json);
        }
    }
}