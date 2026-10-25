using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace ICT.PaymentGateway.IntegrationTests;

/// <summary>End-to-end tests against a real PostgreSQL container. Requires Docker.</summary>
public sealed class PaymentsApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:17").Build();
    private PaymentGatewayFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new PaymentGatewayFactory(_postgres.GetConnectionString());
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static HttpRequestMessage CreateRequest(string key, Guid merchantId, decimal amount = 12.50m, string token = "tok_visa")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(new { amount, currency = "USD", paymentMethodToken = token, description = "order" }),
        };
        request.Headers.Add("X-Merchant-Id", merchantId.ToString());
        if (key.Length > 0)
        {
            request.Headers.Add("Idempotency-Key", key);
        }

        return request;
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>());

    [Fact]
    public async Task Post_CreatesPayment_AndGetReturnsIt()
    {
        var merchant = Guid.NewGuid();

        var created = await _client.SendAsync(CreateRequest("key-a", merchant));
        var body = await ReadAsync(created);

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        body.GetProperty("status").GetString().ShouldBe("Authorized");

        var get = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/payments/{body.GetProperty("id").GetGuid()}");
        get.Headers.Add("X-Merchant-Id", merchant.ToString());
        (await _client.SendAsync(get)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_WithSameKeyAndBody_ReturnsOriginalPayment()
    {
        var merchant = Guid.NewGuid();

        var first = await ReadAsync(await _client.SendAsync(CreateRequest("key-b", merchant)));
        var second = await ReadAsync(await _client.SendAsync(CreateRequest("key-b", merchant)));

        second.GetProperty("id").GetGuid().ShouldBe(first.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Post_WithSameKeyAndDifferentBody_Returns422()
    {
        var merchant = Guid.NewGuid();
        await _client.SendAsync(CreateRequest("key-c", merchant, amount: 10m));

        var response = await _client.SendAsync(CreateRequest("key-c", merchant, amount: 99m));

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task Post_WithoutIdempotencyKey_Returns400() =>
        (await _client.SendAsync(CreateRequest(string.Empty, Guid.NewGuid()))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    [Fact]
    public async Task Post_WithCardNumberAsToken_Returns400() =>
        (await _client.SendAsync(CreateRequest("key-d", Guid.NewGuid(), token: "4111111111111111")))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    [Fact]
    public async Task Post_WhenPspDeclines_StoresFailedPayment()
    {
        var response = await _client.SendAsync(CreateRequest("key-e", Guid.NewGuid(), token: "tok_decline_test"));
        var body = await ReadAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        body.GetProperty("status").GetString().ShouldBe("Failed");
    }

    [Fact]
    public async Task Get_ForUnknownPayment_Returns404()
    {
        var get = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/payments/{Guid.NewGuid()}");
        get.Headers.Add("X-Merchant-Id", Guid.NewGuid().ToString());

        (await _client.SendAsync(get)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ForAnotherMerchantsPayment_Returns404()
    {
        var created = await ReadAsync(await _client.SendAsync(CreateRequest("key-f", Guid.NewGuid())));

        var get = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/payments/{created.GetProperty("id").GetGuid()}");
        get.Headers.Add("X-Merchant-Id", Guid.NewGuid().ToString());

        (await _client.SendAsync(get)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthReady_ReturnsOk() =>
        (await _client.GetAsync("/health/ready")).StatusCode.ShouldBe(HttpStatusCode.OK);
}
