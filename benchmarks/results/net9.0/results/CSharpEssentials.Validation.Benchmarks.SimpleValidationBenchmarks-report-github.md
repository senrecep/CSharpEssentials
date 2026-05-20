```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method             | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Simple-Valid   |   206.2 ns |  2.15 ns |  2.01 ns | 0.0880 |      - |     736 B |
| FV-Simple-Valid    |   362.1 ns |  2.25 ns |  1.99 ns | 0.0839 |      - |     704 B |
| CSE-Simple-Invalid |   292.3 ns |  1.44 ns |  1.20 ns | 0.1922 |      - |    1608 B |
| FV-Simple-Invalid  | 2,413.8 ns | 21.10 ns | 18.71 ns | 0.9041 | 0.0076 |    7584 B |
