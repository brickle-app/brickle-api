# Profile Legal Consent and Minimum Investment API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the API authoritative for typed identity evidence, versioned legal consent, aggregate profile review, user-scoped authorization, and the COP 1,000,000 minimum for every investment operation.

**Architecture:** Extend the existing .NET 8 CQRS code in `BricklePlatform.Api/Application` with focused compliance entities and EF Core repositories rather than adding another application layer. Profile submission, reconsent, and review persist complete relational aggregates atomically; a shared investment policy validates identity, profile approval, current legal evidence, campaign state, inventory, and amount-to-bricks consistency before any webhook or blockchain side effect.

**Tech Stack:** .NET 8, ASP.NET Core authentication/authorization, MediatR 12.2.0, FluentValidation 11.11.0, Entity Framework Core 8.0.3, SQL Server, Azure Blob Storage, xUnit 2.8.1, Moq 4.20.70.

## Global Constraints

- Scope is only `/Volumes/B/Projects/Brickle/BricklePlatform-api`; do not modify mobile or admin repositories.
- The COP 1,000,000 minimum applies to every investment operation.
- Calculate minimum visible bricks as `decimal.Ceiling(1_000_000m / PricePerToken)`.
- Bricks are positive indivisible integers; authoritative amount is `bricks * PricePerToken`.
- Use `decimal` for money and multiply by `1_000_000m` for blockchain base units only after policy validation; do not use `double`, `float`, or `Math.Pow` for money.
- Run profile, legal, wallet, campaign, amount, and inventory checks before permit, webhook, relayer, or blockchain execution.
- Required legal types are exactly `TermsAndConditions`, `PrivacyPolicy`, and `ParticipationContract`; one signature supports all three acceptance rows in one package.
- Acceptance is explicit and append-only. Never infer evidence from `User.TermsAccepted` and never migrate that boolean into acceptance rows.
- New active legal versions require a new package and signature before investment without clearing profile approval.
- Submitted profile snapshots, legal versions, and legal acceptance rows are immutable.
- Rejected users resubmit as a new revision; review is one integral approval or rejection.
- User profile completion and review flags are server-owned.
- Preserve legacy document rows and existing `UserDocumentDto` fields. Classify ambiguous existing rows as `LegacyIdentityDocument`; they cannot satisfy a new submission.
- Reuse `POST /api/User/documents`, `GET /api/User/{userId}/documents`, and `GET /api/User/documents/all`.
- Use `PUT /api/User/profile-submissions/{id}/status` for aggregate review. Legacy `PUT /api/User/documents/{id}/status` remains available but must not approve or reject the whole profile.
- JWT user operations derive identity from `sub`/`ClaimTypes.NameIdentifier` and reject mismatched route or multipart user IDs.
- Global document listing and all aggregate admin routes require a named `Administrator` policy. Caller-provided owner email is audit metadata, not identity.
- Stable error JSON is `{ "code": string, "message": string, "status": int, "traceId": string }`; use property name `code` consistently.
- New identity/signature uploads accept JPEG, PNG, or WebP up to 5 MiB and validate extension, declared MIME type, magic bytes, size, and SHA-256.
- Blob paths use collision-resistant document IDs. New rows persist `BlobPath`; responses generate short-lived read URLs. Preserve `DocumentUrl` for legacy rows.
- Unlinked staged uploads expire after 24 hours; cleanup retries failed blob deletion and runs hourly.
- Do not invent or seed official legal URLs, versions, effective dates, or hashes. Deployment remains blocked until all three official artifacts are supplied.
- Before deployment, verify whether production `[dbo].[UserDocument]` uses `name` or `document_name`; migration must normalize safely or fail with an explicit prerequisite error.
- Follow red-green-refactor. Every production behavior begins with a focused failing test and a verified expected failure.
- Preserve unrelated worktree changes. The implementation worker may make the frequent commits listed below; this planning task itself must not commit.
- Baseline on 2026-08-12: `dotnet test BricklePlatform.Api.sln --no-restore` passes 54 tests with 0 failures and existing compiler warnings.

---

## File Map

**Domain model and contracts**

- Create `src/BricklePlatform.Domain/Enums/LegalDocumentType.cs`: three required legal types.
- Create `src/BricklePlatform.Domain/Enums/UserDocumentKind.cs`: front, back, signature, and conservative legacy kind.
- Create `src/BricklePlatform.Domain/Enums/ProfileSubmissionStatus.cs`: under review, approved, rejected.
- Create `src/BricklePlatform.Domain/Entities/LegalDocumentVersion.cs`: immutable legal artifact metadata.
- Create `src/BricklePlatform.Domain/Entities/ConsentPackage.cs`: idempotent three-document consent package.
- Create `src/BricklePlatform.Domain/Entities/UserLegalAcceptance.cs`: append-only audit row.
- Create `src/BricklePlatform.Domain/Entities/ProfileSubmission.cs`: immutable profile revision and review transition.
- Modify `src/BricklePlatform.Domain/Entities/UserDocument.cs`: typed staged evidence metadata and aggregate binding.
- Modify `src/BricklePlatform.Domain/Entities/User.cs`: private server-owned profile transitions.
- Create `src/BricklePlatform.Domain/DTOs/LegalDocumentVersionDto.cs`.
- Create `src/BricklePlatform.Domain/DTOs/ProfileSubmissionDtos.cs`.
- Modify `src/BricklePlatform.Domain/DTOs/UserDocumentDto.cs`: additive kind/submission/file metadata.
- Modify `src/BricklePlatform.Domain/DTOs/UpdateUserDto.cs`: remove client-writable legal/review fields.
- Create `src/BricklePlatform.Domain/Models/EvidenceUploadResult.cs`.
- Create `src/BricklePlatform.Domain/Models/InvestmentPolicyModels.cs`.
- Create `src/BricklePlatform.Domain/Exceptions/ApiErrorException.cs`.
- Create `src/BricklePlatform.Domain/Interfaces/ILegalDocumentRepository.cs`.
- Create `src/BricklePlatform.Domain/Interfaces/IProfileComplianceRepository.cs`.
- Create `src/BricklePlatform.Domain/Interfaces/IInvestmentPolicy.cs`.
- Modify `src/BricklePlatform.Domain/Interfaces/IUserDocumentRepository.cs`.
- Modify `src/BricklePlatform.Domain/Interfaces/IFileService.cs`.

**Infrastructure and persistence**

