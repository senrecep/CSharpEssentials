```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                 | Categories                | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------- |-------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials_GetAge                | GetAge                    | 19.015 ns | 0.1191 ns | 0.1114 ns |  1.00 |    0.01 |         - |          NA |
| HandWritten_GetAge                     | GetAge                    | 18.717 ns | 0.1305 ns | 0.1221 ns |  0.98 |    0.01 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_NextDayOfWeekDate     | NextDayOfWeekDateOnly     |  2.200 ns | 0.0264 ns | 0.0247 ns |  1.00 |    0.02 |         - |          NA |
| HandWritten_NextDayOfWeekDate          | NextDayOfWeekDateOnly     |  2.243 ns | 0.0010 ns | 0.0008 ns |  1.02 |    0.01 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_NextDayOfWeek         | NextDayOfWeekDateTime     |  6.377 ns | 0.0023 ns | 0.0020 ns |  1.00 |    0.00 |         - |          NA |
| HandWritten_NextDayOfWeek              | NextDayOfWeekDateTime     |  7.098 ns | 0.0188 ns | 0.0176 ns |  1.11 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_PreviousDayOfWeekDate | PreviousDayOfWeekDateOnly |  2.368 ns | 0.0008 ns | 0.0007 ns |  1.00 |    0.00 |         - |          NA |
| HandWritten_PreviousDayOfWeekDate      | PreviousDayOfWeekDateOnly |  2.212 ns | 0.0008 ns | 0.0007 ns |  0.93 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_ToDateOnly            | ToDateOnly                |  1.022 ns | 0.0006 ns | 0.0005 ns |  1.00 |    0.00 |         - |          NA |
| Bcl_DateOnlyFromDateTime               | ToDateOnly                |  1.021 ns | 0.0004 ns | 0.0003 ns |  1.00 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_ToTimeOnly            | ToTimeOnly                |  1.088 ns | 0.0032 ns | 0.0030 ns |  1.00 |    0.00 |         - |          NA |
| Bcl_TimeOnlyFromDateTime               | ToTimeOnly                |  1.194 ns | 0.0005 ns | 0.0004 ns |  1.10 |    0.00 |         - |          NA |
