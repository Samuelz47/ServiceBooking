using ServiceBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Domain.Entities;
public class Booking
{
    public Booking(ServiceOffering service, Provider provider, User user)
    {
        Service = service;
        Provider = provider;
        User = user;
        Status = BookingStatus.Pending;
    }

    public int Id { get; set; }
    [Required]
    public ServiceOffering Service { get; set; }
    [Required]
    public int ServiceId { get; set; }
    [Required]
    public Provider Provider { get; set; }
    [Required]
    public int ProviderId { get; set; }
    [Required]
    public User User { get; set; }
    [Required]
    public int UserId { get; set; }
    public BookingStatus Status { get; set; }
    [Required]
    public DateTime InitialDate { get; set; }
    [Required]
    public DateTime FinalDate { get; set; }
}
