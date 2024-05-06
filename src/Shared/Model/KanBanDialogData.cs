namespace Shared.Model;

public class KanBanDialogData
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public IEnumerable<KanBanSectionDTO> KanBanSections { get; set; } = [];
    public IEnumerable<KanBanTaskItemDTO> KanBanTaskItems { get; set; } = [];
}
