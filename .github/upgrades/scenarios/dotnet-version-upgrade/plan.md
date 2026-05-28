# Upgrade plan: net8.0

## Objective
Upgrade the repository's single project (todostodo.csproj) from net6.0 to net8.0 with minimal disruption and verified tests.

## Scope
- Project: todostodo.csproj
- Current frameworks: net6.0
- Target framework: net8.0
- Assessment signals: packageRisks: [NuGet.0004], binaryIncompatibilities: [Api.0001], sourceIncompatibilities: [Api.0002]

## Approach
1. Prepare: create a feature branch and ensure CI uses a stable .NET 8 SDK image in a temp job.
2. Update project files: change TargetFramework to net8.0 for todostodo.csproj.
3. Restore & update packages: dotnet restore; update NuGet packages where required, paying attention to NuGet.0004.
4. Build & test: dotnet build; dotnet test — fix compilation and test failures.
5. Address incompatibilities: fix Api.0002 source issues and resolve Api.0001 binary compatibility problems (may require package or binding updates).
6. CI update & PR: update pipelines to use dotnet 8, run full CI, document changes, open PR.

## Files to change
- todostodo.csproj
- .github/workflows/*.yml (CI images/SDK versions)
- Any packages.props or Directory.Packages.props if present

## Tests & Verification
- All unit and integration tests pass locally and in CI
- Build artifacts produce no runtime exceptions on basic smoke test

## Rollback
If significant regressions appear, revert the working branch and investigate in a draft PR.

## Timeline
Estimate: 1–2 workdays (single small project). Complex incompatibilities may extend the effort.

## Notes
SDK-style project: true — conversion effort minimal. Prioritize manual review for NuGet.0004 (package risk) and human verification for binary incompatibilities.
