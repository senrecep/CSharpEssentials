```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| CSE-Cascade-Stop | 112.3 ns | 0.91 ns | 0.81 ns | 0.0861 | 0.0001 |     720 B |
| FV-Cascade-Stop  | 691.9 ns | 5.22 ns | 4.36 ns | 0.4015 | 0.0029 |    3360 B |
