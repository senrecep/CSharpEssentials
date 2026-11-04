using EnumsNET;

namespace EnumsNetAot;

public static class Program
{
    public static int Main()
    {
        // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/README.md
        var values = Enums.GetValues<Status>();
        int checksum = 0;
        int parsed = 0;
        foreach (Status value in values)
        {
            string name = value.AsString();
            if (Enums.TryParse(name, ignoreCase: false, out Status back) && back == value)
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
