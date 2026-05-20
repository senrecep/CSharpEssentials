```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|----------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| CSE-Cascade-Stop | 114.6 ns | 0.67 ns | 0.56 ns | 0.0937 |      - |     784 B |
| FV-Cascade-Stop  | 823.3 ns | 7.35 ns | 6.87 ns | 0.4091 | 0.0019 |    3424 B |
