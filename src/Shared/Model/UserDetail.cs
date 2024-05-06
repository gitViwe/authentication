namespace Shared.Model;

public class UserDetail
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ICollection<KanBanSection> KanBanSections { get; set; } = [];
    public ICollection<UserDetailHandle> UserDetailHandles { get; set; } = [];
}

public class UserDetailDTO
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public IEnumerable<KanBanSectionDTO> KanBanSections { get; set; } = [];
    public IEnumerable<KanBanTaskItemDTO> KanBanTaskItems { get; set; } = [];

    public static UserDetailDTO ToDTO(UserDetail detail)
    {
        return new UserDetailDTO()
        {
            Id = detail.Id,
            UserName = detail.UserName,
            KanBanSections = detail.KanBanSections.Select(x => new KanBanSectionDTO()
            {
                Id = x.Id,
                Name = x.Name,
                NewTaskName = x.NewTaskName,
                NewTaskOpen = x.NewTaskOpen,
            }).ToList(),
            KanBanTaskItems = detail.KanBanSections.SelectMany(i => i.KanBanTaskItems).Select(i => new KanBanTaskItemDTO
            {
                Id = i.Id,
                Name = i.Name,
                Status = i.Status,
            }).ToList()
        };
    }
}
