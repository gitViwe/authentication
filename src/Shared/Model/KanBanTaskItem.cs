namespace Shared.Model;

public class KanBanTaskItem
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public KanBanSection KanBanSection { get; set; } = new();
}

public class KanBanTaskItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
