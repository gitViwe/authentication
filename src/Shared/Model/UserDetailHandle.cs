namespace Shared.Model;

public class UserDetailHandle
{
    public int Id { get; set; }
    public string UserHandle { get; set; } = string.Empty;
    public UserDetail UserDetail { get; set; } = new();
    public int UserId { get; set; }
}
