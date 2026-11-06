```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                 | Categories                | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------- |-------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| CSharpEssentials_GetAge                | GetAge                    | 11.368 ns | 0.0025 ns | 0.0021 ns |  1.00 |    0.00 |         - |          NA |
| HandWritten_GetAge                     | GetAge                    | 11.494 ns | 0.0026 ns | 0.0021 ns |  1.01 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_NextDayOfWeekDate     | NextDayOfWeekDateOnly     |  2.126 ns | 0.0027 ns | 0.0022 ns |  1.00 |    0.00 |         - |          NA |
| HandWritten_NextDayOfWeekDate          | NextDayOfWeekDateOnly     |  2.143 ns | 0.0084 ns | 0.0079 ns |  1.01 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_NextDayOfWeek         | NextDayOfWeekDateTime     |  5.641 ns | 0.0065 ns | 0.0057 ns |  1.00 |    0.00 |         - |          NA |
| HandWritten_NextDayOfWeek              | NextDayOfWeekDateTime     |  6.322 ns | 0.0240 ns | 0.0224 ns |  1.12 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_PreviousDayOfWeekDate | PreviousDayOfWeekDateOnly |  2.191 ns | 0.0116 ns | 0.0108 ns |  1.00 |    0.01 |         - |          NA |
| HandWritten_PreviousDayOfWeekDate      | PreviousDayOfWeekDateOnly |  2.171 ns | 0.0366 ns | 0.0342 ns |  0.99 |    0.02 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_ToDateOnly            | ToDateOnly                |  1.022 ns | 0.0005 ns | 0.0005 ns |  1.00 |    0.00 |         - |          NA |
| Bcl_DateOnlyFromDateTime               | ToDateOnly                |  1.022 ns | 0.0001 ns | 0.0001 ns |  1.00 |    0.00 |         - |          NA |
|                                        |                           |           |           |           |       |         |           |             |
| CSharpEssentials_ToTimeOnly            | ToTimeOnly                |  1.044 ns | 0.0003 ns | 0.0003 ns |  1.00 |    0.00 |         - |          NA |
| Bcl_TimeOnlyFromDateTime               | ToTimeOnly                |  1.039 ns | 0.0011 ns | 0.0010 ns |  1.00 |    0.00 |         - |          NA |
