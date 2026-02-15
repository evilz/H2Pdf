# GitHub Copilot Instructions for H2Pdf

This file provides project-wide instructions for GitHub Copilot when working with the H2Pdf repository.

## Project Overview

H2Pdf is a .NET library that generates production-ready PDFs using Razor components and C#. It enables developers to create PDFs using familiar Razor syntax and modern .NET patterns.

## Technology Stack

- **Framework:** .NET 10.0
- **Language:** C# 12
- **PDF Engine:** MigraDoc/PdfSharp
- **Dependency Injection:** ASP.NET Core DI
- **Testing:** xUnit
- **Benchmarking:** BenchmarkDotNet

## Repository Structure

```
H2Pdf/
├── src/H2Pdf/              # Main library source code
├── tests/H2Pdf.Tests/      # Unit and integration tests
├── samples/                # Sample applications
├── examples/               # Additional code examples
├── benchmarks/             # Performance benchmarks
└── docs/                   # Documentation (README, Architecture, etc.)
```

## Development Commands

### Build
```bash
dotnet restore H2Pdf.slnx
dotnet build H2Pdf.slnx --configuration Release
```

### Test
```bash
dotnet test H2Pdf.slnx --configuration Release
```

### Run Samples
```bash
cd samples/H2Pdf.Sample
dotnet run
```

### Benchmarks
```bash
dotnet run -c Release --project benchmarks/H2Pdf.Benchmarks
```

## Code Style and Conventions

- Use modern C# features (records, pattern matching, init properties)
- Follow existing naming conventions in the codebase
- Add XML documentation comments to all public APIs
- Use dependency injection for better testability
- Prefer immutability where appropriate
- Keep methods focused and single-purpose

## Testing Requirements

- All new features must include tests
- Tests should follow Arrange-Act-Assert pattern
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- All tests must pass before merging PRs
- Aim for high coverage of public APIs

## Documentation Standards

- Update README.md when adding new features
- Keep Architecture.md synchronized with design changes
- Provide code examples in documentation
- Use clear, concise language in documentation
- Include XML comments for public APIs

## Performance Considerations

- This library is used for production PDF generation
- Be mindful of memory allocations
- Consider async/await patterns for I/O operations
- Profile performance-critical code paths
- Run benchmarks when making performance-related changes

## Security

- Never commit secrets or API keys
- Validate all user inputs
- Handle file paths securely
- Use proper exception handling
- Follow .NET security best practices

## Dependency Management

- Minimize external dependencies
- Carefully evaluate new dependencies before adding
- Keep dependencies up-to-date with security patches
- Document rationale for new dependencies

## Pull Request Guidelines

1. Create a feature branch from `main`
2. Make focused, atomic commits
3. Ensure all tests pass locally
4. Update documentation if needed
5. Provide clear PR description explaining:
   - What changed
   - Why it changed
   - How to test it

## Cross-Platform Support

H2Pdf supports Windows, Linux, and macOS. Ensure all changes work across platforms:
- Avoid platform-specific APIs unless necessary
- Use `Path.Combine()` for file paths
- Test on multiple platforms when possible

## CI/CD

The project uses GitHub Actions for continuous integration:
- All PRs must pass CI checks
- CI runs on Ubuntu (Linux)
- Tests run in Release configuration
- NuGet packages are built and validated

## Common Tasks

### Adding a New Feature
1. Write tests first (TDD approach recommended)
2. Implement the feature
3. Update documentation
4. Run full test suite
5. Create PR with clear description

### Fixing a Bug
1. Write a failing test that reproduces the bug
2. Fix the bug
3. Verify the test passes
4. Check for similar issues in the codebase
5. Create PR with bug description and fix

### Updating Documentation
1. Make changes in markdown files
2. Verify formatting is correct
3. Ensure links work
4. Update examples if API changed
5. Create PR

## Getting Help

- Review [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines
- Check [Architecture.md](Architecture.md) for design details
- Look at existing code and tests for patterns
- Review samples for usage examples

## Custom Agents

This repository has specialized agents for different tasks:
- **dotnet-dev-agent** - For .NET code development
- **docs-agent** - For documentation work
- **test-agent** - For writing and maintaining tests

Use the appropriate agent based on your task for best results.
