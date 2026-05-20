```

BenchmarkDotNet v0.14.0, macOS 26.3.1 (a) (25D771280a) [Darwin 25.3.0]
Apple M3 Pro, 1 CPU, 12 logical and 12 physical cores
.NET SDK 11.0.100-preview.3.26207.106
  [Host]  : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD
  default : .NET 9.0.4 (9.0.425.16305), Arm64 RyuJIT AdvSIMD

Job=default  

```
| Method       | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------- |----------:|---------:|---------:|-------:|----------:|
| CSE-Equality |  90.28 ns | 0.415 ns | 0.368 ns | 0.0554 |     464 B |
| FV-Equality  | 193.32 ns | 1.112 ns | 0.929 ns | 0.0753 |     632 B |
