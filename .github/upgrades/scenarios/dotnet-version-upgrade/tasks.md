# Tasks

- id: 01-update-csproj
  title: Updating project TargetFramework
  description: Change TargetFramework in todostodo.csproj from net6.0 to net8.0, verify project loads in IDE.

- id: 02-update-packages
  title: Updating NuGet packages
  description: Run dotnet list package --outdated, update packages where safe, pay special attention to NuGet.0004 risk; document any packages requiring manual approval.

- id: 03-restore-and-build
  title: Restoring and building
  description: dotnet restore && dotnet build; fix compile errors introduced by the upgrade.

- id: 04-fix-source-incompatibilities
  title: Fixing source incompatibilities (Api.0002)
  description: Implement code changes to address source-breaking changes flagged by the assessment.

- id: 05-resolve-binary-incompatibilities
  title: Resolving binary incompatibilities (Api.0001)
  description: Replace or update binary dependencies or apply compatibility shims; run integration smoke tests.

- id: 06-update-ci
  title: Updating CI to use .NET 8
  description: Modify GitHub Actions workflows to target dotnet 8 SDK and run full CI.

- id: 07-run-tests-and-validate
  title: Running tests and validation
  description: Run unit and integration tests in CI and locally; perform smoke tests on build artifacts.

- id: 08-create-pr
  title: Create PR with changes
  description: Open a PR describing changes, highlight package risks and incompatibilities, request reviewers.
