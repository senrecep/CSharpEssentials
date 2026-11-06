```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                                      | Categories | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------------------ |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Ef_EnumToStringConverter_Read                               | Read       | 91.20 ns | 0.028 ns | 0.024 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_EnumWireNameConverter_Read                 | Read       | 91.65 ns | 0.051 ns | 0.043 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_Read                  | Read       | 92.63 ns | 0.270 ns | 0.210 ns |  1.02 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_Read        | Read       | 87.68 ns | 0.104 ns | 0.082 ns |  0.96 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_Read          | Read       | 50.19 ns | 0.049 ns | 0.046 ns |  0.55 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| CSharpEssentials_EnumWireNameConverter_LegacyInput          | ReadLegacy | 92.40 ns | 0.074 ns | 0.065 ns |  1.00 |      - |         - |          NA |
| CSharpEssentials_HasLegacyEnumStorage_LegacyInput           | ReadLegacy | 86.70 ns | 0.313 ns | 0.262 ns |  0.94 |      - |         - |          NA |
| CSharpEssentials_EnumToFormattedStringConverter_LegacyInput | ReadLegacy | 87.16 ns | 0.030 ns | 0.027 ns |  0.94 |      - |         - |          NA |
| CSharpEssentials_LegacySnakeCaseEnumConverter_LegacyInput   | ReadLegacy | 64.82 ns | 0.306 ns | 0.287 ns |  0.70 |      - |         - |          NA |
|                                                             |            |          |          |          |       |        |           |             |
| Ef_EnumToStringConverter                                    | Write      | 60.53 ns | 0.272 ns | 0.241 ns |  1.00 | 0.0072 |     120 B |        1.00 |
| CSharpEssentials_EnumWireNameConverter                      | Write      | 53.39 ns | 0.036 ns | 0.031 ns |  0.88 |      - |         - |        0.00 |
| CSharpEssentials_HasLegacyEnumStorage                       | Write      | 31.33 ns | 0.108 ns | 0.096 ns |  0.52 |      - |         - |        0.00 |
| CSharpEssentials_EnumToFormattedStringConverter             | Write      | 47.25 ns | 0.033 ns | 0.031 ns |  0.78 |      - |         - |        0.00 |
| CSharpEssentials_LegacySnakeCaseEnumConverter               | Write      | 21.65 ns | 0.005 ns | 0.005 ns |  0.36 |      - |         - |        0.00 |