- Create configurations under `src/BricklePlatform.Infrastructure/Persistence/Configurations/` for all four new entities.
- Modify `src/BricklePlatform.Infrastructure/Persistence/Configurations/UserDocumentConfiguration.cs`.
- Modify `src/BricklePlatform.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Create `src/BricklePlatform.Infrastructure/Repositories/LegalDocumentRepository.cs`.
- Create `src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs`.
- Modify `src/BricklePlatform.Infrastructure/Repositories/UserDocumentRepository.cs`.
- Modify `src/BricklePlatform.Infrastructure/Interfaces/IBlobStorageRepository.cs`.
- Modify `src/BricklePlatform.Infrastructure/Repositories/BlobStorageRepository.cs`.
- Modify `src/BricklePlatform.Infrastructure/Constants/BlobConstants.cs`.
- Modify `src/BricklePlatform.Infrastructure/Services/FileService.cs`.
- Create EF migration `src/BricklePlatform.Infrastructure/Migrations/20260812180000_AddProfileLegalCompliance.cs` and `src/BricklePlatform.Infrastructure/Migrations/20260812180000_AddProfileLegalCompliance.Designer.cs`; update `src/BricklePlatform.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`.
- Modify `src/BricklePlatform.Infrastructure/DependencyContainer.cs`.

**API application and HTTP surface**

- Create `src/BricklePlatform.Api/Authorization/AuthorizationPolicies.cs`.
- Create `src/BricklePlatform.Api/Application/Security/IRequestActor.cs` and `HttpRequestActor.cs`.
- Create `src/BricklePlatform.Api/Application/Dtos/ApiErrorResponseDto.cs`.
- Create `src/BricklePlatform.Api/Application/ExceptionHandler/ApiErrorExceptionHandler.cs`.
- Create legal-document query/handler under `Application/Queries/LegalDocument/` and `Application/Handlers/LegalDocument/`.
- Create profile submission/reconsent/review commands, queries, and handlers in the existing CQRS folders.
- Create `src/BricklePlatform.Api/Application/Services/InvestmentPolicy.cs`.
- Create `StagedDocumentCleanupService.cs` and `StagedDocumentCleanupBackgroundService.cs` in the same services folder.
- Create profile/reconsent/review validators under `src/BricklePlatform.Api/Validators/ProfileSubmission/`.
- Create `src/BricklePlatform.Api/Controllers/LegalDocumentsController.cs`.
- Modify existing `src/BricklePlatform.Api/Controllers/UserController.cs` for every user/document/submission/reconsent/admin route.
- Modify authentication, upload, user update, legacy review, and investment handlers identified in the tasks below.

**Tests**

- Add focused xUnit files under `test/BricklePlatform.Test/Domain`, `Persistence`, `Auth`, `Exceptions`, `Handlers`, and `Services` using existing Moq and `NullLogger<T>` conventions.
- Modify existing auth, user, controller, and legacy document tests where behavior intentionally changes.

### Task 1: Stable API Errors and Authenticated Request Actor

**Files:**
- Create: `src/BricklePlatform.Domain/Exceptions/ApiErrorException.cs`
- Create: `src/BricklePlatform.Api/Application/Dtos/ApiErrorResponseDto.cs`
- Create: `src/BricklePlatform.Api/Application/ExceptionHandler/ApiErrorExceptionHandler.cs`
- Create: `src/BricklePlatform.Api/Application/Security/IRequestActor.cs`
- Create: `src/BricklePlatform.Api/Application/Security/HttpRequestActor.cs`
- Modify: `src/BricklePlatform.Api/Extensions/WebApplicationExtension.cs:65-69`
- Test: `test/BricklePlatform.Test/Exceptions/ApiErrorExceptionHandlerTests.cs`
- Test: `test/BricklePlatform.Test/Auth/HttpRequestActorTests.cs`

**Interfaces:**
- Consumes: ASP.NET Core `HttpContext.User` and request headers.
- Produces: `ApiErrorException(string code, string message, HttpStatusCode statusCode)` and `IRequestActor` used by all user-scoped commands.

- [ ] **Step 1: Write the failing error serialization test**

```csharp
[Fact]
public async Task HandlerWritesStableCodeProperty()
{
    var context = new DefaultHttpContext();
    context.Response.Body = new MemoryStream();
    var exception = new ApiErrorException(
        "PROFILE_NOT_APPROVED",
        "The latest profile submission is not approved.",
        HttpStatusCode.Forbidden);

    await new ApiErrorExceptionHandler().Handler(context, exception, new HeaderRequestModel
    {
        CorrelationId = "trace-1"
    });

    context.Response.Body.Position = 0;
    var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
    Assert.Equal(403, context.Response.StatusCode);
    Assert.Contains("\"code\":\"PROFILE_NOT_APPROVED\"", json);
}
```

- [ ] **Step 2: Run the test and verify RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~ApiErrorExceptionHandlerTests`

Expected: FAIL to compile because `ApiErrorException` and `ApiErrorExceptionHandler` do not exist.

- [ ] **Step 3: Implement the stable exception and response**

```csharp
public sealed class ApiErrorException : Exception
{
    public ApiErrorException(string code, string message, HttpStatusCode statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public HttpStatusCode StatusCode { get; }
}

public sealed record ApiErrorResponseDto(
    [property: JsonProperty("code")] string Code,
    [property: JsonProperty("message")] string Message,
    [property: JsonProperty("status")] int Status,
    [property: JsonProperty("traceId")] string TraceId);
```

Register the exact exception type in the existing middleware dictionary before `ApplicationException`.

- [ ] **Step 4: Write the failing actor tests**

Test `RequireUserId()` with `ClaimTypes.NameIdentifier`, JWT `sub`, missing/non-Guid subject, and `RequireUser(otherId)` mismatch. The actor exposes:

```csharp
public interface IRequestActor
{
    bool IsAdministrator { get; }
    Guid RequireUserId();
    string RequireSubject();
    void RequireUser(Guid expectedUserId);
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? Source { get; }
    string? AppVersion { get; }
    string CorrelationId { get; }
}
```

- [ ] **Step 5: Run actor tests and verify RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~HttpRequestActorTests`

Expected: FAIL to compile because `IRequestActor` is missing.

- [ ] **Step 6: Implement `HttpRequestActor` and register it**

Read `ClaimTypes.NameIdentifier` first, then `JwtRegisteredClaimNames.Sub`; reject malformed/mismatched IDs with `ApiErrorException("USER_SCOPE_MISMATCH", "The authenticated user does not match the requested user.", HttpStatusCode.Forbidden)`. Read `correlationId`, `source`, `app-version`, IP, and user agent from `IHttpContextAccessor`.

- [ ] **Step 7: Run both suites and verify GREEN**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~ApiErrorExceptionHandlerTests|FullyQualifiedName~HttpRequestActorTests"`

Expected: all selected tests PASS; response JSON uses `code`.

- [ ] **Step 8: Commit**

```bash
git add src/BricklePlatform.Domain/Exceptions src/BricklePlatform.Api/Application/Dtos/ApiErrorResponseDto.cs src/BricklePlatform.Api/Application/ExceptionHandler src/BricklePlatform.Api/Application/Security src/BricklePlatform.Api/Extensions/WebApplicationExtension.cs test/BricklePlatform.Test/Exceptions test/BricklePlatform.Test/Auth/HttpRequestActorTests.cs
git commit -m "feat: add stable API errors and request actor"
```

### Task 2: Named Administrator Policy and User Scope

**Files:**
- Create: `src/BricklePlatform.Api/Authorization/AuthorizationPolicies.cs`
- Modify: `src/BricklePlatform.Api/Authentication/ApiKeyAuthenticationHandler.cs:39-48`
- Modify: `src/BricklePlatform.Api/Extensions/AuthExtension.cs:59`
- Modify: `src/BricklePlatform.Api/Controllers/UserController.cs:943-1012`
- Test: `test/BricklePlatform.Test/Auth/AuthorizationPolicyTests.cs`

**Interfaces:**
- Consumes: existing API-key scheme and `IRequestActor` from Task 1.
- Produces: `AuthorizationPolicies.Administrator` for all global/admin routes.

- [ ] **Step 1: Write failing authorization tests**

Build an `AuthorizationService` and assert the policy succeeds only for a principal carrying `brickle:administrator=true`; assert an ordinary authenticated JWT principal fails.

