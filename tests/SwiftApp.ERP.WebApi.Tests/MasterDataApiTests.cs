using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Persistence;
using Xunit;

namespace SwiftApp.ERP.WebApi.Tests;

public class MasterDataApiTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly ErpWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public MasterDataApiTests(ErpWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetCategories_ShouldReturnList()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/masterdata/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<CategoryResponse>>(body, JsonOpts);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCategory_ShouldReturn201()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var request = new CategoryRequest("Test Category", "A test category", null, null, null);

        var response = await client.PostAsJsonAsync("/api/v1/masterdata/categories", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<CategoryResponse>(JsonOpts);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task CreateAndGetCategory_ShouldRoundtrip()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var request = new CategoryRequest("Roundtrip Cat", "Test roundtrip", null, null, null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/masterdata/categories", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>(JsonOpts);
        created.Should().NotBeNull();

        var getResponse = await client.GetAsync($"/api/v1/masterdata/categories/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content.ReadFromJsonAsync<CategoryResponse>(JsonOpts);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Roundtrip Cat");
        fetched.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetNonExistentCategory_ShouldReturn404()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1/masterdata/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPagedResult()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/masterdata/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductResponse>>(body, JsonOpts);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUnitsOfMeasure_ShouldReturnList()
    {
        await _factory.EnsureDatabaseCreatedAsync();
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/masterdata/units-of-measure");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
