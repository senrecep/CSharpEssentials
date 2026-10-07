using System.Net;
using System.Net.Http.Headers;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Http;

public sealed class SsrfGuardHandlerTests : IAsyncLifetime
{
    private static readonly Dictionary<string, IPAddress[]> FakeDns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["loopback.test"] = [IPAddress.Loopback],
        ["alias.test"] = [IPAddress.Loopback],
        ["mixed.test"] = [IPAddress.Parse("8.8.8.8"), IPAddress.Parse("10.0.0.5")],
        ["private.test"] = [IPAddress.Parse("192.168.1.10")],
        ["metadata.test"] = [IPAddress.Parse("169.254.169.254")],
        ["mapped.test"] = [IPAddress.Parse("::ffff:127.0.0.1")],
        ["public.test"] = [IPAddress.Parse("8.8.8.8")],
        ["public-mapped.test"] = [IPAddress.Parse("::ffff:8.8.8.8")],
        ["public-nat64.test"] = [IPAddress.Parse("64:ff9b::808:808")],
        ["xn--bcher-kva.test"] = [IPAddress.Loopback],
    };

    private static readonly IPNetwork LoopbackNetwork = IPNetwork.Parse("127.0.0.0/8");

    private SsrfLoopbackServer _server = null!;
    private readonly List<ServiceProvider> _providers = [];

    public async Task InitializeAsync() => _server = await SsrfLoopbackServer.StartAsync();

    public async Task DisposeAsync()
    {
        foreach (ServiceProvider provider in _providers)
            await provider.DisposeAsync();
        await _server.DisposeAsync();
    }

    [Fact]
    public async Task SendAsync_Should_Connect_For_Real_When_Loopback_Is_Allowlisted()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Block_When_Host_Is_Loopback_Literal()
    {
        HttpClient client = CreateClient();

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("127.0.0.1", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
        ex.RequestUri.Should().Be(_server.Url("127.0.0.1", "/ok"));
    }

    [Theory]
    [InlineData("private.test")]
    [InlineData("metadata.test")]
    [InlineData("mapped.test")]
    [InlineData("169.254.169.254")]
    [InlineData("[::1]")]
    [InlineData("[::ffff:127.0.0.1]")]
    public async Task SendAsync_Should_Block_When_Address_Is_Private(string host)
    {
        HttpClient client = CreateClient();

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Block_When_Dns_Returns_Public_And_Private_Addresses()
    {
        HttpClient client = CreateClient();

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("mixed.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
        ex.BlockedAddress.Should().Be(IPAddress.Parse("10.0.0.5"));
    }

    [Fact]
    public async Task SendAsync_Should_Not_Reveal_Resolved_Address_In_Message_When_Address_Is_Blocked()
    {
        HttpClient client = CreateClient();

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("metadata.test", "/ok")));

        ex.Message.Should().NotContain("169.254");
        ex.BlockedAddress.Should().Be(IPAddress.Parse("169.254.169.254"));
    }

    [Fact]
    public async Task SendAsync_Should_Block_Http_When_AllowHttp_Is_False()
    {
        HttpClient client = CreateClient(o =>
        {
            o.AllowHttp = false;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Block_Port_Outside_Allowlist()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(new Uri("http://loopback.test:6379/")));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Block_Redirect_To_Private_Address()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        Uri target = _server.Url("169.254.169.254", "/latest/meta-data/");

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(RedirectUrl(target)));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
        ex.RequestUri.Should().Be(target);
    }

    [Fact]
    public async Task SendAsync_Should_Block_Redirect_To_Host_Resolving_To_Private_Address()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(RedirectUrl(_server.Url("metadata.test", "/"))));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Theory]
    [InlineData("ftp://loopback.test/file")]
    [InlineData("file:///etc/passwd")]
    [InlineData("http://loopback.test/")]
    public async Task SendAsync_Should_Block_Redirect_To_Disallowed_Scheme_Or_Port(string target)
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(RedirectUrl(new Uri(target))));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Follow_Redirects_Up_To_The_Limit()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/chain/3"));

        body.Should().Be("done");
    }

    [Fact]
    public async Task SendAsync_Should_Throw_When_Redirect_Limit_Is_Exceeded()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/chain/4")));

        ex.Reason.Should().Be(SsrfBlockReason.RedirectLimitExceeded);
    }

    [Fact]
    public async Task SendAsync_Should_Not_Follow_Redirects_When_MaxRedirects_Is_Zero()
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxRedirects = 0;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/chain/1")));

        ex.Reason.Should().Be(SsrfBlockReason.RedirectLimitExceeded);
    }

    [Fact]
    public async Task SendAsync_Should_Return_Redirect_Response_Without_Location()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        using HttpResponseMessage response = await client.GetAsync(_server.Url("loopback.test", "/redirect?to=&status=302"));

        response.StatusCode.Should().Be(HttpStatusCode.Found);
    }

    [Fact]
    public async Task SendAsync_Should_Keep_Authorization_On_Same_Origin_Redirect()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var request = new HttpRequestMessage(HttpMethod.Get, RedirectUrl(new Uri("/auth", UriKind.Relative)));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");

        using HttpResponseMessage response = await client.SendAsync(request);

        (await response.Content.ReadAsStringAsync()).Should().Be("Bearer secret");
    }

    [Fact]
    public async Task SendAsync_Should_Strip_Authorization_On_Cross_Origin_Redirect()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var request = new HttpRequestMessage(HttpMethod.Get, RedirectUrl(_server.Url("alias.test", "/auth")));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");

        using HttpResponseMessage response = await client.SendAsync(request);

        (await response.Content.ReadAsStringAsync()).Should().Be("none");
    }

    [Fact]
    public async Task SendAsync_Should_Strip_Credentials_And_Cookies_When_Redirect_Is_Cross_Origin()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using HttpRequestMessage request = CreateCredentialedRequest(RedirectUrl(_server.Url("alias.test", "/headers")));

        using HttpResponseMessage response = await client.SendAsync(request);

        (await response.Content.ReadAsStringAsync()).Should().Be("||");
    }

    [Fact]
    public async Task SendAsync_Should_Keep_Credentials_And_Cookies_When_Redirect_Is_Same_Origin()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using HttpRequestMessage request = CreateCredentialedRequest(RedirectUrl(new Uri("/headers", UriKind.Relative)));

        using HttpResponseMessage response = await client.SendAsync(request);

        (await response.Content.ReadAsStringAsync()).Should().Be("Bearer secret|session=abc|Basic proxy");
    }

    [Fact]
    public async Task SendAsync_Should_Not_Store_Cookies_When_Response_Sets_One()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/set-cookie"));

        body.Should().Be("||");
    }

    [Theory]
    [InlineData(3, 0, HttpVersionPolicy.RequestVersionOrLower)]
    [InlineData(3, 0, HttpVersionPolicy.RequestVersionOrHigher)]
    [InlineData(3, 0, HttpVersionPolicy.RequestVersionExact)]
    [InlineData(1, 1, HttpVersionPolicy.RequestVersionOrHigher)]
    [InlineData(2, 0, HttpVersionPolicy.RequestVersionOrHigher)]
    public async Task SendAsync_Should_Cap_Version_Below_Http3_When_Request_Allows_Http3(int major, int minor, HttpVersionPolicy policy)
    {
        using var recorder = new VersionRecordingHandler();
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork), configureBuilder: b => b.AddHttpMessageHandler(() => recorder));
        using var request = new HttpRequestMessage(HttpMethod.Get, _server.Url("loopback.test", "/version"))
        {
            Version = new Version(major, minor),
            VersionPolicy = policy,
        };

        using HttpResponseMessage response = await client.SendAsync(request);

        recorder.Version.Should().BeLessThanOrEqualTo(HttpVersion.Version20);
        recorder.VersionPolicy.Should().Be(HttpVersionPolicy.RequestVersionOrLower);
        response.Version.Should().BeLessThan(HttpVersion.Version30);
        (await response.Content.ReadAsStringAsync()).Should().NotBe("HTTP/3");
    }

    [Fact]
    public async Task SendAsync_Should_Keep_Exact_Version_Policy_When_Version_Is_Below_Http3()
    {
        using var recorder = new VersionRecordingHandler();
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork), configureBuilder: b => b.AddHttpMessageHandler(() => recorder));
        using var request = new HttpRequestMessage(HttpMethod.Get, _server.Url("loopback.test", "/version"))
        {
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };

        using HttpResponseMessage response = await client.SendAsync(request);

        recorder.Version.Should().Be(HttpVersion.Version11);
        recorder.VersionPolicy.Should().Be(HttpVersionPolicy.RequestVersionExact);
        (await response.Content.ReadAsStringAsync()).Should().Be("HTTP/1.1");
    }

    [Theory]
    [InlineData(301, "GET:")]
    [InlineData(302, "GET:")]
    [InlineData(303, "GET:")]
    [InlineData(307, "POST:payload")]
    [InlineData(308, "POST:payload")]
    public async Task SendAsync_Should_Apply_Method_Semantics_Per_Redirect_Status(int status, string expected)
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var content = new StringContent("payload");

        using HttpResponseMessage response = await client.PostAsync(
            RedirectUrl(new Uri("/echo", UriKind.Relative), status),
            content);

        (await response.Content.ReadAsStringAsync()).Should().Be(expected);
    }

    [Fact]
    public async Task SendAsync_Should_Turn_Put_Into_Get_On_303()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var content = new StringContent("payload");

        using HttpResponseMessage response = await client.PutAsync(
            RedirectUrl(new Uri("/echo", UriKind.Relative), 303),
            content);

        (await response.Content.ReadAsStringAsync()).Should().Be("GET:");
    }

    [Fact]
    public async Task SendAsync_Should_Keep_Put_On_302()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var content = new StringContent("payload");

        using HttpResponseMessage response = await client.PutAsync(
            RedirectUrl(new Uri("/echo", UriKind.Relative), 302),
            content);

        (await response.Content.ReadAsStringAsync()).Should().Be("PUT:payload");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SendAsync_Should_Throw_When_Buffered_Response_Exceeds_Limit(bool declareLength)
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxResponseContentLength = 4096;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });

        SsrfBlockedException ex = await AssertBlockedAsync(
            () => client.GetAsync(_server.Url("loopback.test", $"/big?length=100000&declare={declareLength}")));

        ex.Reason.Should().Be(SsrfBlockReason.ResponseTooLarge);
    }

    [Fact]
    public async Task SendAsync_Should_Abort_Streamed_Response_When_Limit_Is_Exceeded()
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxResponseContentLength = 4096;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/big?length=100000&declare=false"),
            HttpCompletionOption.ResponseHeadersRead);
        await using Stream stream = await response.Content.ReadAsStreamAsync();

        Func<Task> act = () => stream.CopyToAsync(Stream.Null);

        (await act.Should().ThrowAsync<SsrfBlockedException>()).Which.Reason.Should().Be(SsrfBlockReason.ResponseTooLarge);
    }

    [Fact]
    public async Task SendAsync_Should_Read_Response_Within_Limit()
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxResponseContentLength = 4096;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });

        byte[] body = await client.GetByteArrayAsync(_server.Url("loopback.test", "/big?length=4096&declare=false"));

        body.Should().HaveCount(4096);
    }

    [Fact]
    public async Task SendAsync_Should_Throw_Timeout_When_Headers_Are_Late()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromMilliseconds(300);
            o.AllowedNetworks.Add(LoopbackNetwork);
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/slow")));

        ex.Reason.Should().Be(SsrfBlockReason.Timeout);
        ex.InnerException.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public async Task SendAsync_Should_Throw_Timeout_When_Body_Is_Late()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromMilliseconds(500);
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/slow-body"),
            HttpCompletionOption.ResponseHeadersRead);

        Func<Task> act = () => response.Content.ReadAsStringAsync();

        (await act.Should().ThrowAsync<SsrfBlockedException>()).Which.Reason.Should().Be(SsrfBlockReason.Timeout);
    }

    [Fact]
    public async Task Read_Synchronous_Should_Throw_Ssrf_Timeout_When_Timeout_Has_Elapsed()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromMilliseconds(300);
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/slow-body"),
            HttpCompletionOption.ResponseHeadersRead);
        using Stream stream = await response.Content.ReadAsStreamAsync();
        await Task.Delay(TimeSpan.FromMilliseconds(600));

        Action act = () => stream.ReadExactly(new byte[1]);

        act.Should().Throw<SsrfBlockedException>().Which.Reason.Should().Be(SsrfBlockReason.Timeout);
    }

    [Fact]
    public async Task ReadAsync_Should_Throw_Ssrf_Timeout_When_Caller_Token_Is_Cancelable()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromMilliseconds(500);
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/slow-body"),
            HttpCompletionOption.ResponseHeadersRead);

        Func<Task> act = () => response.Content.ReadAsStringAsync(cts.Token);

        (await act.Should().ThrowAsync<SsrfBlockedException>()).Which.Reason.Should().Be(SsrfBlockReason.Timeout);
    }

    [Fact]
    public async Task SendAsync_Should_Throw_OperationCanceled_When_Caller_Cancels()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromSeconds(30);
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        Func<Task> act = () => client.GetAsync(_server.Url("loopback.test", "/slow"), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SendAsync_Should_Use_Registered_Address_Policy()
    {
        HttpClient client = CreateClient(services: s => s.AddSingleton<IOutboundAddressPolicy, AllowLoopbackPolicy>());

        string body = await client.GetStringAsync(_server.Url("127.0.0.1", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Use_Registered_Request_Policy()
    {
        HttpClient client = CreateClient(
            o => o.AllowedNetworks.Add(LoopbackNetwork),
            s => s.AddSingleton<IOutboundRequestPolicy, DenyPathPolicy>());

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Use_Address_Policy_From_Options()
    {
        HttpClient client = CreateClient(o => o.AddressPolicy = new AllowLoopbackPolicy());

        string body = await client.GetStringAsync(_server.Url("127.0.0.1", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Prefer_Options_Address_Policy_When_Container_Has_One()
    {
        HttpClient client = CreateClient(
            o => o.AddressPolicy = DefaultOutboundAddressPolicy.Instance,
            s => s.AddSingleton<IOutboundAddressPolicy, AllowLoopbackPolicy>());

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("127.0.0.1", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Use_Request_Policy_From_Options()
    {
        HttpClient client = CreateClient(o =>
        {
            o.AllowedNetworks.Add(LoopbackNetwork);
            o.RequestPolicy = new DenyPathPolicy();
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Prefer_Options_Request_Policy_When_Container_Has_One()
    {
        HttpClient client = CreateClient(
            o =>
            {
                o.AllowedNetworks.Add(LoopbackNetwork);
                o.RequestPolicy = new AllowAllRequestPolicy();
            },
            s => s.AddSingleton<IOutboundRequestPolicy, DenyPathPolicy>());

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Apply_Options_Policies_Per_Client()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("open").AddSsrfGuard(o =>
        {
            ConfigureForServer(o);
            o.AddressPolicy = new AllowLoopbackPolicy();
        });
        services.AddHttpClient("strict").AddSsrfGuard(ConfigureForServer);
        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);
        IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();

        string body = await factory.CreateClient("open").GetStringAsync(_server.Url("127.0.0.1", "/ok"));
        SsrfBlockedException ex = await AssertBlockedAsync(() => factory.CreateClient("strict").GetAsync(_server.Url("127.0.0.1", "/ok")));

        body.Should().Be("ok");
        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Theory]
    [InlineData("[::ffff:127.0.0.1]")]
    [InlineData("mapped.test")]
    public async Task SendAsync_Should_Pass_Normalized_And_Raw_Address_To_Policy_When_Address_Embeds_IPv4(string host)
    {
        var policy = new RecordingAddressPolicy(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        HttpClient client = CreateClient(o => o.AddressPolicy = policy);

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        policy.Addresses.Should().Equal(IPAddress.Loopback, IPAddress.Parse("::ffff:127.0.0.1"));
        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Theory]
    [InlineData("[::1]")]
    [InlineData("[::ffff:127.0.0.1]")]
    [InlineData("[::127.0.0.1]")]
    [InlineData("mapped.test")]
    public async Task SendAsync_Should_Block_When_Custom_Policy_Rejects_Loopback(string host)
    {
        HttpClient client = CreateClient(o => o.AddressPolicy = new RejectLoopbackPolicy());

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Theory]
    [InlineData("[::1]")]
    [InlineData("[::]")]
    public async Task SendAsync_Should_Block_When_AllowedNetworks_Matches_Only_Normalized_Address(string host)
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(IPNetwork.Parse("0.0.0.0/0")));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Theory]
    [InlineData("public.test")]
    [InlineData("public-mapped.test")]
    [InlineData("public-nat64.test")]
    public async Task SendAsync_Should_Block_When_BlockedNetworks_Contains_Embedded_IPv4(string host)
    {
        HttpClient client = CreateClient(o => o.BlockedNetworks.Add(IPNetwork.Parse("8.8.8.0/24")));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Block_When_BlockedNetworks_Contains_IPv6_Form()
    {
        HttpClient client = CreateClient(o => o.BlockedNetworks.Add(IPNetwork.Parse("64:ff9b::/96")));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("public-nat64.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
        ex.BlockedAddress.Should().Be(IPAddress.Parse("64:ff9b::808:808"));
    }

    [Fact]
    public async Task SendAsync_Should_Allow_When_AllowedNetworks_Contains_Embedded_IPv4()
    {
        var policy = new RecordingAddressPolicy();
        HttpClient client = CreateClient(o =>
        {
            o.AddressPolicy = policy;
            o.AllowedNetworks.Add(IPNetwork.Parse("127.0.0.0/8"));
        });

        string body = await client.GetStringAsync(_server.Url("127.0.0.1", "/ok"));

        body.Should().Be("ok");
        policy.Addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_Should_Still_Block_Other_Ranges_When_AllowedNetworks_Is_Set()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(IPNetwork.Parse("10.0.0.0/8")));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("private.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
        ex.BlockedAddress.Should().Be(IPAddress.Parse("192.168.1.10"));
    }

    [Fact]
    public async Task SendAsync_Should_Use_Network_Snapshot_When_Options_Are_Mutated_After_Client_Creation()
    {
        SsrfGuardOptions? captured = null;
        HttpClient client = CreateClient(o =>
        {
            o.AllowedNetworks.Add(LoopbackNetwork);
            captured = o;
        });
        captured!.AllowedNetworks.Clear();
        captured.BlockedNetworks.Add(LoopbackNetwork);

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Keep_Blocked_Network_Snapshot_When_Options_Are_Cleared_After_Client_Creation()
    {
        SsrfGuardOptions? captured = null;
        HttpClient client = CreateClient(o =>
        {
            o.AllowedHosts.Add("loopback.test");
            o.BlockedNetworks.Add(LoopbackNetwork);
            captured = o;
        });
        captured!.BlockedNetworks.Clear();

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Allow_Allowlisted_Host()
    {
        HttpClient client = CreateClient(o => o.AllowedHosts.Add("*.test"));

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Block_Denylisted_Network_Even_When_Host_Is_Allowlisted()
    {
        HttpClient client = CreateClient(o =>
        {
            o.AllowedHosts.Add("loopback.test");
            o.BlockedNetworks.Add(IPNetwork.Parse("127.0.0.1/32"));
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendAsync_Should_Block_Denylisted_Host()
    {
        HttpClient client = CreateClient(o =>
        {
            o.AllowedNetworks.Add(LoopbackNetwork);
            o.BlockedHosts.Add("alias.test");
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("alias.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.HostNotAllowed);
    }

    [Theory]
    [InlineData("alias.test.", "alias.test")]
    [InlineData("alias.test", "alias.test.")]
    [InlineData("ALIAS.TEST.", "alias.test")]
    [InlineData("*.TEST.", "alias.test.")]
    [InlineData("bücher.test", "bücher.test")]
    [InlineData("BÜCHER.test", "xn--bcher-kva.test")]
    [InlineData("xn--bcher-kva.test", "bücher.test")]
    public async Task SendAsync_Should_Block_Denylisted_Host_When_Host_Differs_By_Trailing_Dot_Case_Or_Idn(string pattern, string host)
    {
        HttpClient client = CreateClient(o =>
        {
            o.AllowedNetworks.Add(LoopbackNetwork);
            o.BlockedHosts.Add(pattern);
        });

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url(host, "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.HostNotAllowed);
    }

    [Theory]
    [InlineData("bücher.test")]
    [InlineData("BÜCHER.TEST.")]
    [InlineData("*.test")]
    public async Task SendAsync_Should_Allow_Allowlisted_Idn_Host_When_Pattern_Is_Unicode_Or_Wildcard(string pattern)
    {
        HttpClient client = CreateClient(o => o.AllowedHosts.Add(pattern));

        string body = await client.GetStringAsync(_server.Url("bücher.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Allow_Allowlisted_Host_When_Pattern_Has_Trailing_Dot()
    {
        HttpClient client = CreateClient(o => o.AllowedHosts.Add("loopback.test."));

        string body = await client.GetStringAsync(_server.Url("loopback.test", "/ok"));

        body.Should().Be("ok");
    }

    [Fact]
    public async Task SendAsync_Should_Throw_When_Primary_Handler_Is_Replaced()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("guarded")
            .AddSsrfGuard()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("guarded");

        Func<Task> act = () => client.GetAsync(new Uri("https://example.com/"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendAsync_Should_Throw_When_Proxy_Is_Enabled_After_Guard()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("guarded")
            .AddSsrfGuard()
            .ConfigurePrimaryHttpMessageHandler((handler, _) => GetSocketsHandler(handler).UseProxy = true);
        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("guarded");

        Func<Task> act = () => client.GetAsync(new Uri("https://example.com/"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendAsync_Should_Throw_When_Cookies_Are_Enabled_After_Guard()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("guarded")
            .AddSsrfGuard()
            .ConfigurePrimaryHttpMessageHandler((handler, _) => GetSocketsHandler(handler).UseCookies = true);
        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("guarded");

        Func<Task> act = () => client.GetAsync(new Uri("https://example.com/"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendAsync_Should_Cap_Version_When_Handler_After_Guard_Requests_Http3()
    {
        using var rewriter = new RewritingHandler(request =>
        {
            request.Version = HttpVersion.Version30;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        });
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork), configureBuilder: b => b.AddHttpMessageHandler(() => rewriter));

        using HttpResponseMessage response = await client.GetAsync(_server.Url("loopback.test", "/version"));

        response.RequestMessage!.Version.Should().BeLessThanOrEqualTo(HttpVersion.Version20);
        response.RequestMessage.VersionPolicy.Should().Be(HttpVersionPolicy.RequestVersionOrLower);
        (await response.Content.ReadAsStringAsync()).Should().Be("HTTP/1.1");
    }

    [Theory]
    [InlineData("http://loopback.test:6379/")]
    [InlineData("ftp://loopback.test/file")]
    public async Task SendAsync_Should_Block_When_Handler_After_Guard_Rewrites_Uri_To_Disallowed_Target(string target)
    {
        using var rewriter = new RewritingHandler(request => request.RequestUri = new Uri(target));
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork), configureBuilder: b => b.AddHttpMessageHandler(() => rewriter));

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(_server.Url("loopback.test", "/ok")));

        ex.Reason.Should().Be(SsrfBlockReason.RequestNotAllowed);
        ex.RequestUri.Should().Be(new Uri(target));
    }

    [Fact]
    public async Task SendAsync_Should_Not_Reveal_Query_Or_UserInfo_In_Message_When_Request_Is_Blocked()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        var uri = new Uri("http://user:hunter2@loopback.test:6379/path?token=s3cr3t#frag");

        SsrfBlockedException ex = await AssertBlockedAsync(() => client.GetAsync(uri));

        ex.Message.Should().Contain("http://loopback.test:6379/path").And.NotContain("hunter2").And.NotContain("s3cr3t");
        ex.RequestUri.Should().Be(uri);
    }

    [Fact]
    public async Task ReadAsByteArrayAsync_Should_Read_Body_When_Caller_Token_Is_Cancelable()
    {
        HttpClient client = CreateClient(o =>
        {
            o.Timeout = TimeSpan.FromSeconds(30);
            o.MaxResponseContentLength = 100_000;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/big?length=100000&declare=false"),
            HttpCompletionOption.ResponseHeadersRead,
            cts.Token);

        byte[] body = await response.Content.ReadAsByteArrayAsync(cts.Token);

        body.Should().HaveCount(100_000);
    }

    [Fact]
    public void Send_Synchronous_Should_Throw_NotSupported()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));
        using var request = new HttpRequestMessage(HttpMethod.Get, _server.Url("loopback.test", "/ok"));

        Action act = () => client.Send(request);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_Should_Return_Failure_When_Blocked()
    {
        HttpClient client = CreateClient();

        Result<string> result = await client.GetFromJsonAsResultAsync<string>(_server.Url("private.test", "/json"));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("Http.SsrfBlocked");
        result.FirstError.Description.Should().NotContain("192.168");
        result.FirstError.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task PostAsResultAsync_Should_Return_Forbidden_When_Request_Is_Blocked()
    {
        HttpClient client = CreateClient();
        using var content = new StringContent("payload");

        Result result = await client.PostAsResultAsync(_server.Url("metadata.test", "/echo"), content);

        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("Http.SsrfBlocked");
        result.FirstError.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be(SsrfBlockReason.AddressNotAllowed);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_Should_Return_Forbidden_When_Redirect_Limit_Is_Exceeded()
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxRedirects = 0;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, _server.Url("loopback.test", "/chain/1"));

        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be(SsrfBlockReason.RedirectLimitExceeded);
    }

    [Fact]
    public async Task ReadAsStringAsResultAsync_Should_Return_Forbidden_When_Body_Exceeds_Limit()
    {
        HttpClient client = CreateClient(o =>
        {
            o.MaxResponseContentLength = 4096;
            o.AllowedNetworks.Add(LoopbackNetwork);
        });
        using HttpResponseMessage response = await client.GetAsync(
            _server.Url("loopback.test", "/big?length=100000&declare=false"),
            HttpCompletionOption.ResponseHeadersRead);

        Result<string> result = await response.Content.ReadAsStringAsResultAsync();

        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Metadata.Should().ContainKey("reason").WhoseValue.Should().Be(SsrfBlockReason.ResponseTooLarge);
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_Should_Return_Unexpected_When_Failure_Is_Not_Ssrf()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        Result<string> result = await client.GetFromJsonAsResultAsync<string>(_server.Url("unknown.test", "/json"));

        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_Should_Return_Success_When_Allowed()
    {
        HttpClient client = CreateClient(o => o.AllowedNetworks.Add(LoopbackNetwork));

        Result<string> result = await client.GetFromJsonAsResultAsync<string>(_server.Url("loopback.test", "/json"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    private HttpClient CreateClient(
        Action<SsrfGuardOptions>? configure = null,
        Action<IServiceCollection>? services = null,
        Action<IHttpClientBuilder>? configureBuilder = null)
    {
        var collection = new ServiceCollection();
        services?.Invoke(collection);
        IHttpClientBuilder builder = collection.AddHttpClient("guarded").AddSsrfGuard(o =>
        {
            ConfigureForServer(o);
            configure?.Invoke(o);
        });
        configureBuilder?.Invoke(builder);
        ServiceProvider provider = collection.BuildServiceProvider();
        _providers.Add(provider);
        return provider.GetRequiredService<IHttpClientFactory>().CreateClient("guarded");
    }

    private void ConfigureForServer(SsrfGuardOptions options)
    {
        options.AllowHttp = true;
        options.AllowedPorts.Add(_server.Port);
        options.ResolveHostAsync = ResolveAsync;
    }

    private static HttpRequestMessage CreateCredentialedRequest(Uri uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
        request.Headers.ProxyAuthorization = new AuthenticationHeaderValue("Basic", "proxy");
        request.Headers.Add("Cookie", "session=abc");
        return request;
    }

    private Uri RedirectUrl(Uri target, int status = 302) =>
        _server.Url("loopback.test", $"/redirect?status={status}&to={Uri.EscapeDataString(target.OriginalString)}");

    private static SocketsHttpHandler GetSocketsHandler(HttpMessageHandler handler) =>
        (SocketsHttpHandler)((DelegatingHandler)handler).InnerHandler!;

    private static async Task<SsrfBlockedException> AssertBlockedAsync(Func<Task> act) =>
        (await act.Should().ThrowAsync<SsrfBlockedException>()).Which;

    private static Task<IPAddress[]> ResolveAsync(string host, CancellationToken cancellationToken) =>
        FakeDns.TryGetValue(host, out IPAddress[]? addresses)
            ? Task.FromResult(addresses)
            : Task.FromException<IPAddress[]>(new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.HostNotFound));

    private sealed class AllowLoopbackPolicy : IOutboundAddressPolicy
    {
        public bool IsAllowed(IPAddress address, Uri requestUri) =>
            IPAddress.IsLoopback(address) || DefaultOutboundAddressPolicy.Instance.IsAllowed(address, requestUri);
    }

    private sealed class DenyPathPolicy : IOutboundRequestPolicy
    {
        public bool IsAllowed(Uri requestUri) => requestUri.AbsolutePath != "/ok";
    }

    private sealed class AllowAllRequestPolicy : IOutboundRequestPolicy
    {
        public bool IsAllowed(Uri requestUri) => true;
    }

    private sealed class RejectLoopbackPolicy : IOutboundAddressPolicy
    {
        public bool IsAllowed(IPAddress address, Uri requestUri) => !IPAddress.IsLoopback(address);
    }

    private sealed class RecordingAddressPolicy(Func<IPAddress, bool>? allow = null) : IOutboundAddressPolicy
    {
        public List<IPAddress> Addresses { get; } = [];

        public bool IsAllowed(IPAddress address, Uri requestUri)
        {
            Addresses.Add(address);
            return allow?.Invoke(address) ?? false;
        }
    }

    private sealed class RewritingHandler(Action<HttpRequestMessage> rewrite) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            rewrite(request);
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class VersionRecordingHandler : DelegatingHandler
    {
        public Version? Version { get; private set; }

        public HttpVersionPolicy? VersionPolicy { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Version = request.Version;
            VersionPolicy = request.VersionPolicy;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
