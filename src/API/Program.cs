using API.Endpoint;
using API.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .RegisterCorsPolicy()
    .RegisterRedisCache(builder.Configuration)
    .RegisterDbContext(builder.Configuration)
    .RegisterWebAuthnSqlServer(configureSqlServer: sqlServer =>
    {
        sqlServer.ConnectionString = builder.Configuration.GetConnectionString("MSSQL")!;
    })
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    //FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    ServeUnknownFileTypes = true,
});
app.CreateDatabaseTable();

app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapRegistrationEndpoint();
app.MapAuthenticationEndpoint();
app.MapUserDetailEndpoint();

app.Run();
