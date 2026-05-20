```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method             | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| CSE-CollectionItem | 286.5 ns | 1.77 ns | 1.48 ns | 0.1488 |      - |   1.22 KB |
| FV-CollectionItem  | 890.4 ns | 8.40 ns | 7.86 ns | 0.3443 | 0.0010 |   2.81 KB |
