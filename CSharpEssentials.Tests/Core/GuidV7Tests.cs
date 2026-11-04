using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class GuidV7Tests
{
    private const int _rfcSampleSize = 30_000;

    public static TheoryData<string> Generators => ["NewGuid", "portable"];

    private static Guid Generate(string generator) =>
        generator == "NewGuid" ? Guider.NewGuid() : GuidV7.NewGuid();

    private static byte[] BigEndian(Guid guid) => guid.ToByteArray(bigEndian: true);

    private static long UnixMs(byte[] b) =>
        ((long)b[0] << 40) | ((long)b[1] << 32) | ((long)b[2] << 24) | ((long)b[3] << 16) | ((long)b[4] << 8) | b[5];


    [Theory]
    [MemberData(nameof(Generators))]
    public void Generated_ids_carry_version_7_and_variant_10(string generator)
    {
        for (int i = 0; i < _rfcSampleSize; i++)
        {
            Guid guid = Generate(generator);
            byte[] b = BigEndian(guid);

            (b[6] >> 4).Should().Be(7);
            (b[8] >> 6).Should().Be(0b10);
            guid.Version.Should().Be(7);
            (guid.Variant & 0b1100).Should().Be(0b1000);
        }
    }

    [Theory]
    [InlineData("NewGuid")]
    [InlineData("portable")]
    public void Stateless_timestamp_matches_the_clock(string generator)
    {
        long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid guid = Generate(generator);
        long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        UnixMs(BigEndian(guid)).Should().BeInRange(before, after);
    }

    [Fact]
    public void Compose_matches_the_bit_layout_of_Guid_CreateVersion7()
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;
        for (int i = 0; i < 1_000; i++)
        {
            DateTimeOffset timestamp = start.AddMilliseconds(i * 7919);
            Guid expected = Guid.CreateVersion7(timestamp);
            byte[] bytes = BigEndian(expected);

            Guid actual = GuidV7.Compose(timestamp.ToUnixTimeMilliseconds(), bytes);

            actual.Should().Be(expected);
        }
    }

    [Fact]
    public void Compose_writes_version_and_variant_over_any_random_bits()
    {
        byte[] ones = Enumerable.Repeat((byte)0xFF, 16).ToArray();
        byte[] zeros = new byte[16];

        Guid fromOnes = GuidV7.Compose(0x0123_4567_89AB, ones);
        Guid fromZeros = GuidV7.Compose(0x0123_4567_89AB, zeros);

        fromOnes.Should().Be(Guid.Parse("01234567-89ab-7fff-bfff-ffffffffffff"));
        fromZeros.Should().Be(Guid.Parse("01234567-89ab-7000-8000-000000000000"));
    }

    [Theory]
    [MemberData(nameof(Generators))]
    public void Generators_do_not_allocate(string generator)
    {
        for (int i = 0; i < 1_000; i++)
            Generate(generator);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10_000; i++)
            Generate(generator);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.Should().Be(0);
    }
}
