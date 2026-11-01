```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method      | Variant              | Mean      | Error     | StdDev    | Exceptions | Gen0   | Gen1   | Allocated |
|------------ |--------------------- |----------:|----------:|----------:|-----------:|-------:|-------:|----------:|
| **GetById**     | **Plain**                |  **8.297 μs** | **0.1309 μs** | **0.1161 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | Plain                | 15.091 μs | 0.2724 μs | 0.4242 μs |          - | 0.4883 |      - |  11.66 KB |
| PostInvalid | Plain                | 19.757 μs | 0.3922 μs | 0.8608 μs |          - | 0.7324 |      - |  13.28 KB |
| Delete      | Plain                |  6.321 μs | 0.0822 μs | 0.0807 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **CseGenerated**         |  **8.227 μs** | **0.0924 μs** | **0.0819 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | CseGenerated         | 14.729 μs | 0.1639 μs | 0.1453 μs |          - | 0.4883 |      - |  11.66 KB |
| PostInvalid | CseGenerated         | 22.027 μs | 0.4388 μs | 1.1406 μs |          - | 0.7324 |      - |   13.3 KB |
| Delete      | CseGenerated         |  6.276 μs | 0.1221 μs | 0.1082 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **CseReflection**        |  **7.962 μs** | **0.0689 μs** | **0.0576 μs** |          **-** | **0.4883** |      **-** |   **8.69 KB** |
| PostValid   | CseReflection        | 14.449 μs | 0.2563 μs | 0.2140 μs |          - | 0.4883 |      - |  11.66 KB |
| PostInvalid | CseReflection        | 21.412 μs | 0.4274 μs | 0.9015 μs |          - | 0.7324 |      - |   13.3 KB |
| Delete      | CseReflection        |  6.640 μs | 0.1314 μs | 0.1165 μs |          - | 0.4272 |      - |   7.82 KB |
| **GetById**     | **Carter**               |  **9.023 μs** | **0.1588 μs** | **0.1485 μs** |          **-** | **0.4883** | **0.4883** |   **8.69 KB** |
| PostValid   | Carter               | 14.141 μs | 0.2663 μs | 0.2491 μs |          - | 0.4883 |      - |  11.38 KB |
| PostInvalid | Carter               | 30.252 μs | 0.6011 μs | 1.3689 μs |          - | 1.2207 | 1.2207 |  21.33 KB |
| Delete      | Carter               |  6.370 μs | 0.0642 μs | 0.0569 μs |          - | 0.4578 |      - |   7.82 KB |
| **GetById**     | **FastEndpoints**        | **10.126 μs** | **0.0826 μs** | **0.0732 μs** |          **-** | **0.6104** | **0.6104** |  **10.06 KB** |
| PostValid   | FastEndpoints        | 20.148 μs | 0.4051 μs | 1.1945 μs |          - | 0.7324 |      - |  12.67 KB |
| PostInvalid | FastEndpoints        | 46.300 μs | 0.3571 μs | 0.3166 μs |     2.0000 | 1.2207 |      - |  23.18 KB |
| Delete      | FastEndpoints        |  7.869 μs | 0.1009 μs | 0.0843 μs |          - | 0.5493 |      - |   9.08 KB |
| **GetById**     | **FastE(...)ceGen [22]** | **10.455 μs** | **0.1324 μs** | **0.1173 μs** |          **-** | **0.6104** | **0.6104** |  **10.06 KB** |
| PostValid   | FastE(...)ceGen [22] | 20.838 μs | 0.4161 μs | 1.0887 μs |          - | 0.7324 | 0.7324 |  12.67 KB |
| PostInvalid | FastE(...)ceGen [22] | 46.420 μs | 0.6268 μs | 0.5863 μs |     2.0000 | 1.2207 |      - |  23.18 KB |
| Delete      | FastE(...)ceGen [22] |  7.633 μs | 0.1364 μs | 0.1209 μs |          - | 0.5493 |      - |   9.05 KB |
