using SeaBattleRepository.DTO;
using SeaBattleRepository.Interfaces;
using System.Linq.Expressions;
using System.Text.Json;

namespace SeaBattleRepository.Implement
{
    public abstract class JsonRepositoryBase<T, V> : IRepository<T, V>
        where T : class
        where V : class
    {
        protected abstract string FilePath { get; }
        protected List<T> _items = new List<T>();
        protected readonly object _lock = new object();

        protected readonly Func<T, V> _toDto;
        protected readonly Func<V, T> _fromDto;

        protected JsonRepositoryBase(Func<T, V> toDto, Func<V, T> fromDto)
        {
            _toDto = toDto ?? throw new ArgumentNullException(nameof(toDto));
            _fromDto = fromDto ?? throw new ArgumentNullException(nameof(fromDto));
            LoadData();
        }

        protected virtual void LoadData()
        {
            lock (_lock)
            {
                if (File.Exists(FilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(FilePath);
                        _items = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
                    }
                    catch
                    {
                        _items = new List<T>();
                    }
                }
                else
                {
                    _items = new List<T>();
                    SaveData();
                }
            }
        }

        protected virtual void SaveData()
        {
            lock (_lock)
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory) && directory != null)
                    Directory.CreateDirectory(directory);

                var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(FilePath, json);
            }
        }

        public virtual async Task<V> GetByIdAsync(int id)
        {
            var item = _items.FirstOrDefault(i => GetId(i) == id);
            return item != null ? _toDto(item) : null;
        }

        public virtual async Task<int> CreateAsync(V entityDto)
        {
            lock (_lock)
            {
                var entity = _fromDto(entityDto);
                var newId = _items.Any() ? _items.Max(i => GetId(i)) + 1 : 1;
                SetId(entity, newId);
                _items.Add(entity);
                SaveData();
                return newId;
            }
        }

        public virtual async Task UpdateAsync(V entityDto)
        {
            lock (_lock)
            {
                var entity = _fromDto(entityDto);
                var id = GetId(entity);
                var existing = _items.FirstOrDefault(i => GetId(i) == id);
                if (existing != null)
                {
                    var index = _items.IndexOf(existing);
                    _items[index] = entity;
                    SaveData();
                }
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            lock (_lock)
            {
                var item = _items.FirstOrDefault(i => GetId(i) == id);
                if (item != null)
                {
                    _items.Remove(item);
                    SaveData();
                }
            }
        }

        public virtual async Task<V> SearchEntryByConditionAsync(Expression<Func<T, bool>> condition)
        {
            var compiledCondition = condition.Compile();
            var item = _items.FirstOrDefault(compiledCondition);
            return item != null ? _toDto(item) : null;
        }

        public virtual IEnumerable<V> GetByCondition(Func<T, bool> condition)
        {
            return _items.Where(condition).Select(_toDto);
        }

        public virtual async Task SaveAsync()
        {
            await Task.CompletedTask;
        }

        protected abstract int GetId(T entity);
        protected abstract void SetId(T entity, int id);
    }
}