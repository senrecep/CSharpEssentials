```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method     | Mean     | Error   | StdDev  | Gen0   | Allocated |
|----------- |---------:|--------:|--------:|-------:|----------:|
| CSE-String | 159.5 ns | 0.97 ns | 0.81 ns | 0.0467 |     392 B |
| FV-String  | 259.1 ns | 0.72 ns | 0.60 ns | 0.0792 |     664 B |
