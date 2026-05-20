```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method             | Mean       | Error    | StdDev  | Gen0   | Gen1   | Allocated |
|------------------- |-----------:|---------:|--------:|-------:|-------:|----------:|
| CSE-Simple-Valid   |   176.1 ns |  0.54 ns | 0.45 ns | 0.0725 |      - |     608 B |
| FV-Simple-Valid    |   321.7 ns |  1.97 ns | 1.74 ns | 0.0839 |      - |     704 B |
| CSE-Simple-Invalid |   257.1 ns |  0.64 ns | 0.60 ns | 0.1845 | 0.0005 |    1544 B |
| FV-Simple-Invalid  | 2,108.1 ns | 11.45 ns | 9.57 ns | 0.8659 | 0.0076 |    7264 B |
