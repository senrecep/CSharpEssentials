using System.Text;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyStoredEntryTests
{
    private const string NoteHeader = "X-Note";
    private const int ChunkSize = 1000;

    private static readonly string Note = string.Concat(Enumerable.Repeat("çöğüş€ ", 30));

    [Fact]
    public async Task Large_Chunked_Response_Should_Be_Stored_As_A_Version_3_Entry()
    {
        FakeDistributedCache? cache = null;
        await using IdempotencyTestHost host = await StartAsync(1024 * 1024, services => services.AddSingleton<IDistributedCache>(provider =>
            cache = new FakeDistributedCache(provider.GetRequiredService<TimeProvider>())));
        const int size = 300_000;

        IdempotencyResponse first = await host.PostAsync($"/sized?size={size}");

        first.StatusCode.Should().Be(StatusCodes.Status201Created);
        byte[] entry = (await cache!.GetAsync(cache.Keys.Single()))!;
        byte[] payload = ExpectedPayload(StatusCodes.Status201Created, Note, Body(size));
        entry[0].Should().Be(3);
        entry.AsSpan(entry.Length - payload.Length - sizeof(int), sizeof(int)).ToArray().Should().Equal(BitConverter.GetBytes(payload.Length));
        entry.AsSpan(entry.Length - payload.Length).ToArray().Should().Equal(payload);
    }

    [Fact]
    public async Task Large_Chunked_Response_Should_Be_Replayed_Byte_For_Byte()
    {
        await using IdempotencyTestHost host = await StartAsync(1024 * 1024, services => services.AddSingleton<IDistributedCache>(provider =>
            new FakeDistributedCache(provider.GetRequiredService<TimeProvider>())));
        const int size = 300_000;

        IdempotencyResponse first = await host.PostAsync($"/sized?size={size}");
        IdempotencyResponse replay = await host.PostAsync($"/sized?size={size}");

        replay.Replayed.Should().BeTrue();
        replay.StatusCode.Should().Be(first.StatusCode);
        replay.Body.Should().Be(Encoding.ASCII.GetString(Body(size)));
        replay.Header(NoteHeader).Should().Be(first.Header(NoteHeader));
        host.Probe.Executions.Should().Be(1);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(999, true)]
    [InlineData(1000, true)]
    [InlineData(1001, false)]
    public async Task Response_Should_Be_Stored_Only_Up_To_MaxResponseBodySize(int size, bool stored)
    {
        await using IdempotencyTestHost host = await StartAsync(1000, static _ => { });

        IdempotencyResponse first = await host.PostAsync($"/sized?size={size}");
        IdempotencyResponse retry = await host.PostAsync($"/sized?size={size}");

        first.Body.Should().HaveLength(size);
        if (stored)
        {
            retry.Replayed.Should().BeTrue();
            retry.Body.Should().Be(first.Body);
        }
        else
        {
            retry.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }

        host.Probe.Executions.Should().Be(1);
    }

    private static Task<IdempotencyTestHost> StartAsync(long maxResponseBodySize, Action<IServiceCollection> configureServices) =>
        IdempotencyTestHost.StartAsync(
            options =>
            {
                options.MaxResponseBodySize = maxResponseBodySize;
                options.ReplayedHeaders.Clear();
                options.ReplayedHeaders.Add(NoteHeader);
                options.UseDistributedCacheStore();
            },
            services =>
            {
                services.AddDistributedMemoryCache();
                configureServices(services);
            },
            configureEndpoints: static app => app.MapPost("/sized", async (HttpContext context, int size, IdempotencyProbe probe) =>
            {
                probe.Execute();
                context.Response.StatusCode = StatusCodes.Status201Created;
                context.Response.Headers[NoteHeader] = Note;
                byte[] body = Body(size);
                for (int offset = 0; offset < body.Length; offset += ChunkSize)
                    await context.Response.Body.WriteAsync(body.AsMemory(offset, Math.Min(ChunkSize, body.Length - offset)));
            }).WithIdempotency());

    private static byte[] Body(int size)
    {
        byte[] body = new byte[size];
        for (int index = 0; index < size; index++)
            body[index] = (byte)('a' + (index * 7 % 26));
        return body;
    }

    private static byte[] ExpectedPayload(int statusCode, string note, byte[] body)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((byte)3);
            writer.Write(statusCode);
            writer.Write(1);
            writer.Write(NoteHeader);
            writer.Write(1);
            writer.Write(note);
            writer.Write(body.Length);
            writer.Write(body);
        }

        return stream.ToArray();
    }
}
