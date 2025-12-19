using Microsoft.EntityFrameworkCore;
using SeaBattleDB.DB;
using SeaBattleRepository.Interfaces;
using System.Linq.Expressions;

namespace SeaBattleRepository.Implement
{
    public abstract class RepositoryBase<T,V> :
        IRepository<T,V> where T : class where V : class
    {
        protected readonly User29Context context;
        protected Func<T, V> toDTO;

        public RepositoryBase(User29Context context, Func<T, V> toDTO)
        {
            this.context = context;
            this.toDTO = toDTO;

        }
        
        public abstract Task<int> CreateAsync(V entity);
        public abstract Task UpdateAsync(V entity);

        public async Task DeleteAsync(int id)
        {
            var delete = await context.Set<T>().FindAsync(id);
            if (delete == null)
            {
                await Task.CompletedTask;
                return;
            }
            context.Set<T>().Remove(delete);
        }

        public IEnumerable<V> GetByCondition(Func<T, bool> condition)
        {
            var array = context.Set<T>().AsNoTracking()
                .Where(condition).ToList();
            return array.Select(s => toDTO(s));
        }

        public async Task<V> GetByIdAsync(int id)
        {
            var entity = await context.Set<T>().FindAsync(id);
            return toDTO(entity);
        }

        public async Task SaveAsync()
        { 
            await context.SaveChangesAsync();
        }

        public async Task<V> SearchEntryByConditionAsync(Expression<Func<T, bool>> condition)
        {
            var entry = await context.Set<T>().AsNoTracking()
                 .FirstOrDefaultAsync(condition);
            return toDTO(entry);
        }
    }
}
