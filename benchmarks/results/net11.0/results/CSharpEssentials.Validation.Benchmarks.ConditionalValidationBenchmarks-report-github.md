```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 149.8 ns | 0.61 ns | 0.51 ns | 0.0620 |     520 B |
| FV-Conditional-Business  | 211.3 ns | 2.54 ns | 2.25 ns | 0.0832 |     696 B |
| CSE-Conditional-Personal | 119.1 ns | 0.43 ns | 0.36 ns | 0.0429 |     360 B |
| FV-Conditional-Personal  | 187.6 ns | 0.52 ns | 0.40 ns | 0.0792 |     664 B |
