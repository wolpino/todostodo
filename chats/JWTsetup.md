# JWT Setup in Your Project

JWTs are used for **stateless authentication** after users log in or authenticate via OAuth. Here's how they work:

## 1. JWT Creation

When a user logs in or completes OAuth:
- `AuthController.GetToken()` creates a JWT token with user claims
- Token is signed with HMAC-SHA256 using the secret key
- Token expires in **3 days**
- Returns a `JwtData` object with the token and expiration

```csharp
// Token includes these claims:
new Claim(ClaimTypes.Name, user.Id.ToString()),
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
// + any user roles
```

## 2. JWT Configuration

Located in `Program.cs`:
- **Key Source**: `builder.Configuration["JWT:Key"]` → user secret
- **Stored in**: `AuthOptions` static class with:
  - `ISSUER = "$rootnamespace$"`
  - `AUDIENCE = "$rootnamespace$.Client"`
  - `KEY = ""` (loaded from config)

## 3. JWT Validation

JwtBearer middleware validates incoming tokens:
- Verifies issuer matches `AuthOptions.ISSUER`
- Verifies audience matches `AuthOptions.AUDIENCE`  
- Verifies signature with symmetric key
- `OnTokenValidated` event loads user from database for authorization

## 4. Frontend Usage

- Token stored in `localStorage`
- Automatically added to API requests in Authorization header: `Bearer {token}`
- `AuthProvider` manages token state

---

## Fixing Migration Errors

The migration error occurs because `Program.cs` **throws an exception** if `JWT:Key` is missing. EF Core needs to initialize the app to create migrations.

**Set the user secret:**

```bash
dotnet user-secrets set JWT:Key "your-secret-key-here"
```

Then run the migration:

```bash
dotnet-ef migrations add AddTasksAndSettingsModels
```

**Apply the migration:**

```bash
dotnet-ef database update
```
