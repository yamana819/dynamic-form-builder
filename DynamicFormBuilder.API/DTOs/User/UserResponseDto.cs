
namespace DynamicFormBuilder.API.DTOs.User;

public class UserResponseDto
{

    public Guid UserId { get; set;} 

    public string UserName { get; set; } = null!;

    public string RoleName {get;set;} = null!;

}

public class AdminUserResponseDto:UserResponseDto
{
    public byte RoleId { get; set; }

    public DateTime UserStartDate { get; set; }

    public DateTime? UserLastActiveDate { get; set; }    

    public bool IsDeleted { get; set; }
}