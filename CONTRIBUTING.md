# Contributing to CSharpEssentials

We love your input! We want to make contributing to CSharpEssentials as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Code of Conduct

By participating in this project, you are expected to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## Development Workflow

Here's how you can contribute to the project using Git:

1. Fork the Repository:

   ```bash
   # Clone your fork
   git clone https://github.com/SenRecep/CSharpEssentials.git

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

   # Commit with a descriptive message
   git commit -m "Description of your changes"
   ```

5. Keep your branch updated:

   ```bash
   # Add the upstream repository
   git remote add upstream https://github.com/SenRecep/CSharpEssentials.git

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

## We Use [Github Flow](https://guides.github.com/introduction/flow/index.html)

Pull requests are the best way to propose changes to the codebase. We actively welcome your pull requests:

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code follows the existing style.
6. Issue that pull request!

## Any contributions you make will be under the MIT Software License

In short, when you submit code changes, your submissions are understood to be under the same [MIT License](http://choosealicense.com/licenses/mit/) that covers the project. Feel free to contact the maintainers if that's a concern.

## Report bugs using Github's [issue tracker](https://github.com/SenRecep/CSharpEssentials/issues)

We use GitHub issues to track public bugs. Report a bug by [opening a new issue](https://github.com/SenRecep/CSharpEssentials/issues/new); it's that easy!

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
- Write XML documentation for public APIs
- Keep methods small and focused
- Use meaningful names for variables and methods

## Testing

- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Follow existing test patterns
- Include both positive and negative test cases

## Documentation

- Update relevant documentation
- Include XML comments for public APIs
- Update README.md if needed
- Add examples for new features

## License

By contributing, you agree that your contributions will be licensed under its MIT License.
