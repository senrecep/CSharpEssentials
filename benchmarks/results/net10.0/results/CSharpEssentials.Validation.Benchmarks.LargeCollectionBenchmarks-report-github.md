```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-LargeCollection |  5.811 μs | 0.0104 μs | 0.0081 μs | 3.1128 |      - |  25.47 KB |
| FV-LargeCollection  | 18.024 μs | 0.0731 μs | 0.0610 μs | 5.7068 | 0.0305 |  46.63 KB |
