# Contributing to CSharpEssentials

Contributions are welcome: bug reports, discussion of the current code, fixes, feature proposals, and help as a maintainer. Everything (code, issues, feature requests and pull requests) lives on GitHub.

## Code of conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisites

- .NET SDK: the 11.0 prerelease SDK pinned in `global.json` (`rollForward` is `latestFeature`, prereleases allowed). The libraries target .NET 11/10/9/8 and netstandard2.0/2.1, so a build needs only that SDK; the NuGet packages themselves work with the SDKs of their target frameworks.
- .NET 9 and .NET 10 runtimes: the test projects run on net9.0 and net10.0 (the net8.0 test project rolls forward to a newer runtime).
- Docker: the PostgreSQL tests of `CSharpEssentials.Tests` start a container through Testcontainers.
- Git.

## Workflow

The project uses [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow): fork, branch from `main`, open a pull request.

```bash
git clone https://github.com/<your-username>/CSharpEssentials.git
cd CSharpEssentials
git config core.hooksPath .githooks   # pre-commit hook that checks the README badges
git checkout -b <branch-name>         # for example feature/new-validation-rule

dotnet build
dotnet test
```

1. Make your change. Add tests for code that should be tested, and update the documentation for any API you change.
2. Commit with a Conventional Commits message, for example `git commit -m "feat(results): add MapError overload for async chains"`. release-please generates releases from these messages: `feat` gives a minor bump, `fix` and `perf` a patch, and `feat!` or a `BREAKING CHANGE:` footer a major. See `.claude/rules/git.md` for the allowed types and scopes.
3. Keep your branch current:

   ```bash
   git remote add upstream https://github.com/senrecep/CSharpEssentials.git
   git fetch upstream
   git rebase upstream/main
   ```

4. Push (`git push origin <branch-name>`) and open a pull request from your fork. Describe the change and link related issues. The build, the test suite and the CI package checks must pass.

## Bug reports

Report bugs in the [issue tracker](https://github.com/senrecep/CSharpEssentials/issues) ([new issue](https://github.com/senrecep/CSharpEssentials/issues/new)). A useful report has:

- a short summary and background
- specific steps to reproduce, with sample code if you can
- what you expected to happen
- what actually happens
- notes, such as why you think it happens or what you tried that did not work

## Coding style

- 4 spaces for indentation (see `.editorconfig`).
- Follow the C# conventions in `.claude/rules/csharp.md`. The build treats warnings, analyzer findings and code style violations as errors.
- Write XML documentation for public APIs; do not add inline code comments that explain code.
- Keep methods small and focused, with meaningful names.

## Testing

- Write unit tests for new features, with both positive and negative cases.
- Follow the existing test patterns (`.claude/rules/testing.md`).
- Make sure all tests pass before you open a pull request.

## Documentation

- Update the documentation your change affects, including README.MD (package table, badges) when needed.
- Include XML documentation comments (`///`) for public APIs.
- When a public API changes, update the package `Readme.MD` and the matching `.well-known/agent-skills/<package>/SKILL.md`. The same file ships inside the package at `skills/<name>/SKILL.md`, so agents read the version that matches the installed package.
- Do not bump the version or edit `CHANGELOG.md` in a feature pull request; release-please generates both.
- Add examples for new features.

## License

Code you submit is covered by the same [MIT License](LICENCE) as the rest of the project. Contact the maintainers if that is a concern.
