```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| CSE-LargeCollection |  5.703 μs | 0.0116 μs | 0.0097 μs | 3.1128 |      - |  25.47 KB |
| FV-LargeCollection  | 17.690 μs | 0.1242 μs | 0.1037 μs | 5.7068 | 0.0305 |  46.63 KB |
