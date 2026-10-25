using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ICT.PaymentGateway.IntegrationTests;

/// <summary>Boots the API against the supplied PostgreSQL connection string. Development environment applies migrations on start.</summary>
internal sealed class PaymentGatewayFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:PaymentGateway", connectionString);
    }
}
