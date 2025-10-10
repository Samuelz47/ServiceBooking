using Microsoft.EntityFrameworkCore;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Infrastructure.Repositories;
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<T>> GetAllAsync(QueryParameters queryParameters)
    {
        var totalCount = await _context.Set<T>().CountAsync();      // Verificando o numero de total de itens

        var items = await _context.Set<T>()
                                  .AsNoTracking()
                                  .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)    //Pula os itens das paginas anteriores
                                  .Take(queryParameters.PageSize)                                       //Pega a quantidade de itens definida por pagina
                                  .ToListAsync();

        return new PagedResult<T>(items, queryParameters.PageNumber, queryParameters.PageSize, totalCount);
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public T Create(T entity)
    {
        _context.Set<T>().Add(entity);

        return entity;
    }

    public T Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;

        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);

        return entity;
    }
}
