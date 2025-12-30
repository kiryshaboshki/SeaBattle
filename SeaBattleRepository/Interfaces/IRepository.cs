using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SeaBattleRepository.Interfaces
{
    public interface IRepository<T, V> where T : class where V : class
    {
        Task<V> GetByIdAsync(int id);
        Task<int> CreateAsync(V entity);
        Task UpdateAsync(V entity);
        Task DeleteAsync(int id);
        Task<V> SearchEntryByConditionAsync(Expression<Func<T, bool>> condition);
        IEnumerable<V> GetByCondition(Func<T, bool> condition);
        Task SaveAsync();
    }
}
