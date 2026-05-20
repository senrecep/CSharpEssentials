```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Nullable-Valid   |  69.54 ns | 0.639 ns | 0.567 ns | 0.0411 |      - |     344 B |
| FV-Nullable-Valid    | 206.84 ns | 1.414 ns | 1.254 ns | 0.0715 |      - |     600 B |
| CSE-Nullable-Invalid | 118.61 ns | 1.471 ns | 1.376 ns | 0.1023 |      - |     856 B |
| FV-Nullable-Invalid  | 859.12 ns | 6.690 ns | 5.931 ns | 0.3405 | 0.0019 |    2848 B |
