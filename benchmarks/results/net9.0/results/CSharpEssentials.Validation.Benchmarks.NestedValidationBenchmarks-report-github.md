```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method     | Mean     | Error   | StdDev  | Gen0   | Allocated |
|----------- |---------:|--------:|--------:|-------:|----------:|
| CSE-Nested | 177.8 ns | 1.30 ns | 1.15 ns | 0.0851 |     712 B |
| FV-Nested  | 440.2 ns | 5.90 ns | 5.52 ns | 0.1516 |    1272 B |
