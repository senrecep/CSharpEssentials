namespace CseAot;

public static class Program
{
    public static int Main()
    {
        // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
        var values = StatusExtensions.GetValues();
        int checksum = 0;
        int parsed = 0;
        foreach (Status value in values)
        {
            string name = value.ToOptimizedString();
            if (StatusExtensions.TryParseWire(name, out Status back) && back == value)
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
