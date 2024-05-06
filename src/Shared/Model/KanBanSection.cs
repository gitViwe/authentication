namespace Shared.Model;

public class KanBanSection
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public bool NewTaskOpen { get; set; }
    public string NewTaskName { get; set; } = string.Empty;
    public UserDetail UserDetail { get; set; } = new();
    public int UserId { get; set; }
    public ICollection<KanBanTaskItem> KanBanTaskItems { get; set; } = [];
}

public class KanBanSectionDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool NewTaskOpen { get; set; }
    public string NewTaskName { get; set; } = string.Empty;
}
