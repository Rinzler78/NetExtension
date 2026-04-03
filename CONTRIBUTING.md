# Contributing to Rinzler78.NetExtension

Thank you for your interest in contributing to Rinzler78.NetExtension! This document provides guidelines and information for contributors.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Code Style Guidelines](#code-style-guidelines)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)
- [Code Review Guidelines](#code-review-guidelines)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Issue Reporting](#issue-reporting)
- [Security Reporting](#security-reporting)

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct:

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Maintain professional communication

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Git
- A code editor (Visual Studio, VS Code, JetBrains Rider, etc.)

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/NetExtension.git
   cd NetExtension
   ```

3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/Rinzler78/NetExtension.git
   ```

4. **Install dependencies**:
   ```bash
   dotnet restore src/Rinzler78.NetExtension.sln
   ```

5. **Build the solution**:
   ```bash
   ./scripts/local/build.sh
   ```

6. **Run tests** to ensure everything works:
   ```bash
   ./scripts/local/test.sh
   ```

7. **Install pre-commit hooks** (runs automatically on first build, or manually):
   ```bash
   # Install pre-commit hooks
   pip install pre-commit && pre-commit install
   ```

## Making Changes

### Before You Start

1. **Check existing issues** to see if your idea is already being worked on
2. **Create an issue** to discuss major changes before implementing
3. **Keep changes focused** - one feature/fix per pull request

### Development Workflow

1. **Create a feature branch** from `develop`:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our coding standards
3. **Add tests** for new functionality
4. **Update documentation** as needed
5. **Test your changes** thoroughly

## Code Style Guidelines

### General Guidelines

- **Follow C# coding conventions** from Microsoft
- **Use meaningful names** for variables, methods, and classes
- **Write clear, self-documenting code**
- **Keep methods small and focused** (Single Responsibility Principle)
- **Use async/await properly** for asynchronous operations

### Naming Conventions

- **Classes**: PascalCase (e.g., `StringHelper`)
- **Methods**: PascalCase (e.g., `CalculateSimilarity`)
- **Properties**: PascalCase (e.g., `IsValid`)
- **Fields**: camelCase with underscore prefix for private fields (e.g., `_value`)
- **Parameters**: camelCase (e.g., `inputString`)
- **Local variables**: camelCase (e.g., `result`)

### Documentation

- **Add XML documentation** for all public APIs:
  ```csharp
  /// <summary>
  /// Calculates the similarity between two strings using Jaro-Winkler algorithm.
  /// </summary>
  /// <param name="source">The source string</param>
  /// <param name="target">The target string to compare</param>
  /// <returns>A similarity score between 0.0 and 1.0</returns>
  public static double CalculateSimilarity(this string source, string target)
  ```

- **Include examples** in documentation when helpful
- **Update README.md** for new modules or significant features

### Code Organization

- **Group related functionality** in appropriate namespaces
- **Use extension methods** for utility functions
- **Follow existing project structure**
- **Keep files focused** on a single responsibility

## Testing Requirements

### Unit Tests

- **Write unit tests** for all new functionality
- **Use descriptive test names** that explain what is being tested
- **Follow the Arrange-Act-Assert pattern**:
  ```csharp
  [Fact]
  public void StringSimilarity_WithIdenticalStrings_ShouldReturnOne()
  {
      // Arrange
      var source = "hello";
      var target = "hello";

      // Act
      var result = source.CalculateSimilarity(target);

      // Assert
      result.Should().Be(1.0);
  }
  ```

### Test Guidelines

- **Use FluentAssertions** for readable assertions
- **Test edge cases** (null values, empty strings, etc.)
- **Mock external dependencies** when necessary
- **Ensure tests are deterministic** and don't depend on external resources

### Running Tests

```bash
# Run all tests
./scripts/local/test.sh

# Run tests with coverage
./scripts/local/test.sh

# Run specific test class
dotnet test --filter "ClassName=StringHelperTests"
```

## Pull Request Process

### Before Submitting

1. **Ensure all tests pass**:
   ```bash
   ./scripts/local/test.sh
   ```

2. **Check code formatting**:
   ```bash
   ./scripts/local/quality.sh
   ```

3. **Build in Release mode**:
   ```bash
   ./scripts/local/build.sh
   ```

4. **Update documentation** if necessary

### Pull Request Template

When creating a pull request, please include:

- **Clear description** of changes made
- **Reason for the change** (fixes issue #X, adds feature Y)
- **Testing performed** (unit tests, manual testing)
- **Breaking changes** (if any)
- **Screenshots** (for UI changes)

### Example PR Description

```markdown
## Description
Adds a new string extension method for calculating Levenshtein distance.

## Changes Made
- Added `ComputeLevenshteinDistance` extension method
- Added comprehensive unit tests
- Updated string utilities documentation

## Testing
- All existing tests pass
- Added 15 new unit tests covering edge cases
- Manual testing with various string inputs

## Related Issues
Closes #42
```

## Code Review Guidelines

### For Contributors

- **Be responsive** to feedback
- **Explain your approach** when asked
- **Make requested changes promptly**
- **Ask questions** if feedback is unclear

### Review Criteria

Pull requests are reviewed for:

- **Functionality**: Does it work as intended?
- **Code quality**: Is it readable, maintainable, and well-structured?
- **Testing**: Are there adequate tests?
- **Documentation**: Is it properly documented?
- **Performance**: Does it impact performance negatively?
- **Security**: Are there any security concerns?

## Branch Naming Conventions

Use descriptive branch names that follow this pattern:

- **Features**: `feature/add-xml-validation`
- **Bug fixes**: `bugfix/fix-null-reference`
- **Documentation**: `docs/update-contributing-guide`
- **Performance**: `perf/optimize-string-operations`
- **Refactoring**: `refactor/extract-common-utilities`

## Issue Reporting

### Bug Reports

When reporting bugs, include:

- **Description** of the issue
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Environment** (.NET version, OS, etc.)
- **Code sample** (if applicable)

### Feature Requests

For feature requests, include:

- **Description** of the proposed feature
- **Use case** and motivation
- **Proposed API** (if applicable)
- **Examples** of how it would be used

## Security Reporting

If you discover a security vulnerability, please:

1. **Do NOT create a public issue**
2. **Email the maintainers** at [security email if available]
3. **Include details** about the vulnerability
4. **Wait for acknowledgment** before public disclosure

## Getting Help

- **Create an issue** for questions about the codebase
- **Check existing documentation** in the `/docs` directory
- **Review the README.md** for basic information
- **Look at existing code** for examples and patterns

## License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (MIT License).

---

**Thank you for contributing to Rinzler78.NetExtension!** 🚀

Your contributions help make this library better for everyone.
