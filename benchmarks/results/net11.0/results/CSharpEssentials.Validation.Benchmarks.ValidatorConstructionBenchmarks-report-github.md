```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD
  default : .NET 11.0.0 (11.0.26.20806), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method   | Mean         | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------- |-------------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Ctor |     2.473 ns | 0.0080 ns | 0.0067 ns | 0.0029 |      - |      24 B |
| FV-Ctor  | 1,612.950 ns | 8.5835 ns | 7.6090 ns | 1.1139 | 0.0153 |    9432 B |
