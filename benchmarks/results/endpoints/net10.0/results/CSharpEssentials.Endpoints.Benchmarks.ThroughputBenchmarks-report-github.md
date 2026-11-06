```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method      | Variant              | Mean      | Error     | StdDev    | Exceptions | Gen0   | Gen1   | Allocated |
|------------ |--------------------- |----------:|----------:|----------:|-----------:|-------:|-------:|----------:|
| **GetById**     | **Plain**                |  **8.723 μs** | **0.1723 μs** | **0.1612 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | Plain                | 16.744 μs | 0.3262 μs | 0.5540 μs |          - | 0.4883 |      - |  11.66 KB |
| PostInvalid | Plain                | 23.880 μs | 0.4726 μs | 0.8025 μs |          - | 0.7324 |      - |  13.31 KB |
| Delete      | Plain                |  6.727 μs | 0.0907 μs | 0.0804 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **CseGenerated**         |  **8.855 μs** | **0.1470 μs** | **0.1444 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | CseGenerated         | 17.325 μs | 0.3253 μs | 0.6497 μs |          - | 0.4883 |      - |  11.67 KB |
| PostInvalid | CseGenerated         | 24.369 μs | 0.4776 μs | 0.5500 μs |          - | 0.7324 |      - |  13.31 KB |
| Delete      | CseGenerated         |  6.728 μs | 0.1201 μs | 0.1003 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **CseReflection**        |  **9.187 μs** | **0.1769 μs** | **0.2300 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | CseReflection        | 17.052 μs | 0.3376 μs | 0.6173 μs |          - | 0.4883 |      - |  11.67 KB |
| PostInvalid | CseReflection        | 24.331 μs | 0.4830 μs | 0.6611 μs |          - | 0.7324 |      - |   13.3 KB |
| Delete      | CseReflection        |  6.856 μs | 0.1284 μs | 0.1479 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **Carter**               |  **8.907 μs** | **0.1435 μs** | **0.1342 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | Carter               | 15.620 μs | 0.3065 μs | 0.5526 μs |          - | 0.4883 |      - |  11.38 KB |
| PostInvalid | Carter               | 32.138 μs | 0.5094 μs | 0.4516 μs |          - | 1.2207 |      - |  21.33 KB |
| Delete      | Carter               |  6.801 μs | 0.0946 μs | 0.0839 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **FastEndpoints**        | **11.165 μs** | **0.1996 μs** | **0.2050 μs** |          **-** | **0.6104** | **0.6104** |  **10.07 KB** |
| PostValid   | FastEndpoints        | 21.474 μs | 0.4269 μs | 0.9809 μs |          - | 0.7324 |      - |  12.67 KB |
| PostInvalid | FastEndpoints        | 50.133 μs | 0.9701 μs | 0.9075 μs |     2.0000 | 1.2207 |      - |  23.18 KB |
| Delete      | FastEndpoints        |  8.530 μs | 0.1265 μs | 0.1184 μs |          - | 0.5493 |      - |   9.08 KB |
| **GetById**     | **FastE(...)ceGen [22]** | **11.559 μs** | **0.2296 μs** | **0.2456 μs** |          **-** | **0.6104** |      **-** |  **10.07 KB** |
| PostValid   | FastE(...)ceGen [22] | 22.755 μs | 0.4522 μs | 0.7430 μs |          - | 0.7324 |      - |  12.68 KB |
| PostInvalid | FastE(...)ceGen [22] | 50.464 μs | 0.7821 μs | 0.7316 μs |     2.0000 | 1.2207 |      - |  23.18 KB |
| Delete      | FastE(...)ceGen [22] |  8.612 μs | 0.1668 μs | 0.1854 μs |          - | 0.5493 |      - |   9.05 KB |
