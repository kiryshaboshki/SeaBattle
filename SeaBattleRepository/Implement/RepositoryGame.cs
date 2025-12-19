using SeaBattleRepository.Models;
using System.Text.Json;

namespace SeaBattleRepository.Implement
{
    public class RepositoryGame
    {
        private readonly string _gamesFilePath = "Data/games.json";
        private List<Game> _games;
        private readonly object _lock = new object();

        public RepositoryGame()
        {
            LoadData();
        }

        private void LoadData()
        {
            lock (_lock)
            {
                if (File.Exists(_gamesFilePath))
                {
                    var json = File.ReadAllText(_gamesFilePath);
                    _games = JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
                }
                else
                {
                    _games = new List<Game>();
                    SaveGames();
                }
            }
        }

        public List<Game> GetByCondition(Func<Game, bool> predicate)
        {
            return _games.Where(predicate).ToList();
        }

        public async Task<Game> GetByIdAsync(int id)
        {
            return _games.FirstOrDefault(g => g.Id == id);
        }

        public async Task<Game> CreateAsync(Game game)
        {
            lock (_lock)
            {
                var newId = _games.Any() ? _games.Max(g => g.Id) + 1 : 1;
                game.Id = newId;
                game.CreatedAt = DateTime.UtcNow;
                _games.Add(game);
                SaveGames();
                return game;
            }
        }

        public async Task UpdateAsync(Game game)
        {
            lock (_lock)
            {
                var existing = _games.FirstOrDefault(g => g.Id == game.Id);
                if (existing != null)
                {
                    existing.Status = game.Status;
                    existing.IdUserWinner = game.IdUserWinner;
                    existing.UserIds = game.UserIds;
                    SaveGames();
                }
            }
        }

        public async Task AddUserToGameAsync(int gameId, int userId)
        {
            lock (_lock)
            {
                var game = _games.FirstOrDefault(g => g.Id == gameId);
                if (game != null && !game.UserIds.Contains(userId))
                {
                    game.UserIds.Add(userId);
                    SaveGames();
                }
            }
        }

        public async Task<Game> SearchEntryByConditionAsync(Func<Game, bool> predicate)
        {
            return _games.FirstOrDefault(predicate) ?? new Game();
        }

        public async Task<List<Game>> GetAllAsync()
        {
            return _games;
        }

        public async Task<Game> GetGameWithUsersAsync(int gameId, RepositoryUser userRepo)
        {
            var game = _games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) return new Game();
            
            return game;
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }

        private void SaveGames()
        {
            var directory = Path.GetDirectoryName(_gamesFilePath);
            if (!Directory.Exists(directory) && directory != null)
                Directory.CreateDirectory(directory);
                
            var json = JsonSerializer.Serialize(_games, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_gamesFilePath, json);
        }
    }
}