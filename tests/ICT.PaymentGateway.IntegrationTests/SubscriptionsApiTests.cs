using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;

namespace ICT.PaymentGateway.IntegrationTests;

/// <summary>End-to-end subscription tests against a real PostgreSQL container. Requires Docker.</summary>
public sealed class SubscriptionsApiTests : IAsyncLifetime
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

    private static HttpRequestMessage Purchase(Guid merchantId, string key, string plan, string token = "tok_visa")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/subscriptions")
        {
            Content = JsonContent.Create(new { plan, paymentMethodToken = token }),
        };
        request.Headers.Add("X-Merchant-Id", merchantId.ToString());
        request.Headers.Add("Idempotency-Key", key);
        return request;
    }

    private static HttpRequestMessage Get(string url, Guid merchantId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Merchant-Id", merchantId.ToString());
        return request;
    }

    [Fact]
    public async Task GetPlans_ReturnsBasicClassicAndGold()
    {
        var plans = await _client.GetFromJsonAsync<JsonElement>("/api/v1/subscription-plans");

        plans.EnumerateArray().Select(p => p.GetProperty("plan").GetString())
            .ShouldBe(["Basic", "Classic", "Gold"]);
    }

    [Fact]
    public async Task Current_BeforePurchase_Returns404()
    {
        var response = await _client.SendAsync(Get("/api/v1/subscriptions/current", Guid.NewGuid()));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Purchase_WhenPaymentSucceeds_UnlocksAccount()
    {
        var merchant = Guid.NewGuid();

        var purchase = await _client.SendAsync(Purchase(merchant, "sub-a", "Gold"));
        var current = await _client.SendAsync(Get("/api/v1/subscriptions/current", merchant));
        var body = await current.Content.ReadFromJsonAsync<JsonElement>();

        purchase.StatusCode.ShouldBe(HttpStatusCode.Created);
        current.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.GetProperty("plan").GetString().ShouldBe("Gold");
    }

    [Fact]
    public async Task Purchase_WhenPaymentDeclined_Returns402AndStaysLocked()
    {
        var merchant = Guid.NewGuid();

        var purchase = await _client.SendAsync(Purchase(merchant, "sub-b", "Gold", token: "tok_decline_test"));
        var current = await _client.SendAsync(Get("/api/v1/subscriptions/current", merchant));

        purchase.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        current.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Purchase_ReplayWithSameKeyAndBody_ReturnsSameSubscription()
    {
        var merchant = Guid.NewGuid();

        var first = await (await _client.SendAsync(Purchase(merchant, "sub-c", "Classic"))).Content.ReadFromJsonAsync<JsonElement>();
        var second = await (await _client.SendAsync(Purchase(merchant, "sub-c", "Classic"))).Content.ReadFromJsonAsync<JsonElement>();

        second.GetProperty("id").GetGuid().ShouldBe(first.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Purchase_SameKeyDifferentPlan_Returns422()
    {
        var merchant = Guid.NewGuid();
        await _client.SendAsync(Purchase(merchant, "sub-d", "Basic"));

        var response = await _client.SendAsync(Purchase(merchant, "sub-d", "Gold"));

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Purchase_WithUnknownPlan_Returns400()
    {
        var response = await _client.SendAsync(Purchase(Guid.NewGuid(), "sub-e", "Platinum"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GatedEndpoint_IsForbiddenUntilPlanIsBought_ThenTierRulesApply()
    {
        var merchant = Guid.NewGuid();

        (await _client.SendAsync(Get("/api/v1/membership/basic", merchant))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await _client.SendAsync(Purchase(merchant, "sub-f", "Classic"));

        (await _client.SendAsync(Get("/api/v1/membership/basic", merchant))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _client.SendAsync(Get("/api/v1/membership/classic", merchant))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _client.SendAsync(Get("/api/v1/membership/gold", merchant))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
