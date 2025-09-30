using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Entities;
public class ServiceOffering
{
    public ServiceOffering()
    {
        Providers = new Collection<Provider>();
    }
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(300)]
    public string? Description { get; set; }
    public ICollection<Provider>? Providers { get; set; }
}
