using API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace API.Endpoint;

public static class UserDetailEndpoint
{
    public static IEndpointRouteBuilder MapUserDetailEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{userHandleBase64}", GetUserDetailAsync)
            .WithName("User Detail")
            .Produces<UserDetail>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapPut("/kanban", UpdateKanBanAsync)
            .WithName("Update Kanban")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetUserDetailAsync(
        [FromRoute] string userHandleBase64,
        [FromServices] WebAuthenticationDbContext dbContext,
        CancellationToken cancellation = default)
    {
        var userHandle = await dbContext.UserDetailHandles
            .AsNoTracking()
            .FirstOrDefaultAsync(handle => handle.UserHandle.Equals(userHandleBase64.Replace("\"", string.Empty)), cancellationToken: cancellation);

        if (userHandle is null)
        {
            return Results.NotFound();
        }

        var user = await dbContext.UserDetails
            .AsNoTracking()
            .Include(user => user.KanBanSections)
                .ThenInclude(section => section.KanBanTaskItems)
            .FirstOrDefaultAsync(user => user.Id == userHandle.UserId, cancellationToken: cancellation);

        return user is null
            ? Results.NotFound()
            : Results.Ok(UserDetailDTO.ToDTO(user));
    }

    private static async Task<IResult> UpdateKanBanAsync(
        [FromBody] KanBanDialogData data,
        [FromServices] WebAuthenticationDbContext dbContext,
        CancellationToken cancellation = default)
    {
        await dbContext.KanBanSections
            .Where(section => section.UserId == data.Id)
            .ExecuteDeleteAsync(cancellation);

        var user = await dbContext.UserDetails
            .Include(user => user.KanBanSections)
                .ThenInclude(section => section.KanBanTaskItems)
            .FirstOrDefaultAsync(user => user.Id == data.Id, cancellationToken: cancellation);

        foreach (var section in data.KanBanSections.Select(section => new KanBanSection()
        {
            Name = section.Name,
            NewTaskName = section.NewTaskName,
            NewTaskOpen = section.NewTaskOpen,
            KanBanTaskItems = data.KanBanTaskItems.Where(x => x.Status == section.Name).Select(x => new KanBanTaskItem
            {
                Name = x.Name,
                Status = x.Status,
            }).ToList()
        }))
        {
            user.KanBanSections.Add(section);
        }

        await dbContext.SaveChangesAsync(cancellation);

        return Results.Ok();
    }
}