- [ ] **Step 2: Run and verify RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~AuthorizationPolicyTests`

Expected: FAIL because `AuthorizationPolicies` does not exist.

- [ ] **Step 3: Implement policy constants and API-key claim**

```csharp
public static class AuthorizationPolicies
{
    public const string Administrator = "Administrator";
    public const string AdministratorClaim = "brickle:administrator";
}
```

Add `new Claim(AuthorizationPolicies.AdministratorClaim, bool.TrueString)` to a validated API-key principal and configure:

```csharp
services.AddAuthorization(options =>
    options.AddPolicy(AuthorizationPolicies.Administrator, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim(AuthorizationPolicies.AdministratorClaim, bool.TrueString)));
services.AddHttpContextAccessor();
services.AddScoped<IRequestActor, HttpRequestActor>();
```

- [ ] **Step 4: Apply authorization and scope to reused document routes**

Apply `[Authorize(Policy = AuthorizationPolicies.Administrator)]` to `GetAllDocuments` and `UpdateDocumentStatus`. Inject `IRequestActor` into `UserController` and call `_requestActor.RequireUser(userId)` before dispatching `GetUserDocumentsQuery`. Task 5 applies the same check to multipart upload before storage access.

- [ ] **Step 5: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~AuthorizationPolicyTests|FullyQualifiedName~UserControllerTests"`

Expected: all selected tests PASS.

```bash
git add src/BricklePlatform.Api/Authorization src/BricklePlatform.Api/Authentication/ApiKeyAuthenticationHandler.cs src/BricklePlatform.Api/Extensions/AuthExtension.cs src/BricklePlatform.Api/Controllers/UserController.cs test/BricklePlatform.Test/Auth/AuthorizationPolicyTests.cs test/BricklePlatform.Test/Controllers/UserControllerTests.cs
git commit -m "feat: enforce user and administrator policies"
```

### Task 3: Compliance Domain, EF Model, and Safe Migration

**Files:**
- Create: domain enums/entities/interfaces listed in the File Map.
- Create: `src/BricklePlatform.Infrastructure/Persistence/Configurations/{LegalDocumentVersion,ConsentPackage,UserLegalAcceptance,ProfileSubmission}Configuration.cs`
- Modify: `src/BricklePlatform.Domain/Entities/UserDocument.cs`
- Modify: `src/BricklePlatform.Infrastructure/Persistence/Configurations/UserDocumentConfiguration.cs`
- Modify: `src/BricklePlatform.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `src/BricklePlatform.Infrastructure/Migrations/20260812180000_AddProfileLegalCompliance.cs`
- Create: `src/BricklePlatform.Infrastructure/Migrations/20260812180000_AddProfileLegalCompliance.Designer.cs`
- Modify: `src/BricklePlatform.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`
- Test: `test/BricklePlatform.Test/Domain/ProfileComplianceEntityTests.cs`
- Test: `test/BricklePlatform.Test/Persistence/ProfileComplianceModelTests.cs`

**Interfaces:**
- Consumes: existing `User`, `UserDocument`, `DocumentTypeEnum`, and EF Core context.
- Produces: immutable legal/profile aggregates and indexed SQL schema used by all later tasks.

- [ ] **Step 1: Write failing domain invariant tests**

Test these concrete behaviors: rejection rejects blank reason/observation; an already reviewed submission rejects a second decision; a consent package rejects duplicate legal version IDs; staged documents reject a second aggregate binding.

- [ ] **Step 2: Run and verify RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~ProfileComplianceEntityTests`

Expected: FAIL to compile because the entities and enums do not exist.

- [ ] **Step 3: Implement exact enum and entity APIs**

```csharp
public enum LegalDocumentType { TermsAndConditions = 1, PrivacyPolicy = 2, ParticipationContract = 3 }
public enum UserDocumentKind { IdentityFront = 1, IdentityBack = 2, Signature = 3, LegacyIdentityDocument = 4 }
public enum ProfileSubmissionStatus { UnderReview = 1, Approved = 2, Rejected = 3 }

public static LegalDocumentVersion Create(
    LegalDocumentType documentType, string version, DateTime effectiveAtUtc,
    string canonicalUrl, string contentSha256, string locale,
    string jurisdiction, bool isActive, DateTime publishedAtUtc);

public static ConsentPackage Create(
    Guid userId, Guid signatureDocumentId, Guid idempotencyKey,
    string requestSha256, DateTime createdAtUtc);

public static ProfileSubmission Create(
    Guid userId, int revision, ProfileSnapshot snapshot,
    Guid identityFrontDocumentId, Guid identityBackDocumentId,
    Guid signatureDocumentId, Guid consentPackageId,
    Guid idempotencyKey, string requestSha256, DateTime submittedAtUtc);

public void Approve(string reviewerIdentity, DateTime reviewedAtUtc);
public void Reject(string reviewerIdentity, string reasonCode,
    string observation, DateTime reviewedAtUtc);
```

`ProfileSnapshot` contains first name, last name, phone, `DateOnly` birth date, nationality, residence country, `DocumentTypeEnum`, and document number. `UserLegalAcceptance.Create(...)` receives user/submission/package/version/signature IDs plus accepted UTC time, authenticated subject, IP, user agent, source, app version, and correlation ID.

- [ ] **Step 4: Extend `UserDocument` additively**

Add nullable `DocumentKind`, `ProfileSubmissionId`, `ConsentPackageId`, `BlobPath`, `MimeType`, `SizeBytes`, `ContentSha256`, and `StagedExpiresAtUtc`. Add `BindToProfileSubmission(Guid)` and `BindToConsentPackage(Guid)`; each clears expiration and rejects already-bound evidence.

- [ ] **Step 5: Write failing EF metadata tests**

Assert table names, foreign keys, and these indexes:

```text
LegalDocumentVersion: unique (document_type, version, locale, jurisdiction)
LegalDocumentVersion: unique filtered (document_type, locale, jurisdiction) WHERE is_active = 1
ProfileSubmission: unique (user_id, revision) and (user_id, idempotency_key)
ConsentPackage: unique (user_id, idempotency_key)
UserLegalAcceptance: unique (consent_package_id, legal_document_version_id)
UserDocument: indexes for owner/kind/binding and staged expiration
```

- [ ] **Step 6: Implement EF configurations and generate migration**

Run:

```bash
dotnet ef migrations add AddProfileLegalCompliance --project src/BricklePlatform.Infrastructure --startup-project src/BricklePlatform.Api --output-dir Migrations
```

Expected: EF generates one migration, designer, and snapshot update. Before continuing, verify the generated migration identifier is `20260812180000`; if the local EF timestamp differs, rename the migration class/file pair and `[Migration("...")]` metadata consistently to `20260812180000_AddProfileLegalCompliance` so all paths in this plan remain exact.

- [ ] **Step 7: Harden only the generated legacy-column migration block**

Use guarded SQL:

```sql
IF COL_LENGTH('[dbo].[UserDocument]', 'document_name') IS NULL
BEGIN
    IF COL_LENGTH('[dbo].[UserDocument]', 'name') IS NOT NULL
        EXEC sp_rename '[dbo].[UserDocument].[name]', 'document_name', 'COLUMN';
    ELSE
        THROW 51000, 'UserDocument requires name or document_name before compliance migration.', 1;
END;

UPDATE [dbo].[UserDocument]
SET [document_kind] = 4
WHERE [document_kind] IS NULL;
```

The extension columns are nullable for compatibility. Do not insert legal versions and do not read `terms_accepted`.

