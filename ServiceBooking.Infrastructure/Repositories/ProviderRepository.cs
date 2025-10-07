using ServiceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;

namespace ServiceBooking.Infrastructure.Repositories;
public class ProviderRepository : Repository<Provider>, IProviderRepository
{
    public ProviderRepository(AppDbContext context) : base(context)
    {
    }
}
