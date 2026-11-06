```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 73.16 ns | 0.028 ns | 0.023 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 81.07 ns | 0.075 ns | 0.070 ns |  1.11 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 86.92 ns | 0.037 ns | 0.032 ns |  1.19 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 72.45 ns | 0.124 ns | 0.097 ns |  0.99 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 46.45 ns | 0.340 ns | 0.301 ns |  0.63 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 82.17 ns | 0.059 ns | 0.055 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 86.72 ns | 0.157 ns | 0.122 ns |  1.06 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 72.35 ns | 0.059 ns | 0.049 ns |  0.88 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 61.07 ns | 0.164 ns | 0.153 ns |  0.74 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 52.89 ns | 0.416 ns | 0.389 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 35.72 ns | 0.027 ns | 0.023 ns |  0.68 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 30.36 ns | 0.031 ns | 0.026 ns |  0.57 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 54.25 ns | 0.036 ns | 0.030 ns |  1.03 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 26.54 ns | 0.017 ns | 0.014 ns |  0.50 |      - |         - |        0.00 |
