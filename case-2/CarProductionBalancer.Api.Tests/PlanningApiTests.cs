using System.Net;
using System.Net.Http.Json;
using CarProductionBalancer.Api.Data;
using CarProductionBalancer.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CarProductionBalancer.Api.Tests;

public class PlanningApiTests
{
    // Bikin client test pakai sqlite in-memory biar unique constraint diuji beneran
    private static HttpClient CreateClient(SqliteConnection? connection = null)
    {
        var ownConnection = connection == null;
        connection ??= new SqliteConnection("Data Source=:memory:");
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlite(connection));
            });
        });

        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return client;
    }

    private CreatePlanningRequest SampleRequest(string requestCode = "REQ-TEST-001") => new()
    {
        RequestCode = requestCode,
        CandidateToken = "token-abc",
        Slots = new()
        {
            new() { SlotName = "Monday",    OriginalQuantity = 5 },
            new() { SlotName = "Tuesday",   OriginalQuantity = 3 },
            new() { SlotName = "Wednesday", OriginalQuantity = 7 },
            new() { SlotName = "Thursday",  OriginalQuantity = 0 },
            new() { SlotName = "Friday",    OriginalQuantity = 8 },
            new() { SlotName = "Saturday",  OriginalQuantity = 0 },
            new() { SlotName = "Sunday",    OriginalQuantity = 4 },
        }
    };

    // 1. POST request valid -> status 201 Created dan hasilnya balance
    [Fact]
    public async Task Post_ValidRequest_Returns201AndBalancedResult()
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/planning", SampleRequest());

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(body);
        Assert.Equal("REQ-TEST-001", body!.RequestCode);
        Assert.Equal("Success", body.Status);
        Assert.Equal(27, body.OriginalTotal);
        Assert.Equal(27, body.BalancedTotal);
        Assert.Equal(7, body.Slots.Count);

        var activeBalanced = body.Slots.Where(s => s.IsActive).Select(s => s.BalancedQuantity).ToList();
        Assert.True(activeBalanced.Max() - activeBalanced.Min() <= 1);
    }

    // 2. Kirim RequestCode yang sama -> return 200 dengan data lama (idempotency)
    [Fact]
    public async Task Post_DuplicateRequestCode_Returns200WithSameData()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();

        var client1 = CreateClient(conn);
        var req = SampleRequest("REQ-IDEMPOTENT");

        var res1 = await client1.PostAsJsonAsync("/api/planning", req);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        var body1 = await res1.Content.ReadFromJsonAsync<PlanningResponse>();

        var client2 = CreateClient(conn);
        var res2 = await client2.PostAsJsonAsync("/api/planning", req);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        var body2 = await res2.Content.ReadFromJsonAsync<PlanningResponse>();

        Assert.Equal(body1!.PlanningId, body2!.PlanningId);
        Assert.Equal(body1.RequestCode, body2.RequestCode);
    }

    // 3. Ada slot nilainya negatif -> return 400
    [Fact]
    public async Task Post_NegativeSlot_Returns400()
    {
        var client = CreateClient();
        var req = SampleRequest("REQ-NEGATIVE");
        req.Slots[2].OriginalQuantity = -5;

        var res = await client.PostAsJsonAsync("/api/planning", req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Errors);
    }

    // 4. RequestCode kosong -> return 400
    [Fact]
    public async Task Post_MissingRequestCode_Returns400()
    {
        var client = CreateClient();
        var req = SampleRequest();
        req.RequestCode = "";

        var res = await client.PostAsJsonAsync("/api/planning", req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    // 5. GET riwayat planning
    [Fact]
    public async Task Get_History_ReturnsListOfPlannings()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();

        var writeClient = CreateClient(conn);
        await writeClient.PostAsJsonAsync("/api/planning", SampleRequest("REQ-HIST-1"));
        await writeClient.PostAsJsonAsync("/api/planning", SampleRequest("REQ-HIST-2"));

        var readClient = CreateClient(conn);
        var res = await readClient.GetAsync("/api/planning");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var list = await res.Content.ReadFromJsonAsync<List<PlanningListItem>>();
        Assert.NotNull(list);
        Assert.True(list!.Count >= 2);
    }

    // 6. GET detail planning berdasarkan id
    [Fact]
    public async Task Get_ById_ReturnsDetailWithSlots()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();

        var writeClient = CreateClient(conn);
        var postRes = await writeClient.PostAsJsonAsync("/api/planning", SampleRequest("REQ-DETAIL"));
        var created = await postRes.Content.ReadFromJsonAsync<PlanningResponse>();

        var readClient = CreateClient(conn);
        var res = await readClient.GetAsync($"/api/planning/{created!.PlanningId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var body = await res.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.PlanningId, body!.PlanningId);
        Assert.Equal(7, body.Slots.Count);
    }

    // 7. GET detail dengan id yang ga ada -> return 404
    [Fact]
    public async Task Get_ById_UnknownId_Returns404()
    {
        var client = CreateClient();
        var res = await client.GetAsync($"/api/planning/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // 8. Cek invarian total awal == total hasil
    [Fact]
    public async Task Post_ValidRequest_TotalUnchanged()
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/planning", SampleRequest("REQ-INVARIANT"));
        var body = await res.Content.ReadFromJsonAsync<PlanningResponse>();

        Assert.Equal(body!.OriginalTotal, body.BalancedTotal);
        Assert.Equal(body.OriginalTotal, body.Slots.Sum(s => s.BalancedQuantity));
    }

    // 9. Cek invarian slot 0 tetap 0
    [Fact]
    public async Task Post_ValidRequest_InactiveSlotsRemainZero()
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/planning", SampleRequest("REQ-ZEROS"));
        var body = await res.Content.ReadFromJsonAsync<PlanningResponse>();

        foreach (var slot in body!.Slots.Where(s => !s.IsActive))
            Assert.Equal(0, slot.BalancedQuantity);
    }

    // 10. Kalau semua slot 0, hasil output tetap 0 semua
    [Fact]
    public async Task Post_AllZeroSlots_ReturnsAllZero()
    {
        var client = CreateClient();
        var req = new CreatePlanningRequest
        {
            RequestCode = "REQ-ALL-ZERO",
            CandidateToken = "token",
            Slots = Enumerable.Range(1, 7).Select(i => new SlotRequest
            {
                SlotName = $"Day{i}",
                OriginalQuantity = 0
            }).ToList()
        };

        var res = await client.PostAsJsonAsync("/api/planning", req);
        var body = await res.Content.ReadFromJsonAsync<PlanningResponse>();

        Assert.Equal(0, body!.BalancedTotal);
        Assert.All(body.Slots, s => Assert.Equal(0, s.BalancedQuantity));
    }
}