- [ ] **Step 8: Verify GREEN, inspect SQL, and commit**

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~ProfileComplianceEntityTests|FullyQualifiedName~ProfileComplianceModelTests"
dotnet ef migrations script --idempotent --project src/BricklePlatform.Infrastructure --startup-project src/BricklePlatform.Api --output /tmp/brickle-profile-legal.sql
```

Expected: tests PASS; SQL contains guarded normalization, four new tables, required indexes, and no legal-document seed statements.

```bash
git add src/BricklePlatform.Domain src/BricklePlatform.Infrastructure/Persistence src/BricklePlatform.Infrastructure/Migrations test/BricklePlatform.Test/Domain test/BricklePlatform.Test/Persistence
git commit -m "feat: add profile compliance persistence model"
```

### Task 4: Active Legal Document Registry

**Files:**
- Create: `src/BricklePlatform.Domain/DTOs/LegalDocumentVersionDto.cs`
- Create: `src/BricklePlatform.Domain/Interfaces/ILegalDocumentRepository.cs`
- Create: `src/BricklePlatform.Infrastructure/Repositories/LegalDocumentRepository.cs`
- Create: `src/BricklePlatform.Api/Application/Queries/LegalDocument/GetActiveLegalDocumentsQuery.cs`
- Create: `src/BricklePlatform.Api/Application/Handlers/LegalDocument/GetActiveLegalDocumentsQueryHandler.cs`
- Create: `src/BricklePlatform.Api/Controllers/LegalDocumentsController.cs`
- Modify: `src/BricklePlatform.Infrastructure/DependencyContainer.cs:30-65`
- Test: `test/BricklePlatform.Test/Handlers/LegalDocument/GetActiveLegalDocumentsQueryHandlerTests.cs`

**Interfaces:**
- Consumes: `LegalDocumentVersion` from Task 3.
- Produces: `GET /api/legal-documents/active` and repository methods used by submission/reconsent/investment.

- [ ] **Step 1: Write failing handler tests**

Test that exactly one active row for each enum type is returned in enum order; zero, two, or four rows throw `PROFILE_SUBMISSION_INCOMPLETE` rather than returning a partial registry.

- [ ] **Step 2: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~GetActiveLegalDocumentsQueryHandlerTests`

Expected: FAIL to compile because repository/query/handler are absent.

- [ ] **Step 3: Implement repository and query contracts**

```csharp
public interface ILegalDocumentRepository
{
    Task<IReadOnlyList<LegalDocumentVersion>> GetActiveRequiredAsync(
        string locale, string jurisdiction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LegalDocumentVersion>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}

public sealed record GetActiveLegalDocumentsQuery(
    string Locale = "es-CO", string Jurisdiction = "CO")
    : IRequest<IReadOnlyList<LegalDocumentVersionDto>>;
```

- [ ] **Step 4: Implement the authenticated endpoint**

```csharp
[ApiController]
[Route("api/legal-documents")]
[Authorize]
public sealed class LegalDocumentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<LegalDocumentVersionDto>>> GetActive(
        [FromQuery] string locale = "es-CO",
        [FromQuery] string jurisdiction = "CO",
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(
            new GetActiveLegalDocumentsQuery(locale, jurisdiction), cancellationToken));
}
```

DTO fields are ID, type, version, effective UTC date, canonical URL, SHA-256, locale, and jurisdiction.

- [ ] **Step 5: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~GetActiveLegalDocumentsQueryHandlerTests`

Expected: all selected tests PASS.

```bash
git add src/BricklePlatform.Domain/DTOs/LegalDocumentVersionDto.cs src/BricklePlatform.Domain/Interfaces/ILegalDocumentRepository.cs src/BricklePlatform.Infrastructure/Repositories/LegalDocumentRepository.cs src/BricklePlatform.Infrastructure/DependencyContainer.cs src/BricklePlatform.Api/Application/Queries/LegalDocument src/BricklePlatform.Api/Application/Handlers/LegalDocument src/BricklePlatform.Api/Controllers/LegalDocumentsController.cs test/BricklePlatform.Test/Handlers/LegalDocument
git commit -m "feat: expose active legal document registry"
```

### Task 5: Typed Reused Document Upload and Staged Cleanup

**Files:**
- Modify: `src/BricklePlatform.Api/Application/Dtos/UploadUserDocumentRequestDto.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/UserDocument/UploadUserDocumentCommandHandler.cs`
- Modify: document query handlers under `src/BricklePlatform.Api/Application/Handlers/UserDocument/`
- Modify: `src/BricklePlatform.Api/Controllers/UserController.cs:911-985`
- Modify: `src/BricklePlatform.Domain/DTOs/UserDocumentDto.cs`
- Modify: `src/BricklePlatform.Domain/Interfaces/IFileService.cs`
- Modify: `src/BricklePlatform.Domain/Interfaces/IUserDocumentRepository.cs`
- Create: `src/BricklePlatform.Domain/Models/EvidenceUploadResult.cs`
- Modify: blob and file infrastructure files from the File Map.
- Create: `src/BricklePlatform.Api/Application/Services/StagedDocumentCleanupService.cs`
- Create: `src/BricklePlatform.Api/Application/Services/StagedDocumentCleanupBackgroundService.cs`
- Test: `test/BricklePlatform.Test/Services/FileServiceEvidenceValidationTests.cs`
- Test: `test/BricklePlatform.Test/Handlers/UserDocument/UploadUserDocumentCommandHandlerTests.cs`
- Test: `test/BricklePlatform.Test/Services/StagedDocumentCleanupServiceTests.cs`

**Interfaces:**
- Consumes: `IRequestActor`, `UserDocumentKind`, and existing `POST /api/User/documents`.
- Produces: typed staged evidence IDs and metadata used by aggregate commands.

- [ ] **Step 1: Write failing content-validation tests**

Use in-memory complete PNG, JPEG, and WebP fixtures. Test valid content, a `.png` containing text, declared MIME mismatch, GIF/PDF rejection, empty input, and 5 MiB plus one byte.

- [ ] **Step 2: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~FileServiceEvidenceValidationTests`

Expected: current extension-only validation incorrectly accepts spoofed content.

- [ ] **Step 3: Implement evidence storage contract**

```csharp
public sealed record EvidenceUploadResult(
    string BlobPath, string ReadUrl, string MimeType,
    long SizeBytes, string ContentSha256);

Task<EvidenceUploadResult> UploadEvidenceAsync(
    Guid userId, Guid documentId, UserDocumentKind kind, Stream stream,
    string fileName, string declaredContentType,
    CancellationToken cancellationToken = default);
Task<string> GetReadUrlAsync(string blobPath, CancellationToken cancellationToken = default);
Task DeleteBlobAsync(string blobPath, CancellationToken cancellationToken = default);
```

Recognize PNG `89 50 4E 47`, JPEG `FF D8 FF`, and WebP `RIFF....WEBP`; store lowercase SHA-256. Build `user-documents/{userId:D}/{documentId:D}/{kind-lowercase}{extension}`.

- [ ] **Step 4: Write failing upload handler tests**

Assert mismatched JWT/body owner fails before file upload, `LegacyIdentityDocument` is rejected for new uploads, metadata is persisted, the document ID appears in `BlobPath`, and expiration is approximately UTC now plus 24 hours.

- [ ] **Step 5: Implement the additive upload request and response**

```csharp
public sealed class UploadUserDocumentRequestDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public UserDocumentKind DocumentKind { get; set; }
    public IFormFile File { get; set; } = null!;
}
```

Keep the route and existing fields. Add `DocumentKind`, `ProfileSubmissionId`, `MimeType`, `SizeBytes`, and `ContentSha256` to `UserDocumentDto`. Query handlers generate a fresh read URL from `BlobPath`, falling back to legacy `DocumentUrl` only when `BlobPath` is null.

- [ ] **Step 6: Write and implement staged cleanup tests**

