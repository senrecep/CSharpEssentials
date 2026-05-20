```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method   | Mean         | Error      | StdDev    | Gen0   | Gen1   | Allocated |
|--------- |-------------:|-----------:|----------:|-------:|-------:|----------:|
| CSE-Ctor |     2.449 ns |  0.0149 ns | 0.0124 ns | 0.0029 |      - |      24 B |
| FV-Ctor  | 1,938.622 ns | 10.2142 ns | 9.0546 ns | 1.1444 | 0.0153 |    9624 B |
