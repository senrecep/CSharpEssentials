```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=NextDayOfWeekDateTime  

```
| Method                         | Mean     | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------------------- |---------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials_NextDayOfWeek | 4.627 ns | 0.0035 ns | 0.0033 ns |  1.00 |         - |          NA |
| HandWritten_NextDayOfWeek      | 5.601 ns | 0.0035 ns | 0.0032 ns |  1.21 |         - |          NA |
