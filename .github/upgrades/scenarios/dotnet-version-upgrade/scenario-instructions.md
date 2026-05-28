# .NET Version Upgrade (dotnet-version-upgrade)

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net8.0 (user accepted fallback to .NET 11 Preview)

## Source Control
- **Source Branch**: addsqlite
- **Working Branch**: upgrade-dotnet-11
- **Commit Strategy**: After Each Task


## Upgrade Options
- **Upgrade Strategy**: upgrade-net8 (Upgrade to net8.0)

## Strategy
- Follow a conservative, test-first approach to upgrade the single project todostodo.csproj to net8.0.
- Sequence: create feature branch → update csproj TargetFramework → restore and update NuGet packages → build & run tests → address source (Api.0002) and binary (Api.0001) incompatibilities → update CI → open PR.
- Prioritize manual review for package risk NuGet.0004; binary incompatibilities may need dependency replacement or API adaptation.
- Keep commits small (one per task) and include clear PR notes and rollback instructions.

