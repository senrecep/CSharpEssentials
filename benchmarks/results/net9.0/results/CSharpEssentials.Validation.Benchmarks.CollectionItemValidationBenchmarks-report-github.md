```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method             | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------------------- |-----------:|--------:|--------:|-------:|----------:|
| CSE-CollectionItem |   345.7 ns | 0.79 ns | 0.62 ns | 0.2027 |   1.66 KB |
| FV-CollectionItem  | 1,065.6 ns | 8.39 ns | 7.00 ns | 0.3433 |   2.81 KB |
