using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Repositories;
public interface IServiceOfferingRepository : IRepository<ServiceOffering>
{
    Task<ServiceOffering?> GetByIdWithDetailsAsync(int id);
    Task<List<ServiceOffering>> GetByIdsAsync(IEnumerable<int> ids);
}