```csharp
public interface IStagedDocumentCleanupService
{
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}
```

Load unbound expired rows, delete each blob, then delete its row. Retain the row when blob deletion fails. Register a hosted service that runs once after startup and then every hour.

- [ ] **Step 7: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~FileServiceEvidenceValidationTests|FullyQualifiedName~UploadUserDocumentCommandHandlerTests|FullyQualifiedName~StagedDocumentCleanupServiceTests"`

Expected: all selected tests PASS.

```bash
git add src/BricklePlatform.Domain/DTOs/UserDocumentDto.cs src/BricklePlatform.Domain/Interfaces src/BricklePlatform.Domain/Models/EvidenceUploadResult.cs src/BricklePlatform.Infrastructure/Constants/BlobConstants.cs src/BricklePlatform.Infrastructure/Interfaces/IBlobStorageRepository.cs src/BricklePlatform.Infrastructure/Repositories/BlobStorageRepository.cs src/BricklePlatform.Infrastructure/Repositories/UserDocumentRepository.cs src/BricklePlatform.Infrastructure/Services/FileService.cs src/BricklePlatform.Api/Application/Dtos/UploadUserDocumentRequestDto.cs src/BricklePlatform.Api/Application/Handlers/UserDocument src/BricklePlatform.Api/Application/Services/StagedDocumentCleanupService.cs src/BricklePlatform.Api/Application/Services/StagedDocumentCleanupBackgroundService.cs src/BricklePlatform.Api/Controllers/UserController.cs test/BricklePlatform.Test/Services test/BricklePlatform.Test/Handlers/UserDocument
git commit -m "feat: add typed staged evidence uploads"
```

### Task 6: Immutable Aggregate Profile Submission

**Files:**
- Create: `src/BricklePlatform.Domain/DTOs/ProfileSubmissionDtos.cs`
- Create: `src/BricklePlatform.Domain/Interfaces/IProfileComplianceRepository.cs`
- Create: `src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs`
- Create: `src/BricklePlatform.Api/Application/Commands/ProfileSubmission/CreateProfileSubmissionCommand.cs`
- Create: `src/BricklePlatform.Api/Application/Handlers/ProfileSubmission/CreateProfileSubmissionCommandHandler.cs`
- Create: `src/BricklePlatform.Api/Validators/ProfileSubmission/CreateProfileSubmissionRequestValidator.cs`
- Modify: `src/BricklePlatform.Api/Controllers/UserController.cs`
- Test: `test/BricklePlatform.Test/Handlers/ProfileSubmission/CreateProfileSubmissionCommandHandlerTests.cs`

**Interfaces:**
- Consumes: active registry, typed staged documents, actor audit metadata, EF aggregate model.
- Produces: `POST /api/User/{userId}/profile-submissions` with immutable revisions and idempotency.

- [ ] **Step 1: Write failing validation tests**

Cover every required profile field, age below 18, phone not matching `^\+?[0-9]{7,15}$`, invalid document enum, blank document number, repeated document IDs, acceptance count other than three, duplicate version IDs, and any `accepted=false`.

- [ ] **Step 2: Write failing handler tests**

Cover actor/route mismatch, missing evidence, wrong owner, wrong kind, stale legal IDs, document-number conflict, successful revision 1, revision increment after rejection, identical idempotent replay, changed-payload key reuse, transaction failure, and notification only after commit.

- [ ] **Step 3: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~CreateProfileSubmission`

Expected: FAIL to compile because contracts and handler do not exist.

- [ ] **Step 4: Implement exact request/command contracts**

```csharp
public sealed record CreateProfileSubmissionRequest(
    ProfileSnapshotDto Profile,
    ProfileSubmissionDocumentsDto Documents,
    IReadOnlyList<LegalAcceptanceRequestDto> LegalAcceptances);
public sealed record ProfileSubmissionDocumentsDto(
    Guid IdentityFrontId, Guid IdentityBackId, Guid SignatureId);
public sealed record LegalAcceptanceRequestDto(Guid DocumentVersionId, bool Accepted);
public sealed record CreateProfileSubmissionCommand(
    Guid UserId, Guid IdempotencyKey, CreateProfileSubmissionRequest Request)
    : IRequest<ProfileSubmissionDto>;
```

Repository methods:

```csharp
Task<ProfileSubmission?> GetByIdempotencyKeyAsync(Guid userId, Guid key, CancellationToken ct);
Task<int> GetNextRevisionAsync(Guid userId, CancellationToken ct);
Task<ProfileSubmissionDto> AddSubmissionAggregateAsync(
    ProfileSubmission submission, ConsentPackage package,
    IReadOnlyCollection<UserLegalAcceptance> acceptances,
    IReadOnlyCollection<UserDocument> documents, User user, CancellationToken ct);
