using API.Endpoint;
using API.Extension;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.RegisterCorsPolicy();
builder.RegisterRedisCache();
builder.RegisterDbContext();
builder.RegisterWebAuthnSqlServer(configureSqlServer: sqlServer =>
{
    sqlServer.ConnectionString = builder.Configuration.GetConnectionString("MSSQL")!;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
