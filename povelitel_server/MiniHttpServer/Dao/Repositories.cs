using MiniHttpServer.Models;
using MiniHttpServer.ORM;

namespace MiniHttpServer.Dao;

public interface IRepository<T> where T : class, IEntity, new()
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public abstract class Repository<T> : IRepository<T> where T : class, IEntity, new()
{
    protected readonly IOrmContext Orm;
    protected readonly string Table;

    protected Repository(IOrmContext orm, string table)
    {
        Orm = orm ?? throw new ArgumentNullException(nameof(orm));
        Table = table;
    }

    public virtual Task<IReadOnlyList<T>> GetAllAsync() => Orm.GetAllAsync<T>(Table);

    public virtual Task<T?> GetByIdAsync(int id) => Orm.GetByIdAsync<T>(Table, id);

    public virtual Task<int> InsertAsync(T entity) => Orm.InsertAsync(Table, entity);

    public virtual Task UpdateAsync(T entity) => Orm.UpdateAsync(Table, entity.Id, entity);

    public virtual Task DeleteAsync(int id) => Orm.DeleteAsync(Table, id);
}

public interface ICardRepository : IRepository<Card>
{
    Task<IReadOnlyList<Card>> SearchAsync(string query);
}

public sealed class CardRepository : Repository<Card>, ICardRepository
{
    public CardRepository(IOrmContext orm) : base(orm, "cards") { }

    public async Task<IReadOnlyList<Card>> SearchAsync(string query)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(query))
            return all;
        return all.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public interface IMonsterRepository : IRepository<Monster>
{
    Task<IReadOnlyList<Monster>> SearchAsync(string query);
}

public sealed class MonsterRepository : Repository<Monster>, IMonsterRepository
{
    public MonsterRepository(IOrmContext orm) : base(orm, "monsters") { }

    public async Task<IReadOnlyList<Monster>> SearchAsync(string query)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(query))
            return all;
        return all.Where(m => m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public interface IItemRepository : IRepository<Item> { }

public sealed class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(IOrmContext orm) : base(orm, "items") { }
}

public interface IRuleRepository : IRepository<Rule>
{
    Task<Rule?> GetBySlugAsync(string slug);
}

public sealed class RuleRepository : Repository<Rule>, IRuleRepository
{
    public RuleRepository(IOrmContext orm) : base(orm, "rules") { }

    public async Task<Rule?> GetBySlugAsync(string slug)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        return all.FirstOrDefault(r => string.Equals(r.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }
}

public interface IStrategyRepository : IRepository<Strategy>
{
    Task<Strategy?> GetBySlugAsync(string slug);
}

public sealed class StrategyRepository : Repository<Strategy>, IStrategyRepository
{
    public StrategyRepository(IOrmContext orm) : base(orm, "strategies") { }

    public async Task<Strategy?> GetBySlugAsync(string slug)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        return all.FirstOrDefault(s => string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(IOrmContext orm) : base(orm, "users") { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var all = await GetAllAsync().ConfigureAwait(false);
        return all.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
