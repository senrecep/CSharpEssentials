```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                 | Categories                | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|--------------------------------------- |-------------------------- |----------:|----------:|----------:|------:|----------:|------------:|
| CSharpEssentials_GetAge                | GetAge                    | 11.286 ns | 0.0049 ns | 0.0043 ns |  1.00 |         - |          NA |
| HandWritten_GetAge                     | GetAge                    | 11.210 ns | 0.0049 ns | 0.0043 ns |  0.99 |         - |          NA |
|                                        |                           |           |           |           |       |           |             |
| CSharpEssentials_NextDayOfWeekDate     | NextDayOfWeekDateOnly     |  2.245 ns | 0.0010 ns | 0.0010 ns |  1.00 |         - |          NA |
| HandWritten_NextDayOfWeekDate          | NextDayOfWeekDateOnly     |  2.127 ns | 0.0012 ns | 0.0011 ns |  0.95 |         - |          NA |
|                                        |                           |           |           |           |       |           |             |
| CSharpEssentials_NextDayOfWeek         | NextDayOfWeekDateTime     |  5.387 ns | 0.0108 ns | 0.0090 ns |  1.00 |         - |          NA |
| HandWritten_NextDayOfWeek              | NextDayOfWeekDateTime     |  5.386 ns | 0.0110 ns | 0.0092 ns |  1.00 |         - |          NA |
|                                        |                           |           |           |           |       |           |             |
| CSharpEssentials_PreviousDayOfWeekDate | PreviousDayOfWeekDateOnly |  2.150 ns | 0.0009 ns | 0.0007 ns |  1.00 |         - |          NA |
| HandWritten_PreviousDayOfWeekDate      | PreviousDayOfWeekDateOnly |  2.151 ns | 0.0039 ns | 0.0033 ns |  1.00 |         - |          NA |
|                                        |                           |           |           |           |       |           |             |
| CSharpEssentials_ToDateOnly            | ToDateOnly                |  1.022 ns | 0.0010 ns | 0.0008 ns |  1.00 |         - |          NA |
| Bcl_DateOnlyFromDateTime               | ToDateOnly                |  1.021 ns | 0.0002 ns | 0.0001 ns |  1.00 |         - |          NA |
|                                        |                           |           |           |           |       |           |             |
| CSharpEssentials_ToTimeOnly            | ToTimeOnly                |  1.040 ns | 0.0004 ns | 0.0003 ns |  1.00 |         - |          NA |
| Bcl_TimeOnlyFromDateTime               | ToTimeOnly                |  1.135 ns | 0.0049 ns | 0.0046 ns |  1.09 |         - |          NA |
