# Contributing to RazorPdfKit

Thanks for your interest in improving RazorPdfKit.

## Setup

```bash
git clone https://github.com/evilz/RazorPdf.git
cd RazorPdf
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

## Reporting issues

Please use issue templates and include:

- steps to reproduce
- expected vs. actual behavior
- environment (`dotnet --info`)
