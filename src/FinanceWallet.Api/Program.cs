using FinanceWallet.Api.Extensions;
using FinanceWallet.Api.Filters;
using FinanceWallet.Modules.Identity.Infrastructure;
using FinanceWallet.Modules.Identity.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options => options.Filters.Add<ApiExceptionFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = false;
    });

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiAuthorization();

var app = builder.Build();

// TODO(Phase 12): replace with EF migrations.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();