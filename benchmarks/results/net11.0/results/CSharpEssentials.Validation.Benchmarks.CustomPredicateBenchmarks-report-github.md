```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method           | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|----------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| CSE-Must-Valid   |  39.35 ns | 0.471 ns | 0.418 ns | 0.0296 |      - |     248 B |
| FV-Must-Valid    |  86.95 ns | 0.373 ns | 0.312 ns | 0.0716 |      - |     600 B |
| CSE-Must-Invalid |  60.52 ns | 0.550 ns | 0.487 ns | 0.0526 |      - |     440 B |
| FV-Must-Invalid  | 180.35 ns | 1.677 ns | 1.568 ns | 0.1204 | 0.0002 |    1008 B |
