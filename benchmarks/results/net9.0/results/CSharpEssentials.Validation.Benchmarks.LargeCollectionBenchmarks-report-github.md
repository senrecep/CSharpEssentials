```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean      | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|----------:|
| CSE-LargeCollection |  7.146 μs | 0.0274 μs | 0.0214 μs | 4.2725 |  34.91 KB |
| FV-LargeCollection  | 21.622 μs | 0.1310 μs | 0.1094 μs | 5.7068 |  46.63 KB |
