```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method         | Mean     | Error   | StdDev  | Gen0   | Allocated |
|--------------- |---------:|--------:|--------:|-------:|----------:|
| CSE-DeepNested | 304.6 ns | 0.51 ns | 0.40 ns | 0.1497 |   1.23 KB |
| FV-DeepNested  | 853.9 ns | 8.88 ns | 8.30 ns | 0.2489 |   2.04 KB |
