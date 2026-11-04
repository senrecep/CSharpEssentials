namespace BclAot;

public static class Program
{
    public static int Main()
    {
        // https://learn.microsoft.com/dotnet/api/system.enum.getvalues
        // https://learn.microsoft.com/dotnet/api/system.enum.tostring
        // https://learn.microsoft.com/dotnet/api/system.enum.tryparse
        var values = Enum.GetValues<Status>();
        int checksum = 0;
        int parsed = 0;
        foreach (Status value in values)
        {
            string name = value.ToString();
            if (Enum.TryParse(name, ignoreCase: false, out Status back) && back == value)
            {
                parsed++;
                checksum += (int)back + name.Length;
            }
        }

        bool ok = parsed == 20 && checksum == 971;
        Console.WriteLine($"parsed {parsed} checksum {checksum} {(ok ? "ok" : "MISMATCH")}");
        return ok ? 0 : 1;
    }
}
