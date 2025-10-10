using ServiceBooking.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Repositories;
public interface IRepository<T>
{
    Task<PagedResult<T>> GetAllAsync(QueryParameters queryParameters);
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);                    //Dessa forma podemos buscar por nome, id e etc. podendo aceitar como parametro uma funçao lambda
    T Create(T entity);
    T Update(T entity);
    T Delete(T entity);
}
