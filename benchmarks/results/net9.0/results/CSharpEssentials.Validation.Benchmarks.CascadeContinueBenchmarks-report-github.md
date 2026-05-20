```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Cascade-Continue |   212.8 ns |  1.40 ns |  1.24 ns | 0.1395 | 0.0002 |   1.14 KB |
| FV-Cascade-Continue  | 1,675.6 ns | 16.04 ns | 14.22 ns | 0.7229 | 0.0057 |   5.91 KB |
