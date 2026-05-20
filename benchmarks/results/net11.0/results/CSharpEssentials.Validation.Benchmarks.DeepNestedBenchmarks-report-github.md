```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method         | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|--------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| CSE-DeepNested | 247.1 ns | 0.41 ns | 0.34 ns | 0.1040 |      - |     872 B |
| FV-DeepNested  | 703.5 ns | 1.89 ns | 1.58 ns | 0.2489 | 0.0010 |    2088 B |
