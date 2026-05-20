```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Cascade-Continue |   195.0 ns |  1.46 ns |  1.29 ns | 0.1318 | 0.0002 |   1.08 KB |
| FV-Cascade-Continue  | 1,481.1 ns | 11.51 ns | 10.20 ns | 0.7000 | 0.0057 |   5.72 KB |
