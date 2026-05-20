```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method     | Mean     | Error   | StdDev  | Gen0   | Allocated |
|----------- |---------:|--------:|--------:|-------:|----------:|
| CSE-String | 151.8 ns | 0.35 ns | 0.29 ns | 0.0467 |     392 B |
| FV-String  | 265.4 ns | 1.75 ns | 1.46 ns | 0.0792 |     664 B |
