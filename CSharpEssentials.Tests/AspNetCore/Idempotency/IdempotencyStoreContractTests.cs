using System.Text;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Behavior every <see cref="IIdempotencyStore"/> must have. Derive from it to run the same tests against another store.
/// </summary>
public abstract class IdempotencyStoreContractTests
{
    protected static readonly TimeSpan InFlight = TimeSpan.FromSeconds(30);
    protected static readonly TimeSpan Retention = TimeSpan.FromHours(1);

    protected FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));

    protected abstract IIdempotencyStore Store { get; }

    protected static IdempotentResponse Response(int statusCode = 201, string body = """{"id":1}""") =>
        new(statusCode,
            new Dictionary<string, string[]>
            {
                ["Content-Type"] = ["application/json; charset=utf-8"],
                ["Location"] = ["/orders/1"],
                ["Cache-Control"] = ["no-store", "private"],
            },
            Encoding.UTF8.GetBytes(body));

    protected async Task<string> ReserveAsync(string key, string fingerprint = "fp")
    {
        IdempotencyReservation reservation = await Store.TryReserveAsync(key, fingerprint, InFlight);
        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        return reservation.Token!;
    }

    [Fact]
    public async Task TryReserve_Should_Reserve_A_Free_Key()
    {
        IdempotencyReservation reservation = await Store.TryReserveAsync("k1", "fp", InFlight);

        reservation.Status.Should().Be(IdempotencyReservationStatus.Reserved);
        reservation.Token.Should().NotBeNullOrEmpty();
        reservation.Response.Should().BeNull();
    }

    [Fact]
    public async Task TryReserve_Should_Report_InFlight_For_Same_Fingerprint()
    {
        await Store.TryReserveAsync("k1", "fp", InFlight);

        IdempotencyReservation second = await Store.TryReserveAsync("k1", "fp", InFlight);

        second.Status.Should().Be(IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public async Task TryReserve_Should_Report_Mismatch_For_Different_Fingerprint_While_InFlight()
    {
        await Store.TryReserveAsync("k1", "fp", InFlight);

        IdempotencyReservation second = await Store.TryReserveAsync("k1", "other", InFlight);

        second.Status.Should().Be(IdempotencyReservationStatus.FingerprintMismatch);
    }

    [Fact]
    public async Task TryReserve_Should_Return_Completed_Response()
    {
        string token = await ReserveAsync("k1");
        (await Store.CompleteAsync("k1", token, "fp", Response(), Retention)).Should().BeTrue();

        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "fp", InFlight);

        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
        replay.Response!.StatusCode.Should().Be(201);
        replay.Response.Headers["Location"].Should().Equal("/orders/1");
        replay.Response.Headers["Cache-Control"].Should().Equal("no-store", "private");
        Encoding.UTF8.GetString(replay.Response.Body.Span).Should().Be("""{"id":1}""");
    }

    [Fact]
    public async Task TryReserve_Should_Report_Mismatch_For_Different_Fingerprint_After_Completion()
    {
        string token = await ReserveAsync("k1");
        (await Store.CompleteAsync("k1", token, "fp", Response(), Retention)).Should().BeTrue();

        IdempotencyReservation second = await Store.TryReserveAsync("k1", "other", InFlight);

        second.Status.Should().Be(IdempotencyReservationStatus.FingerprintMismatch);
    }

    [Fact]
    public async Task Release_Should_Free_An_InFlight_Key()
    {
        string token = await ReserveAsync("k1");

        (await Store.ReleaseAsync("k1", token)).Should().BeTrue();
        IdempotencyReservation retry = await Store.TryReserveAsync("k1", "other", InFlight);

        retry.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Release_Should_Keep_A_Completed_Key()
    {
        string token = await ReserveAsync("k1");
        (await Store.CompleteAsync("k1", token, "fp", Response(), Retention)).Should().BeTrue();

        (await Store.ReleaseAsync("k1", token)).Should().BeFalse();
        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "fp", InFlight);

        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
    }

    [Fact]
    public async Task Release_Of_Unknown_Key_Should_Do_Nothing()
    {
        (await Store.ReleaseAsync("missing", "token")).Should().BeFalse();
    }

    [Fact]
    public async Task InFlight_Reservation_Should_Expire_After_Timeout()
    {
        await Store.TryReserveAsync("k1", "fp", InFlight);

        Time.Advance(InFlight + TimeSpan.FromSeconds(1));
        IdempotencyReservation retry = await Store.TryReserveAsync("k1", "fp", InFlight);

        retry.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Completed_Response_Should_Live_For_Retention_Not_InFlight_Timeout()
    {
        string token = await ReserveAsync("k1");
        (await Store.CompleteAsync("k1", token, "fp", Response(), Retention)).Should().BeTrue();

        Time.Advance(InFlight + TimeSpan.FromSeconds(1));
        IdempotencyReservation beforeExpiry = await Store.TryReserveAsync("k1", "fp", InFlight);
        Time.Advance(Retention);
        IdempotencyReservation afterExpiry = await Store.TryReserveAsync("k1", "fp", InFlight);

        beforeExpiry.Status.Should().Be(IdempotencyReservationStatus.Completed);
        afterExpiry.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Keys_Should_Be_Independent()
    {
        await Store.TryReserveAsync("k1", "fp", InFlight);

        IdempotencyReservation other = await Store.TryReserveAsync("k2", "fp", InFlight);

        other.Status.Should().Be(IdempotencyReservationStatus.Reserved);
    }

    [Fact]
    public async Task Completed_Response_With_Empty_Body_Should_Round_Trip()
    {
        string token = await ReserveAsync("k1");
        await Store.CompleteAsync("k1", token, "fp", new IdempotentResponse(204, new Dictionary<string, string[]>(), ReadOnlyMemory<byte>.Empty), Retention);

        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "fp", InFlight);

        replay.Response!.StatusCode.Should().Be(204);
        replay.Response.Headers.Should().BeEmpty();
        replay.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Reservations_Should_Get_Distinct_Tokens()
    {
        string first = await ReserveAsync("k1");
        string second = await ReserveAsync("k2");

        first.Should().NotBe(second);
    }

    [Fact]
    public async Task Release_With_Another_Token_Should_Keep_The_Reservation()
    {
        await ReserveAsync("k1");

        (await Store.ReleaseAsync("k1", "other-token")).Should().BeFalse();
        IdempotencyReservation retry = await Store.TryReserveAsync("k1", "fp", InFlight);

        retry.Status.Should().Be(IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public async Task Late_Request_Should_Not_Release_The_Reservation_Of_A_Retry()
    {
        string late = await ReserveAsync("k1");
        Time.Advance(InFlight + TimeSpan.FromSeconds(1));
        await ReserveAsync("k1");

        (await Store.ReleaseAsync("k1", late)).Should().BeFalse();
        IdempotencyReservation third = await Store.TryReserveAsync("k1", "fp", InFlight);

        third.Status.Should().Be(IdempotencyReservationStatus.InFlight);
    }

    [Fact]
    public async Task Late_Request_Should_Not_Complete_Over_The_Reservation_Of_A_Retry()
    {
        string late = await ReserveAsync("k1");
        Time.Advance(InFlight + TimeSpan.FromSeconds(1));
        string retry = await ReserveAsync("k1", "other");

        (await Store.CompleteAsync("k1", late, "fp", Response(), Retention)).Should().BeFalse();
        (await Store.CompleteAsync("k1", retry, "other", Response(200, "retry"), Retention)).Should().BeTrue();
        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "other", InFlight);

        Encoding.UTF8.GetString(replay.Response!.Body.Span).Should().Be("retry");
    }

    [Fact]
    public async Task Expired_Reservation_That_Nobody_Took_Should_Still_Complete()
    {
        string token = await ReserveAsync("k1");
        Time.Advance(InFlight + TimeSpan.FromSeconds(1));

        (await Store.CompleteAsync("k1", token, "fp", Response(), Retention)).Should().BeTrue();
        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "fp", InFlight);

        replay.Status.Should().Be(IdempotencyReservationStatus.Completed);
    }

    [Fact]
    public async Task Complete_Twice_Should_Keep_The_First_Response()
    {
        string token = await ReserveAsync("k1");

        (await Store.CompleteAsync("k1", token, "fp", Response(201, "first"), Retention)).Should().BeTrue();
        (await Store.CompleteAsync("k1", token, "fp", Response(201, "second"), Retention)).Should().BeFalse();
        IdempotencyReservation replay = await Store.TryReserveAsync("k1", "fp", InFlight);

        Encoding.UTF8.GetString(replay.Response!.Body.Span).Should().Be("first");
    }
}
