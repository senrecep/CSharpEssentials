```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean     | Error    | StdDev   | Gen0   | Allocated |
|--------------------- |---------:|---------:|---------:|-------:|----------:|
| CSE-ExclusiveBetween | 41.80 ns | 0.362 ns | 0.321 ns | 0.0287 |     240 B |
| FV-ExclusiveBetween  | 86.60 ns | 0.434 ns | 0.339 ns | 0.0716 |     600 B |
