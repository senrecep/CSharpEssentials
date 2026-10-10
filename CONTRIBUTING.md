# Contributing to CSharpEssentials

We love your input! We want to make contributing to CSharpEssentials as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Code of Conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## Getting Started

### Prerequisites

- **.NET SDK**: the SDK pinned in `global.json` (currently 11.0 RC1, `rollForward` is `latestFeature` with prereleases allowed). The libraries target .NET 11/10/9/8 and netstandard2.0/2.1, so a build needs only that SDK; the NuGet packages themselves work with the SDKs of their target frameworks.
- **.NET 9 and .NET 10 runtimes**: the test projects run on net9.0 and net10.0.
- **Docker**: the PostgreSQL tests of `CSharpEssentials.Tests` start a container through Testcontainers.
- **Git**: For version control

### First-Time Setup

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

## Development Workflow

Here's how you can contribute to the project using Git:

1. Fork the Repository:

   ```bash
   # Clone your fork
   git clone https://github.com/<your-username>/CSharpEssentials.git

   # Navigate to the newly cloned directory
   cd CSharpEssentials
   ```

2. Create a Branch:

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

7. Create a Pull Request:
   - Go to your fork on GitHub
   - Click "New Pull Request"
   - Select your branch and submit the pull request
   - Add a description of your changes
   - Link any relevant issues

## We Develop with Github

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

## We Use [Github Flow](https://docs.github.com/en/get-started/using-github/github-flow)

Pull requests are the best way to propose changes to the codebase. We actively welcome your pull requests:

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code follows the existing style.
6. Issue that pull request!

## Any contributions you make will be under the MIT Software License

In short, when you submit code changes, your submissions are understood to be under the same [MIT License](http://choosealicense.com/licenses/mit/) that covers the project. Feel free to contact the maintainers if that's a concern.

## Report bugs using Github's [issue tracker](https://github.com/senrecep/CSharpEssentials/issues)

We use GitHub issues to track public bugs. Report a bug by [opening a new issue](https://github.com/senrecep/CSharpEssentials/issues/new); it's that easy!

## Write bug reports with detail, background, and sample code

**Great Bug Reports** tend to have:

- A quick summary and/or background
- Steps to reproduce
  - Be specific!
  - Give sample code if you can.
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening, or stuff you tried that didn't work)

## Development Process

1. Clone the repository
2. Create a new branch for your feature/fix
3. Write your code
4. Write/update tests
5. Run the test suite
6. Push your changes
7. Create a Pull Request

## Coding Style

- Use 4 spaces for indentation
- Follow C# coding conventions
- Write XML documentation for public APIs; do not add inline code comments that explain code
- Keep methods small and focused
- Use meaningful names for variables and methods

## Testing

- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Follow existing test patterns
- Include both positive and negative test cases

## Documentation

- Update relevant documentation
- Include XML documentation comments (`///`) for public APIs
- Update README.MD if needed (package table, badges)
- Update the package `Readme.MD` and the matching `.well-known/agent-skills/<package>/SKILL.md` when a public API changes
- Do not bump the version or edit `CHANGELOG.md` in a feature pull request; release-please generates both
- Add examples for new features

## License

By contributing, you agree that your contributions will be licensed under its MIT License.
