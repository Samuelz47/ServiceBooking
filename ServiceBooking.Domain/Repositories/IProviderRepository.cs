using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Repositories;
public interface IProviderRepository : IRepository<Provider>
{
    Task<Provider?> GetByIdWithDetailsAsync(int id);
    Task<List<Provider>> GetByIdsAsync(IEnumerable<int> ids);
}
