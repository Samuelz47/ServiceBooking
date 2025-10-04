using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBooking.Application.DTOs;
public class LoginDTO
{
    [Required(ErrorMessage = "Email obrigatório")]
    public string? Email { get; set; }
    [Required(ErrorMessage = "Senha obrigatória")]
    public string? Password { get; set; }
}
