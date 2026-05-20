```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Nullable-Valid   |  68.70 ns | 0.212 ns | 0.177 ns | 0.0411 |      - |     344 B |
| FV-Nullable-Valid    | 205.33 ns | 0.692 ns | 0.613 ns | 0.0715 |      - |     600 B |
| CSE-Nullable-Invalid | 115.05 ns | 1.113 ns | 1.041 ns | 0.1023 | 0.0001 |     856 B |
| FV-Nullable-Invalid  | 813.24 ns | 3.978 ns | 3.322 ns | 0.3405 | 0.0019 |    2848 B |
