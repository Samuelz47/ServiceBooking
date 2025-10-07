using Microsoft.EntityFrameworkCore;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
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

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();                  //o Set serve para pegar uma coleção ou tabela do tipo T(no caso a classe)
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
