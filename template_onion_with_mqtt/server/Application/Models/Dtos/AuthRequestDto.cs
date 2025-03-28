using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos;

public class AuthRequestDto
{
    [MinLength(3)] [Required] public string Email { get; set; } = null!;
    [MinLength(4)] [Required] public string Password { get; set; } = null!;
}