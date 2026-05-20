```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 166.2 ns | 0.41 ns | 0.34 ns | 0.0772 |     648 B |
| FV-Conditional-Business  | 261.2 ns | 2.32 ns | 2.17 ns | 0.0830 |     696 B |
| CSE-Conditional-Personal | 131.7 ns | 0.96 ns | 0.85 ns | 0.0582 |     488 B |
| FV-Conditional-Personal  | 232.3 ns | 1.39 ns | 1.16 ns | 0.0792 |     664 B |
