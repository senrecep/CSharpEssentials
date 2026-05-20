```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| CSE-Cascade-Stop | 113.6 ns | 0.61 ns | 0.51 ns | 0.0861 | 0.0001 |     720 B |
| FV-Cascade-Stop  | 696.3 ns | 4.62 ns | 4.10 ns | 0.4015 | 0.0029 |    3360 B |
