using ServiceBooking.Domain.Entities;
using ServiceBooking.Domain.Repositories;
using ServiceBooking.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Infrastructure.Repositories;
public class ServiceOfferingRepository : Repository<ServiceOffering>, IServiceOfferingRepository
{
    public ServiceOfferingRepository(AppDbContext context) : base(context)
    {
    }
}
