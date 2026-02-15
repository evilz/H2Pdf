# Contributing to H2Pdf

Thanks for your interest in improving H2Pdf.

## Setup

```bash
git clone https://github.com/evilz/H2Pdf.git
cd H2Pdf
dotnet build
dotnet test
```

## Development workflow

1. Create a branch from `main`.
2. Make focused changes with tests when applicable.
3. Ensure `dotnet test` passes.
4. Open a pull request with:
   - what changed
   - why it changed
   - any screenshots/artifacts (if UI/docs visuals changed)

## Pull request checklist

- [ ] Code builds locally
- [ ] Tests pass locally
- [ ] Docs were updated when behavior changed
- [ ] Breaking changes are clearly described

## Releases and Publishing

This repository automatically publishes the NuGet package when a GitHub release is created.

### How to publish a new version:

1. Update the version in `src/H2Pdf/H2Pdf.csproj`
2. Commit and push the version change
3. Create a new GitHub release with a tag (e.g., `v1.0.0`)
4. The `publish.yml` workflow will automatically:
   - Build and test the project
   - Pack the NuGet package
   - Push it to NuGet.org

### Required secret:

The repository must have a `NUGET_API_KEY` secret configured in GitHub Settings > Secrets and variables > Actions. This should contain a valid NuGet.org API key with permission to push packages.

## Reporting issues

Please use issue templates and include:

- steps to reproduce
- expected vs. actual behavior
- environment (`dotnet --info`)
