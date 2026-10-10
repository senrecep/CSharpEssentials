using System.Diagnostics;

namespace CSharpEssentials.Tests;

internal static class ChildProcessScenario
{
    private const int TimeoutMilliseconds = 120_000;

    public static void Run(string scenario)
    {
        string assemblyPath = typeof(ChildProcessScenario).Assembly.Location;
        string host = Environment.ProcessPath ?? "dotnet";
        var startInfo = new ProcessStartInfo(host)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            startInfo.ArgumentList.Add(assemblyPath);
        startInfo.ArgumentList.Add(scenario);

        using var process = Process.Start(startInfo)!;
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(TimeoutMilliseconds))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            Assert.Fail($"Scenario '{scenario}' did not finish within {TimeoutMilliseconds} ms.");
        }

        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"Scenario '{scenario}' exited with code {process.ExitCode}.{Environment.NewLine}{Truncate(output.Result)}{Environment.NewLine}{Truncate(error.Result)}");
    }

    public static int Execute(string[] args)
    {
        if (args.Length != 1)
            return 0;

        Action? scenario = OversizedInputScenarios.Find(args[0]);
        if (scenario is null)
        {
            Console.Error.WriteLine($"Unknown scenario '{args[0]}'.");
            return 2;
        }

        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { scenario(); }
            catch (Exception ex) { failure = ex; }
        }, OversizedInputScenarios.StackSizeBytes);
        thread.Start();
        thread.Join();

        if (failure is null)
            return 0;

        Console.Error.WriteLine(failure);
        return 1;
    }

    private static string Truncate(string value) => value.Length <= 4000 ? value : value[..4000];
}
