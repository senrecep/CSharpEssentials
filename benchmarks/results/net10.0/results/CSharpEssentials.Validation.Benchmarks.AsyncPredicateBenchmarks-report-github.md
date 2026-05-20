```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method        | Mean     | Error     | StdDev    | Gen0   | Allocated |
|-------------- |---------:|----------:|----------:|-------:|----------:|
| CSE-MustAsync | 1.092 μs | 0.0247 μs | 0.0667 μs | 0.1011 |     848 B |
| FV-MustAsync  | 1.142 μs | 0.0544 μs | 0.1542 μs | 0.1755 |    1472 B |
