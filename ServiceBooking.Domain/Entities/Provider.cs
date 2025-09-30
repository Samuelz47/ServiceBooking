using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Entities;
public class Provider
{
    public Provider()
    {
        Services = new Collection<ServiceOffering>(); 
    }
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(300)]
    public string? Description { get; set; }
    public string? LogoUrl{ get; set; }
    public ICollection<ServiceOffering>? Services { get; set; }
}
