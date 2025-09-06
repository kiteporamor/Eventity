using System.ComponentModel.DataAnnotations;
using Eventity.Domain.Enums;

namespace Eventity.Domain.Models;

public class User
{
    public User() { }
    
    public User(Guid id, string name, string email, string login, string password, UserRoleEnum role)
    {
        Id = id;
        Name = name;
        Email = email;
        Login = login;
        Password = password;
        Role = role;
    }
    
    public Guid Id { get; set; }
    
    [Required] 
    public string Name { get; set; }
    
    [Required] 
    public string Email { get; set; }
    
    [Required] 
    public string Login { get; set; }
    
    [Required] 
    public string Password { get; set; }
    
    [Required] 
    [EnumDataType(typeof(UserRoleEnum))]
    public UserRoleEnum Role { get; set; }
}
