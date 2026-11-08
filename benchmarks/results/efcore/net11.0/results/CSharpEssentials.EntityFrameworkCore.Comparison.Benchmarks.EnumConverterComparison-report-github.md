```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 73.32 ns | 0.032 ns | 0.030 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 68.40 ns | 0.059 ns | 0.053 ns |  0.93 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 56.66 ns | 0.078 ns | 0.073 ns |  0.77 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 53.03 ns | 0.049 ns | 0.046 ns |  0.72 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 46.11 ns | 0.019 ns | 0.017 ns |  0.63 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 72.86 ns | 0.067 ns | 0.062 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 55.13 ns | 0.108 ns | 0.101 ns |  0.76 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 60.69 ns | 0.201 ns | 0.188 ns |  0.83 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 61.79 ns | 0.534 ns | 0.473 ns |  0.85 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 49.55 ns | 0.331 ns | 0.276 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 35.32 ns | 0.040 ns | 0.033 ns |  0.71 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 30.65 ns | 0.037 ns | 0.035 ns |  0.62 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 55.62 ns | 0.025 ns | 0.023 ns |  1.12 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 26.49 ns | 0.013 ns | 0.012 ns |  0.53 |      - |         - |        0.00 |
