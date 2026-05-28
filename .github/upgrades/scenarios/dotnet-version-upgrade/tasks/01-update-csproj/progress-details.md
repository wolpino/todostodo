## Files Modified
- todostodo.csproj (TargetFramework updated to net8.0; Microsoft.EntityFrameworkCore pinned to 10.0.8)

## Build/Restore Result
- Restore succeeded with 3 warnings.
- Build succeeded with 69 warnings.

### Notable Warnings
- NU1903: AutoMapper 11.0.1 has a known high severity vulnerability
- NU1902: Microsoft.IdentityModel.JsonWebTokens 6.10.0 moderate vulnerability
- NU1902: System.IdentityModel.Tokens.Jwt 6.10.0 moderate vulnerability
- Several nullable-reference warnings (CS86xx) to review

## Commands Run
- dotnet restore
- dotnet build --no-restore

## Changes Summary
- Updated TargetFramework to net8.0
- Pinned Microsoft.EntityFrameworkCore to 10.0.8 to resolve package downgrade

## Next Steps
1. Audit vulnerable packages and decide which to update now (AutoMapper, Identity JWT packages)
2. Update packages (safe updates first) and run restore/build
3. Fix nullable warnings as part of code changes when appropriate

