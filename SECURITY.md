# Security Policy

## Supported Versions

| Version | Supported |
| ------- | --------- |
| 3.x     | Yes       |
| < 3.0   | No        |

## Reporting a Vulnerability

Please **do not** open a public GitHub issue for security vulnerabilities.

Report vulnerabilities privately via [GitHub Security Advisories](https://github.com/senrecep/CSharpEssentials/security/advisories/new).

Include:
- Affected package(s) and version(s)
- Description of the vulnerability
- Steps to reproduce
- Potential impact

You will receive a response within **72 hours**. If the vulnerability is confirmed, a patched release will be published and a CVE will be requested where appropriate.

## Scope

This policy covers all packages published under the `CSharpEssentials` NuGet namespace:

- `CSharpEssentials`
- `CSharpEssentials.Core`
- `CSharpEssentials.Maybe`
- `CSharpEssentials.Result`
- `CSharpEssentials.Rules`
- `CSharpEssentials.Entity`
- `CSharpEssentials.EntityFrameworkCore`
- `CSharpEssentials.AspNetCore`
- And all other packages in this repository

## Out of Scope

- Vulnerabilities in third-party dependencies (report to the respective maintainer)
- Issues in unsupported versions (< 3.0)
