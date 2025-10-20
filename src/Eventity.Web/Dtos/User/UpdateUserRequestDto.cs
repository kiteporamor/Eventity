using System.ComponentModel.DataAnnotations;

namespace Eventity.Web.Dtos;

public class UpdateUserRequestDto
{
    public string? Name { get; set; }
    
    public string? Email { get; set; }
    
    [StringLength(20, MinimumLength = 3)]
    public string? Login { get; set; }
    
    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }
}