Task<bool> HasCurrentAcceptancesAsync(Guid userId, CancellationToken ct);
```

- [ ] **Step 5: Implement deterministic idempotency hashing**

Trim profile strings, sort acceptance IDs, serialize a private normalized record with `System.Text.Json`, and SHA-256 its UTF-8 bytes. Same user/key/hash returns the existing DTO; same user/key/different hash throws `IDEMPOTENCY_KEY_REUSED`.

- [ ] **Step 6: Implement handler in authoritative order**

```text
1. Require JWT user equal to route user.
2. Resolve idempotency replay.
3. Validate user and document-number uniqueness excluding that user.
4. Load all three staged documents in one query.
5. Verify owner and exact IdentityFront/IdentityBack/Signature kinds.
6. Resolve the three active es-CO/CO versions server-side.
7. Require request IDs to equal active IDs as sets.
8. Create package, next submission revision, and three audit rows.
9. Bind documents and apply the profile snapshot to User.
10. Server-set basic=true, full=false, under-review=true.
11. Save all relational changes in one EF transaction.
12. Send under-review email/push only after successful commit.
```

- [ ] **Step 7: Add the route to existing `UserController`**

```csharp
[HttpPost("{userId:guid}/profile-submissions")]
public async Task<ActionResult<ProfileSubmissionDto>> CreateProfileSubmission(
    Guid userId,
    [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
    [FromBody] CreateProfileSubmissionRequest request,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(
        new CreateProfileSubmissionCommand(userId, idempotencyKey, request), cancellationToken);
    return StatusCode(StatusCodes.Status201Created, result);
}
```

- [ ] **Step 8: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~CreateProfileSubmission`

Expected: all selected tests PASS, including atomic rollback and idempotency.

```bash
git add src/BricklePlatform.Domain/DTOs/ProfileSubmissionDtos.cs src/BricklePlatform.Domain/Interfaces/IProfileComplianceRepository.cs src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs src/BricklePlatform.Infrastructure/DependencyContainer.cs src/BricklePlatform.Api/Application/Commands/ProfileSubmission src/BricklePlatform.Api/Application/Handlers/ProfileSubmission src/BricklePlatform.Api/Validators/ProfileSubmission/CreateProfileSubmissionRequestValidator.cs src/BricklePlatform.Api/Controllers/UserController.cs test/BricklePlatform.Test/Handlers/ProfileSubmission
git commit -m "feat: add aggregate profile submission"
```

### Task 7: Idempotent Legal Reconsent

**Files:**
- Create: `src/BricklePlatform.Api/Application/Commands/LegalAcceptance/CreateLegalAcceptanceCommand.cs`
- Create: `src/BricklePlatform.Api/Application/Handlers/LegalAcceptance/CreateLegalAcceptanceCommandHandler.cs`
- Create: `src/BricklePlatform.Api/Validators/ProfileSubmission/CreateLegalAcceptanceRequestValidator.cs`
- Modify: `src/BricklePlatform.Domain/DTOs/ProfileSubmissionDtos.cs`
- Modify: `src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs`
- Modify: `src/BricklePlatform.Api/Controllers/UserController.cs`
- Test: `test/BricklePlatform.Test/Handlers/LegalAcceptance/CreateLegalAcceptanceCommandHandlerTests.cs`

**Interfaces:**
- Consumes: Task 6 hashing/acceptance conventions and typed staged signature.
- Produces: `POST /api/User/{userId}/legal-acceptances` without changing approved profile state.

- [ ] **Step 1: Write failing reconsent tests**

Test signature ownership/kind, exactly three explicit active IDs, stale IDs returning `LEGAL_VERSION_OUTDATED`, package audit metadata, same-key replay, changed-payload conflict, and approved profile flags remaining unchanged.

- [ ] **Step 2: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~CreateLegalAcceptanceCommandHandlerTests`

Expected: FAIL to compile because reconsent contracts do not exist.

- [ ] **Step 3: Implement contract and transaction**

```csharp
public sealed record CreateLegalAcceptanceRequest(
    Guid SignatureId, IReadOnlyList<LegalAcceptanceRequestDto> LegalAcceptances);
public sealed record CreateLegalAcceptanceCommand(
    Guid UserId, Guid IdempotencyKey, CreateLegalAcceptanceRequest Request)
    : IRequest<ConsentPackageDto>;
```

Create one package and three acceptance rows with `ProfileSubmissionId = null`, bind the signature to `ConsentPackageId`, and preserve all user profile flags.

- [ ] **Step 4: Add the reused-controller route**

Add `[HttpPost("{userId:guid}/legal-acceptances")]` to `UserController`; bind `Idempotency-Key` exactly as Task 6 and require the actor user before dispatch.

- [ ] **Step 5: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~CreateLegalAcceptanceCommandHandlerTests`

Expected: all selected tests PASS.

```bash
git add src/BricklePlatform.Api/Application/Commands/LegalAcceptance src/BricklePlatform.Api/Application/Handlers/LegalAcceptance src/BricklePlatform.Api/Validators/ProfileSubmission/CreateLegalAcceptanceRequestValidator.cs src/BricklePlatform.Api/Controllers/UserController.cs src/BricklePlatform.Domain/DTOs/ProfileSubmissionDtos.cs src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs test/BricklePlatform.Test/Handlers/LegalAcceptance
git commit -m "feat: add legal reconsent packages"
```

### Task 8: Aggregate Admin Queue, Detail, and Review

**Files:**
- Create: profile queries/handlers under `src/BricklePlatform.Api/Application/{Queries,Handlers}/ProfileSubmission/`.
- Create: `src/BricklePlatform.Api/Application/Commands/ProfileSubmission/ReviewProfileSubmissionCommand.cs`
- Create: `src/BricklePlatform.Api/Validators/ProfileSubmission/ReviewProfileSubmissionRequestValidator.cs`
- Modify: `src/BricklePlatform.Api/Controllers/UserController.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/UserDocument/UpdateUserDocumentStatusCommandHandler.cs`
- Test: `test/BricklePlatform.Test/Handlers/ProfileSubmission/ProfileSubmissionQueryHandlerTests.cs`
- Test: `test/BricklePlatform.Test/Handlers/ProfileSubmission/ReviewProfileSubmissionCommandHandlerTests.cs`
- Modify: `test/BricklePlatform.Test/Handlers/UserDocument/UpdateUserDocumentStatusCommandHandlerTests.cs`

**Interfaces:**
- Consumes: immutable submission aggregate and `Administrator` policy.
- Produces: queue/detail and exact `PUT /api/User/profile-submissions/{id}/status` route.

- [ ] **Step 1: Write failing queue/detail tests**

Queue returns one row per submission filtered by enum status. Detail includes immutable snapshot, typed front/back/signature evidence, exact legal versions/URLs/hashes/accepted times, current review metadata, and prior user revisions.

- [ ] **Step 2: Write failing review tests**

Test complete approval, incomplete aggregate rejection, rejection requiring both reason and nonblank observation, wrong `expectedRevision`, already reviewed submission, atomic user flags, rollback, and notifications after commit.

- [ ] **Step 3: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~ProfileSubmissionQueryHandlerTests|FullyQualifiedName~ReviewProfileSubmissionCommandHandlerTests"`

Expected: FAIL to compile because query/review contracts do not exist.

- [ ] **Step 4: Implement review contract and concurrency**

```csharp
public sealed record ReviewProfileSubmissionRequest(
    ProfileSubmissionStatus Status, int ExpectedRevision,
    string? ReasonCode, string? Observation);
public sealed record ReviewProfileSubmissionCommand(
    Guid SubmissionId, ReviewProfileSubmissionRequest Request,
    string ReviewerIdentity) : IRequest<ProfileSubmissionDetailDto>;
```

Only `Approved` and `Rejected` are valid targets. Require current `UnderReview`, matching revision, and EF rowversion; map a failed conditional update or `DbUpdateConcurrencyException` to `STALE_PROFILE_SUBMISSION`. Approval revalidates snapshot, front/back/signature, package, and three distinct required legal types; the submitted historical versions need not still be active.

- [ ] **Step 5: Add exact admin routes to `UserController`**

```csharp
[HttpGet("profile-submissions")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public Task<IActionResult> GetProfileSubmissions([FromQuery] ProfileSubmissionStatus? status, ...);

[HttpGet("profile-submissions/{id:guid}")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public Task<IActionResult> GetProfileSubmission(Guid id, ...);

[HttpPut("profile-submissions/{id:guid}/status")]
[Authorize(Policy = AuthorizationPolicies.Administrator)]
public Task<IActionResult> UpdateProfileSubmissionStatus(
    Guid id, [FromBody] ReviewProfileSubmissionRequest request, ...);
```

The effective review path is exactly `PUT /api/User/profile-submissions/{id}/status`.

- [ ] **Step 6: Derive user state atomically**

Approval sets basic/full profile true and under-review false. Rejection sets full false and under-review false while retaining valid basic profile data. Save submission and user in one transaction; send approval/rejection notifications afterward.

- [ ] **Step 7: Neutralize legacy row review**

Change `UpdateUserDocumentStatusCommandHandler` to update only the legacy row. Remove every mutation of user profile flags and profile-wide approval/rejection notification from that handler.

- [ ] **Step 8: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~ProfileSubmission|FullyQualifiedName~UpdateUserDocumentStatusCommandHandlerTests"`

Expected: all selected tests PASS; approving one legacy document does not approve a user.

```bash
git add src/BricklePlatform.Api/Application/Queries/ProfileSubmission src/BricklePlatform.Api/Application/Commands/ProfileSubmission/ReviewProfileSubmissionCommand.cs src/BricklePlatform.Api/Application/Handlers/ProfileSubmission src/BricklePlatform.Api/Validators/ProfileSubmission/ReviewProfileSubmissionRequestValidator.cs src/BricklePlatform.Api/Controllers/UserController.cs src/BricklePlatform.Api/Application/Handlers/UserDocument/UpdateUserDocumentStatusCommandHandler.cs src/BricklePlatform.Infrastructure/Repositories/ProfileComplianceRepository.cs test/BricklePlatform.Test/Handlers/ProfileSubmission test/BricklePlatform.Test/Handlers/UserDocument/UpdateUserDocumentStatusCommandHandlerTests.cs
git commit -m "feat: add aggregate profile administration"
```

### Task 9: Server-Owned Profile State and Authentication Consent

**Files:**
- Modify: `src/BricklePlatform.Domain/Entities/User.cs`
- Modify: `src/BricklePlatform.Domain/DTOs/UpdateUserDto.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/User/UpdateUserHandler.cs`
- Modify: `src/BricklePlatform.Api/Validators/UserValidation/UpdateUserDtoValidation.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/User/CreateUserHandler.cs`
- Modify: `src/BricklePlatform.Api/Controllers/AuthController.cs:78-96,184-201`
- Modify: auth/user tests already present under `test/BricklePlatform.Test/`.

**Interfaces:**
- Consumes: aggregate state transitions from Tasks 6 and 8.
- Produces: server-only completion/review mutation and no fabricated authentication consent.

- [ ] **Step 1: Write failing behavior tests**

Test general update cannot change `TermsAccepted`, `IsBasicProfileComplete`, `IsFullProfileComplete`, or `IsProfileUnderReview`; Google and OTP-created users have `TermsAccepted == false`; user update route rejects a mismatched JWT subject.

- [ ] **Step 2: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~AuthControllerTests|FullyQualifiedName~CreateUserHandlerTest|FullyQualifiedName~UpdateUser|FullyQualifiedName~UserControllerTests"`

Expected: authentication tests expose current `termsAccepted: true` and update tests expose writable flags.

- [ ] **Step 3: Remove client-writable properties and handler mapping**

Remove the four properties from `UpdateUserDto`, their validator rules, and their arguments in `UpdateUserHandler`. Keep response fields for binary/client compatibility.

- [ ] **Step 4: Add explicit entity transitions**

```csharp
public void ApplySubmittedProfile(ProfileSnapshot snapshot);
public void MarkProfileUnderReview();
public void MarkProfileApproved();
public void MarkProfileRejected();
```

Make the three profile flags private-set. Submission/review handlers use only these methods.

- [ ] **Step 5: Stop fabricated consent**

Set `termsAccepted: false` for Google and OTP user creation. Regular `CreateUserHandler` may preserve the compatibility request property but passes `false` to `User.Create`; eligibility never reads the boolean.

- [ ] **Step 6: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~AuthControllerTests|FullyQualifiedName~CreateUserHandlerTest|FullyQualifiedName~UserControllerTests"`

Expected: all selected tests PASS.

```bash
git add src/BricklePlatform.Domain/Entities/User.cs src/BricklePlatform.Domain/DTOs/UpdateUserDto.cs src/BricklePlatform.Api/Application/Handlers/User src/BricklePlatform.Api/Validators/UserValidation/UpdateUserDtoValidation.cs src/BricklePlatform.Api/Controllers/AuthController.cs src/BricklePlatform.Api/Controllers/UserController.cs test/BricklePlatform.Test/Handlers/User test/BricklePlatform.Test/Controllers/AuthControllerTests.cs test/BricklePlatform.Test/Controllers/UserControllerTests.cs
git commit -m "fix: make profile state and consent server owned"
```

### Task 10: Shared Investment Policy and Pre-Side-Effect Enforcement

**Files:**
- Create: `src/BricklePlatform.Domain/Models/InvestmentPolicyModels.cs`
- Create: `src/BricklePlatform.Domain/Interfaces/IInvestmentPolicy.cs`
- Create: `src/BricklePlatform.Api/Application/Services/InvestmentPolicy.cs`
- Modify: `src/BricklePlatform.Infrastructure/DependencyContainer.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/Campaign/CommitFundsHandler.cs`
- Modify: `src/BricklePlatform.Api/Application/Handlers/Investment/CreateInvestmentCommandHandler.cs`
- Modify: `src/BricklePlatform.Api/Controllers/InvestmentController.cs:46-83`
- Test: `test/BricklePlatform.Test/Handlers/Investment/InvestmentPolicyTests.cs`
- Test: `test/BricklePlatform.Test/Handlers/Campaign/CommitFundsHandlerTests.cs`

**Interfaces:**
- Consumes: user/profile/legal repositories, leasing `PricePerToken`, campaign status, and inventory.
- Produces: one reusable policy used by campaign purchase and retained direct/internal creation.

- [ ] **Step 1: Write failing arithmetic and eligibility tests**

```csharp
[Theory]
[InlineData(1000000, 1)]
[InlineData(400000, 3)]
[InlineData(333333.33, 4)]
public void MinimumBricksUsesCeiling(decimal pricePerToken, int expected)
{
    Assert.Equal(expected, _policy.CalculateMinimumBricks(pricePerToken));
}
```

Also test COP `999999m` rejected and `1000000m` accepted; zero, negative, and fractional bricks; amount mismatch; wallet mismatch; no approved latest submission; missing acceptance for one current version; newly activated version; inactive leasing; non-active campaign; and insufficient inventory.

- [ ] **Step 2: Run RED**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~InvestmentPolicyTests`

Expected: FAIL to compile because policy interfaces do not exist.

- [ ] **Step 3: Implement exact policy contracts**

```csharp
public sealed record InvestmentPolicyRequest(
    Guid UserId, Guid LeasingId, string SenderWallet,
    decimal Amount, decimal Bricks);
public sealed record ValidatedInvestment(
    User User, Leasing Leasing, Campaign Campaign, int Bricks,
    decimal AuthoritativeAmount, int MinimumBricks);
public interface IInvestmentPolicy
{
    Task<ValidatedInvestment> ValidateAsync(
        InvestmentPolicyRequest request, CancellationToken cancellationToken = default);
    int CalculateMinimumBricks(decimal pricePerToken);
}
```

```csharp
public int CalculateMinimumBricks(decimal pricePerToken)
{
    if (pricePerToken <= 0m)
        throw new ArgumentOutOfRangeException(nameof(pricePerToken));
    return checked((int)decimal.Ceiling(1_000_000m / pricePerToken));
}
```

- [ ] **Step 4: Implement validation order and stable errors**

```text
1. INVALID_BRICKS_COUNT: bricks <= 0 or decimal.Truncate(bricks) != bricks.
2. Wallet ownership: authenticated user owns sender wallet, ordinal-ignore-case.
3. PROFILE_NOT_APPROVED: latest submission is not Approved.
4. LEGAL_REACCEPTANCE_REQUIRED: any currently active required version lacks acceptance.
5. Campaign/leasing must exist, be active/purchasable, and have inventory.
6. INVESTMENT_BELOW_MINIMUM: amount < 1_000_000m.
7. INVESTMENT_AMOUNT_BRICKS_MISMATCH: amount != bricks * PricePerToken.
```

Use `LEGAL_VERSION_OUTDATED` only when submission/reconsent explicitly supplies stale version IDs. Return `PROFILE_SUBMISSION_INCOMPLETE` for incomplete aggregate construction and the document-specific codes from Global Constraints for evidence failures.

- [ ] **Step 5: Write failing `CommitFundsHandler` order tests**

Mock `IWebHookService`. Assert every policy failure results in `ProcessCommitFunds` `Times.Never`; success calls policy before webhook and uses validated amount/bricks.

- [ ] **Step 6: Refactor campaign purchase before external effects**

Inject `IInvestmentPolicy` and `IRequestActor`. Resolve user from JWT rather than sender-wallet lookup. Validate before permit/webhook work. Convert only validated amount:

```csharp
long baseUnits = decimal.ToInt64(validated.AuthoritativeAmount * 1_000_000m);
```

Use `validated.Bricks` as `int` for investment and inventory updates; never truncate request `TotalTokens`.

- [ ] **Step 7: Close direct investment bypass**

Apply `[Authorize(Policy = AuthorizationPolicies.Administrator)]` specifically to `InvestmentController.CreateInvestment`. Also invoke `IInvestmentPolicy.ValidateAsync` in `CreateInvestmentCommandHandler` using the referenced user's registered wallet, so direct mediator/internal calls cannot bypass policy. Do not introduce a policy-skip flag.

- [ ] **Step 8: Verify GREEN and commit**

Run: `dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~InvestmentPolicyTests|FullyQualifiedName~CommitFundsHandlerTests|FullyQualifiedName~CreateInvestment"`

Expected: all selected tests PASS; every eligibility failure has zero webhook calls.

```bash
git add src/BricklePlatform.Domain/Models/InvestmentPolicyModels.cs src/BricklePlatform.Domain/Interfaces/IInvestmentPolicy.cs src/BricklePlatform.Api/Application/Services/InvestmentPolicy.cs src/BricklePlatform.Api/Application/Handlers/Campaign/CommitFundsHandler.cs src/BricklePlatform.Api/Application/Handlers/Investment/CreateInvestmentCommandHandler.cs src/BricklePlatform.Api/Controllers/InvestmentController.cs src/BricklePlatform.Infrastructure/DependencyContainer.cs test/BricklePlatform.Test/Handlers/Investment test/BricklePlatform.Test/Handlers/Campaign
git commit -m "fix: enforce investment policy before external effects"
```

### Task 11: Contract, Migration, and Full Regression Verification

**Files:**
- Test: `test/BricklePlatform.Test/Controllers/ProfileComplianceRouteTests.cs`
- Verify: all files listed above; no production behavior is introduced in this task.

**Interfaces:**
- Consumes: complete API implementation.
- Produces: executable evidence that routes, policies, migrations, and regression suite match the approved design.

- [ ] **Step 1: Write route/policy reflection tests**

Assert these exact effective routes and attributes:

```text
POST /api/User/documents                       authenticated, actor owner check
GET  /api/User/{userId}/documents              authenticated, actor owner check
GET  /api/User/documents/all                   Administrator
POST /api/User/{userId}/profile-submissions    authenticated, Idempotency-Key
POST /api/User/{userId}/legal-acceptances      authenticated, Idempotency-Key
GET  /api/User/profile-submissions             Administrator
GET  /api/User/profile-submissions/{id}        Administrator
PUT  /api/User/profile-submissions/{id}/status Administrator
POST /api/Investment                           Administrator plus shared policy
```

- [ ] **Step 2: Run focused compliance tests**

Run:

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~Profile|FullyQualifiedName~Legal|FullyQualifiedName~UserDocument|FullyQualifiedName~Investment|FullyQualifiedName~Authorization|FullyQualifiedName~ApiError"
```

Expected: all selected tests PASS with 0 failures.

- [ ] **Step 3: Generate and inspect idempotent migration SQL**

Run:

```bash
dotnet ef migrations script --idempotent --project src/BricklePlatform.Infrastructure --startup-project src/BricklePlatform.Api --output /tmp/brickle-profile-legal.sql
```

Expected: script contains guarded `name`/`document_name` normalization, nullable document extensions, four new tables, filtered active-version uniqueness, revision/idempotency constraints, and no legal seeds or `terms_accepted` conversion.

- [ ] **Step 4: Run the complete test suite**

Run: `dotnet test BricklePlatform.Api.sln --no-restore`

Expected: all tests PASS with 0 failures. Existing baseline warnings may remain; files added by this plan introduce no new warnings.

- [ ] **Step 5: Build Release**

Run: `dotnet build BricklePlatform.Api.sln --configuration Release --no-restore`

Expected: build succeeds with 0 errors.

- [ ] **Step 6: Verify the production migration prerequisite before deployment**

Run against the target SQL Server:

```sql
SELECT c.name, c.is_nullable, t.name AS data_type
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[UserDocument]')
ORDER BY c.column_id;
```

Expected: `[dbo].[UserDocument]` exists and contains `name` or `document_name`. Stop deployment if neither exists.

- [ ] **Step 7: Verify supplied official legal rows without inventing values**

After legal/product supplies and operations inserts the official artifacts, run:

```sql
SELECT document_type, version, effective_at_utc, canonical_url,
       content_sha256, locale, jurisdiction, is_active
FROM dbo.LegalDocumentVersion
WHERE is_active = 1
ORDER BY document_type;
```

Expected: exactly three rows with document types 1, 2, and 3; each row uses the supplied URL/version/date and a 64-character SHA-256. Do not deploy updated clients until this query succeeds.

- [ ] **Step 8: Commit verification tests**

```bash
git add test/BricklePlatform.Test/Controllers/ProfileComplianceRouteTests.cs
git commit -m "test: verify profile legal API contracts"
```

- [ ] **Step 9: Inspect the final change set**

Run:

```bash
git status --short
git diff --stat HEAD~11..HEAD
git log --oneline -11
```

Expected: only intended API source, migration, and tests; no mobile/admin files, no official legal values invented in source, and eleven focused commits.

## Stable Error Catalogue

| Code | HTTP | Message |
|---|---:|---|
| `INVESTMENT_BELOW_MINIMUM` | 422 | Investment amount must be at least COP 1,000,000. |
| `INVESTMENT_AMOUNT_BRICKS_MISMATCH` | 422 | Amount must equal bricks multiplied by the asset price per token. |
| `INVALID_BRICKS_COUNT` | 422 | Bricks must be a positive whole number. |
| `PROFILE_NOT_APPROVED` | 403 | The latest profile submission is not approved. |
| `LEGAL_REACCEPTANCE_REQUIRED` | 403 | Acceptance of all current legal documents is required. |
| `LEGAL_VERSION_OUTDATED` | 409 | The submitted legal document versions are no longer active. |
| `PROFILE_SUBMISSION_INCOMPLETE` | 422 | Profile submission data or evidence is incomplete. |
| `DOCUMENT_KIND_MISMATCH` | 422 | A staged document does not have the required document kind. |
| `DOCUMENT_NOT_OWNED` | 403 | A staged document does not belong to the authenticated user. |
| `STALE_PROFILE_SUBMISSION` | 409 | The profile submission was already reviewed or the expected revision is stale. |
| `IDEMPOTENCY_KEY_REUSED` | 409 | The idempotency key was already used with a different request. |

Every handler throws `ApiErrorException` with one of these codes where applicable. Every serialized response exposes it as JSON property `code`.

## Acceptance Matrix

| Approved requirement | Implemented and proved by |
|---|---|
| Versioned active legal registry | Tasks 3-4 |
| Typed staged upload through existing route | Task 5 |
| Ownership/content/size/hash validation | Tasks 2 and 5 |
| Immutable idempotent profile aggregate | Task 6 |
| Exactly three acceptances and one signature | Tasks 3, 6, and 7 |
| Reconsent preserves profile approval | Task 7 |
| Aggregate admin queue/detail/review | Task 8 |
| Exact aggregate PUT status route | Tasks 8 and 11 |
| Legacy row review cannot approve profile | Task 8 |
| Server-owned flags and no fabricated auth consent | Task 9 |
| Named administrative policy and JWT scope | Tasks 1-2 |
| Shared minimum/brick/wallet/profile/legal/campaign policy | Task 10 |
| Validation before blockchain/webhook | Task 10 |
| Stable `code` error contract | Tasks 1 and 10 |
| Safe migration with no invented legal records | Tasks 3 and 11 |
