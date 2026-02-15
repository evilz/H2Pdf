# GitHub Copilot Agents Configuration

This directory contains custom agent configurations for GitHub Copilot coding agents working with the H2Pdf repository.

## Available Agents

### 1. dotnet-dev-agent.md
**Purpose:** Expert .NET developer for H2Pdf library development  
**Use When:** Developing features, fixing bugs, or modifying core library code in `src/H2Pdf/`  
**Tools:** read, edit, search, bash  

This agent understands:
- .NET 10.0 and C# 12 best practices
- H2Pdf architecture and design patterns
- MigraDoc/PdfSharp PDF rendering
- Dependency injection patterns
- Performance considerations

### 2. docs-agent.md
**Purpose:** Expert technical writer for H2Pdf documentation  
**Use When:** Writing or updating documentation files  
**Tools:** read, edit, search  

This agent specializes in:
- Markdown documentation (README, Architecture, Contributing)
- Code examples and snippets
- XML documentation comments
- Clear technical writing

### 3. test-agent.md
**Purpose:** QA engineer for H2Pdf automated testing  
**Use When:** Writing or maintaining tests in `tests/H2Pdf.Tests/`  
**Tools:** read, edit, search, bash  

This agent focuses on:
- xUnit test framework
- Test naming conventions
- Arrange-Act-Assert pattern
- Test coverage and quality

## Using These Agents

When working with GitHub Copilot coding agent, the appropriate agent will be automatically selected based on:
- The files you're working with
- The task description
- The agent's defined responsibilities

You can also manually specify which agent to use in your task description.

## Project-Wide Instructions

The `.github/copilot-instructions.md` file contains project-wide guidelines that apply to all agents and general Copilot usage, including:
- Project structure and overview
- Development commands
- Code style conventions
- Testing requirements
- Documentation standards

## Modifying Agents

When updating agent configurations:
1. Maintain the YAML frontmatter structure
2. Keep instructions clear and specific
3. Include concrete examples
4. Define clear boundaries (DO/DO NOT)
5. Test the agent with representative tasks

## Best Practices

These agent configurations follow GitHub's best practices for Copilot coding agents:
- Specific personas and purposes
- Clear boundaries and constraints
- Concrete examples of desired output
- Tech stack and tool specifications
- Explicit workflows and commands
- Security and safety guidelines

## References

- [GitHub Copilot coding agent documentation](https://docs.github.com/en/copilot/tutorials/coding-agent)
- [Custom agents configuration reference](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [How to write great agent instructions](https://github.blog/ai-and-ml/github-copilot/how-to-write-a-great-agents-md-lessons-from-over-2500-repositories/)
