# Contributing to CSharpEssentials

Contributions are welcome: bug reports, discussion of the current code, fixes, feature proposals, and help as a maintainer.

## Code of conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## Getting started

### Prerequisites

- **.NET SDK**: the SDK pinned in `global.json` (currently 11.0 RC1, `rollForward` is `latestFeature` with prereleases allowed). The libraries target .NET 11/10/9/8 and netstandard2.0/2.1, so a build needs only that SDK; the NuGet packages themselves work with the SDKs of their target frameworks.
- **.NET 9 and .NET 10 runtimes**: the test projects run on net9.0 and net10.0.
- **Docker**: the PostgreSQL tests of `CSharpEssentials.Tests` start a container through Testcontainers.
- **Git**: for version control.

### First-time setup

```bash
# Clone your fork
git clone https://github.com/<your-username>/CSharpEssentials.git
cd CSharpEssentials

# Configure git hooks (enables pre-commit badge validation)
git config core.hooksPath .githooks

# Build the solution
dotnet build

# Run the test suite
dotnet test
```

## Development workflow

1. Fork the repository:

   ```bash
   # Clone your fork
   git clone https://github.com/<your-username>/CSharpEssentials.git

   # Navigate to the newly cloned directory
   cd CSharpEssentials
   ```

2. Create a branch:

   ```bash
   # Create a new branch for your changes
   git checkout -b <branch-name>
   # Example: git checkout -b feature/new-validation-rule
   ```

3. Make your changes:

   - Write your code
   - Add tests if necessary
   - Update documentation

4. Commit your changes:

   ```bash
   # Add your changes
   git add .

   # Commit with a Conventional Commits message
   git commit -m "feat(results): add MapError overload for async chains"
   ```

   Releases are generated from commit messages by release-please: `feat` gives a minor bump, `fix` and `perf` a patch, and `feat!` or a `BREAKING CHANGE:` footer a major. See `.claude/rules/git.md` for the allowed types and scopes.

5. Keep your branch updated:

   ```bash
   # Add the upstream repository
   git remote add upstream https://github.com/senrecep/CSharpEssentials.git

   # Fetch upstream changes
   git fetch upstream

   # Rebase your branch on upstream main
   git rebase upstream/main
   ```

6. Push your changes:

   ```bash
   # Push your changes to your fork
   git push origin <branch-name>
   ```

7. Open a pull request: go to your fork on GitHub, click "New Pull Request", select your branch, describe your changes and link any related issues.

## We develop with GitHub

The code, issues, feature requests and pull requests all live on GitHub.

## We use [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow)

Propose changes to the codebase through a pull request:

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code follows the existing style.
6. Open the pull request.

## Contributions are licensed under the MIT License

Code you submit is covered by the same [MIT License](http://choosealicense.com/licenses/mit/) as the rest of the project. Contact the maintainers if that is a concern.

## Report bugs in the GitHub [issue tracker](https://github.com/senrecep/CSharpEssentials/issues)

Public bugs are tracked in GitHub issues. To report one, [open a new issue](https://github.com/senrecep/CSharpEssentials/issues/new).

## Write bug reports with detail, background, and sample code

A useful bug report has:

- A short summary and background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can.
- What you expected would happen
- What actually happens
- Notes, such as why you think it happens or what you tried that did not work

## Development process

1. Clone the repository
2. Create a branch for your feature or fix
3. Write your code
4. Write or update tests
5. Run the test suite
6. Push your changes
7. Open a pull request

## Coding style

- Use 4 spaces for indentation
- Follow C# coding conventions
- Write XML documentation for public APIs; do not add inline code comments that explain code
- Keep methods small and focused
- Use meaningful names for variables and methods

## Testing

- Write unit tests for new features
- Make sure all tests pass before you open a pull request
- Follow existing test patterns
- Include both positive and negative test cases

## Documentation

- Update the documentation your change affects
- Include XML documentation comments (`///`) for public APIs
- Update README.MD if needed (package table, badges)
- Update the package `Readme.MD` and the matching `.well-known/agent-skills/<package>/SKILL.md` when a public API changes
- Do not bump the version or edit `CHANGELOG.md` in a feature pull request; release-please generates both
- Add examples for new features

## License

By contributing, you agree that your contributions are licensed under the project's MIT License.
