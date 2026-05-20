```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method                   | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------------------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Conditional-Business | 143.3 ns | 0.35 ns | 0.27 ns | 0.0620 |     520 B |
| FV-Conditional-Business  | 219.8 ns | 1.97 ns | 1.84 ns | 0.0832 |     696 B |
| CSE-Conditional-Personal | 113.5 ns | 0.84 ns | 0.70 ns | 0.0430 |     360 B |
| FV-Conditional-Personal  | 195.8 ns | 2.96 ns | 2.77 ns | 0.0792 |     664 B |
