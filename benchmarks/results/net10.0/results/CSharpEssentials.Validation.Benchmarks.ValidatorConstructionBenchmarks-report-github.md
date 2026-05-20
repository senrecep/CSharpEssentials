```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD
  default : .NET 10.0.7 (10.0.726.21808), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method   | Mean         | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|--------- |-------------:|----------:|----------:|-------:|-------:|----------:|
| CSE-Ctor |     2.186 ns | 0.0046 ns | 0.0041 ns | 0.0029 |      - |      24 B |
| FV-Ctor  | 1,652.300 ns | 5.3076 ns | 4.9647 ns | 1.0986 | 0.0153 |    9272 B |
