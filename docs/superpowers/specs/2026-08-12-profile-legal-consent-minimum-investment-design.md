# Profile, Legal Consent, and Minimum Investment Design

Date: 2026-08-12

Repositories:

- API: `/Volumes/B/Projects/Brickle/BricklePlatform-api`
- Mobile: `/Volumes/B/Projects/Brickle/bloom-mobile-app`
- Admin: `/Volumes/B/Projects/Brickle/brickle-admin`

## Objective

Implement two coordinated product rules:

1. Every investment operation must be at least COP 1,000,000. The number of bricks required for an asset is calculated from its `PricePerToken`; one brick is not globally equivalent to one COP.
2. A profile submission must include valid profile data, identity document front and back, separate acceptance of Terms and Conditions, Privacy Policy, and the platform-wide Participation Contract, plus one handwritten signature that supports the three acceptances.

The API is authoritative for profile state, legal evidence, document ownership, investment eligibility, and the amount-to-bricks relationship. Mobile validation improves the experience but cannot replace API enforcement. The existing admin document section becomes the integral profile review queue.

## Confirmed Product Decisions

- The COP 1,000,000 minimum applies to every investment operation.
- Minimum visible bricks are calculated as `ceil(1_000_000 / PricePerToken)`.
- The three legal documents require separate affirmative acceptances.
- One handwritten signature supports the three acceptances as one consent package.
- The Participation Contract is platform-wide, not asset-specific.
- Initial profile submission requires profile data, identity front, identity back, legal acceptances, and signature.
- Existing approved users without current evidence may navigate but must reconsent before another investment.
- A new active version of any required legal document requires acceptance and a new signature before investing.
- Official document URLs, versions, effective dates, and SHA-256 hashes will be supplied before deployment.
- The existing admin document section will approve or reject the complete profile submission, not individual files.

## Current-State Problems

### Investment

- Mobile permits buying one brick and calculates `amount = bricks * pricePerToken`.
- `commitFunds` accepts client-provided amount and bricks independently.
- The API does not verify their relationship, the COP minimum, profile approval, or current legal acceptance.
- The direct investment endpoint provides another path around campaign purchase policy.
- Fractional bricks can pass validation and be truncated while inventory is decremented.
- Blockchain execution currently occurs before all authoritative checks.

### Profile and legal consent

- `User.TermsAccepted` is one mutable, unversioned boolean.
- Some authentication paths set it to true without affirmative acceptance.
- Complete-profile mobile code hardcodes `termsAccepted: true`.
- Profile data and identity upload are separate state transitions.
- The client can write profile completion and review flags.
- One approved document currently marks the complete profile approved.
- There is no Participation Contract model, acceptance history, signature evidence, or legal document registry.

### Authorization and administration

- User IDs supplied in routes or multipart bodies are not consistently checked against the JWT subject.
- Admin document endpoints rely on generic authorization rather than a specific administrative policy.
- The admin uses an API key and reviews flat document rows. It does not display an immutable profile snapshot or legal evidence.

## Architecture

The solution has four bounded capabilities:

1. A versioned legal document registry.
2. Staged document uploads using the existing user document endpoint.
3. An immutable, idempotent profile submission aggregate reviewed through the existing admin section.
4. A shared investment policy enforced before blockchain execution.

## Domain Model

### LegalDocumentVersion

Represents the exact legal artifact presented to users.

Required fields:

- `Id`
- `DocumentType`: `TermsAndConditions`, `PrivacyPolicy`, or `ParticipationContract`
- `Version`
- `EffectiveAtUtc`
- `CanonicalUrl`
- `ContentSha256`
- `Locale`
- `Jurisdiction`
- `IsActive`
- `PublishedAtUtc`
- `RetiredAtUtc`
- standard creation/update timestamps

Rules:

- At most one active version exists per document type, locale, and jurisdiction.
- A profile submission or reconsent package must contain exactly the three versions active at submission time.
- Historical versions and their URLs/hashes remain immutable.

### ProfileSubmission

Represents one immutable review revision.

Required fields:

- `Id`
- `UserId`
- `Revision`
- `Status`: `UnderReview`, `Approved`, or `Rejected`
- complete profile snapshot as structured fields or a normalized owned record
- `IdentityFrontDocumentId`
- `IdentityBackDocumentId`
- `SignatureDocumentId`
- `IdempotencyKey`
- `SubmittedAtUtc`
- `ReviewedAtUtc`
- `ReviewerIdentity`
- `RejectionReasonCode`
- `ReviewObservation`
- standard timestamps

Rules:

- `(UserId, Revision)` and `(UserId, IdempotencyKey)` are unique.
- Submitted profile data and evidence are not mutated in place.
- A rejected user resubmits as a new revision.
- Approval or rejection is an integral decision for the submission.
- Rejection requires a reason and nonblank observation.
- Review commands use an expected revision or concurrency token to reject stale decisions.

### UserLegalAcceptance

Append-only evidence for one accepted legal version.

Required fields:

- `Id`
- `UserId`
- `ProfileSubmissionId` for onboarding, nullable for reconsent
- `ConsentPackageId`
- `LegalDocumentVersionId`
- `SignatureDocumentId`
- `AcceptedAtUtc`
- authenticated subject
- request IP
- user agent
- app/source identifier and version when available
- correlation ID

Rules:

- One consent package has exactly three acceptance rows and one shared signature.
- Acceptance is not inferred from viewing a document or from `User.TermsAccepted`.
- Acceptance rows are immutable.
- A unique constraint prevents duplicate legal versions in one consent package.

### UserDocument extensions

Reuse the current document table and endpoints. Add nullable fields first for migration compatibility:

- `DocumentKind`: `IdentityFront`, `IdentityBack`, `Signature`, or `LegacyIdentityDocument`
- `ProfileSubmissionId`
- `BlobPath`, allowing short-lived URLs to replace persisted long-lived SAS URLs
- MIME type, size, and SHA-256 where the storage abstraction can provide them

Legacy documents remain visible. Existing ambiguous `Identity Document` rows are classified conservatively as legacy and cannot alone satisfy a new profile submission.

## API Contract

### Active legal documents

```http
GET /api/legal-documents/active
```

Returns exactly one active version for each required type, including IDs, versions, effective dates, URLs, and hashes. Mobile must render the returned URLs rather than hardcoded legal constants.

### Existing document upload

Keep:

```http
POST /api/User/documents
Content-Type: multipart/form-data
```

Fields:

- Existing `UserId`, retained temporarily for compatibility
- Existing `Name`
- Existing `File`
- New required `DocumentKind` for updated clients

For JWT users, the API derives identity from `sub` and rejects a mismatched `UserId`. Admin/API-key behavior is governed by an explicit administrative policy.

The response remains an additive `UserDocumentDto`, now including `documentKind` and `profileSubmissionId`. Updated mobile code stores the returned document ID, not only its URL.

Uploads are staged until linked by a profile submission or reconsent package. The API validates ownership, supported kind, extension, content signature/MIME, file size, and uniqueness. Blob names use collision-resistant IDs. Unlinked staged files have a retention/cleanup policy.

### Profile submission

```http
POST /api/User/{userId}/profile-submissions
Idempotency-Key: <uuid>
Content-Type: application/json
```

Request:

```json
{
  "profile": {
    "firstName": "...",
    "lastName": "...",
    "phoneNumber": "...",
    "dateOfBirth": "YYYY-MM-DD",
    "nationality": "...",
    "countryOfResidence": "...",
    "documentType": 1,
    "documentNumber": "..."
  },
  "documents": {
    "identityFrontId": "uuid",
    "identityBackId": "uuid",
    "signatureId": "uuid"
  },
  "legalAcceptances": [
    { "documentVersionId": "uuid", "accepted": true },
    { "documentVersionId": "uuid", "accepted": true },
    { "documentVersionId": "uuid", "accepted": true }
  ]
}
```

The server:

1. Resolves the user from JWT and verifies the route ID.
2. Validates all profile fields, adulthood, phone format, document type/number, and uniqueness.
3. Verifies all document IDs belong to the user and match front, back, and signature kinds.
4. Resolves active legal versions server-side and requires exact agreement with the request.
5. Creates one consent package and three acceptance rows.
6. Creates the immutable submission and binds the staged documents.
7. Server-sets basic profile complete, full profile incomplete, and under-review state.
8. Commits relational state atomically and emits notifications after commit.

Repeating the same user/idempotency key returns the existing result. Reusing the key with a different payload returns a conflict.

### Reconsent

```http
POST /api/User/{userId}/legal-acceptances
Idempotency-Key: <uuid>
```

The request contains a staged signature ID and three explicit active document version acceptances. It does not require new identity files or alter an approved profile. The server applies the same ownership, active-version, evidence, audit, and idempotency rules.

### Admin review

Preserve existing document list routes for legacy views and compatibility:

- `GET /api/User/{userId}/documents`
- `GET /api/User/documents/all`

Add aggregate routes consumed by the transformed admin section:

```http
GET /api/User/profile-submissions?status=UnderReview
GET /api/User/profile-submissions/{submissionId}
PUT /api/User/profile-submissions/{submissionId}/status
```

Review request:

```json
{
  "status": "Approved",
  "expectedRevision": 2,
  "reasonCode": null,
  "observation": null
}
```

Approval validates that profile data, identity front/back, one signature, and the three submitted legal versions remain internally valid. It then derives the user flags server-side. Rejection requires a reason code and observation, sets the submission rejected, and clears under-review without marking the user approved.

The existing `PUT /api/User/documents/{id}/status` remains temporarily available for legacy records but no longer changes the whole user profile. New submissions are reviewed only through the aggregate endpoint.

### General user update

Remove completion/review flags and legal acceptance from client-writable profile update contracts. If they remain temporarily for binary compatibility, handlers ignore them and response documentation marks them server-owned.

Authentication-created users must not be marked as having accepted legal documents.

## Profile State

The API owns state transitions:

```text
Incomplete -> UnderReview -> Approved
                         -> Rejected -> UnderReview (new revision)
```

- `Incomplete`: mandatory profile data or evidence has not been submitted.
- `UnderReview`: one complete immutable submission awaits an admin decision.
- `Approved`: the latest reviewed submission was approved.
- `Rejected`: the latest reviewed submission was rejected and a new revision is required.

Legal currency is a separate eligibility concern. An approved profile can remain approved while investment is blocked because active legal versions require reconsent.

## Investment Policy

Create a shared application/domain policy used by `CommitFundsHandler` and any retained internal investment creation path.

### Rules

- Currency rule: minimum `Amount` is COP 1,000,000 per operation.
- Bricks are indivisible positive integers.
- Expected bricks are derived from the amount and asset price under a single documented rounding rule.
- For the minimum selector, `minimumBricks = ceil(1_000_000 / PricePerToken)`.
- For a selected brick count, authoritative amount is `bricks * PricePerToken`.
- A request is rejected if its amount and bricks do not match the authoritative calculation.
- The authenticated user must own the sender wallet.
- The latest profile submission must be approved.
- The user must have accepted all currently active required legal versions.
- The campaign must be active/purchasable and have sufficient inventory.
- All checks occur before permit/webhook/blockchain execution.
- The direct `POST /api/Investment` route is restricted to an explicit internal/admin policy or removed from public authentication.

Money remains `decimal` in API/domain code and must not use floating-point arithmetic. Blockchain base-unit conversion occurs only after policy validation.

### Error codes

Return a stable machine-readable error code and human-readable message:

- `INVESTMENT_BELOW_MINIMUM`
- `INVESTMENT_AMOUNT_BRICKS_MISMATCH`
- `INVALID_BRICKS_COUNT`
- `PROFILE_NOT_APPROVED`
- `LEGAL_REACCEPTANCE_REQUIRED`
- `LEGAL_VERSION_OUTDATED`
- `PROFILE_SUBMISSION_INCOMPLETE`
- `DOCUMENT_KIND_MISMATCH`
- `DOCUMENT_NOT_OWNED`
- `STALE_PROFILE_SUBMISSION`
- `IDEMPOTENCY_KEY_REUSED`

Mobile must preserve these API errors instead of reducing failures to `false`.

## Mobile Design

### Initial profile flow

The existing complete-profile wizard becomes:

1. Personal data.
2. Identification data.
3. Identity front and back upload.
4. Three separate legal acceptance controls linked to API-provided active documents.
5. Handwritten signature pad.
6. Summary and final submission.

The app uploads the three artifacts through the existing document upload route, records their IDs, and performs one final profile submission. Failed final submission is retryable with the same idempotency key. The UI must not locally mark the profile complete or under review until the API confirms the aggregate submission.

### Existing users and reconsent

Existing users may navigate normally. Investment eligibility returned by or derived from the API identifies missing current legal evidence. Attempting to invest opens a legal reconsent flow showing all three active documents, three separate acceptances, and a new handwritten signature. Identity documents are not requested again.

### Investment UI

- Display the COP 1,000,000 minimum.
- Calculate and display minimum bricks for the selected asset.
- Do not enable purchase below the minimum.
- Validate again in the purchase helper before creating a permit.
- Map profile, consent, and minimum error codes to corrective navigation/messages.
- API validation remains authoritative.

## Admin Design

Transform the existing `/documents` section rather than adding a parallel review module.

### Queue

- One row per profile submission, not per file.
- Show user, revision, submission date, current state, and evidence completeness.
- Dashboard pending count is the number of under-review submissions.
- Existing document filters may remain as a legacy subsection if operationally required.

### Detail and decision

The detail view shows:

- immutable profile snapshot
- identity front and back side by side
- signature preview
- the three legal document types, exact versions, URLs/hashes, and accepted timestamps
- prior submission/review history
- reviewer metadata after decision

The existing approve/reject interaction becomes one integral submission decision. Rejection requires a structured reason and nonblank observation. The API response is the source of the resulting state. A stale revision conflict requires refreshing before another decision.

### Admin authorization

The API continues to support the admin's current API-key integration during this scope, but document-all, submission queue/detail, and review endpoints require a named administrative authorization policy. Caller-supplied owner email is audit metadata only and does not establish identity. Moving the browser-held API key to stronger admin authentication/BFF is recommended separately because it is broader than these product requirements.

## Security and Data Integrity

- Compare JWT `sub` with every user-scoped route/body ID.
- Restrict global document listing and review mutations to the admin policy.
- Never derive acceptance from a mutable user boolean.
- Record timestamps in UTC and preserve legal evidence immutably.
- Generate short-lived signed document URLs where feasible; do not expose permanent public files.
- Validate file content, not only extension.
- Use collision-resistant blob keys and clean abandoned staged uploads.
- Make SQL submission and review transitions transactional.
- Use optimistic concurrency for admin review.
- Derive user profile flags server-side.
- Perform all investment checks before external financial side effects.

## Migration and Compatibility

1. Verify deployed `user_documents` schema because migrations and EF mapping disagree on `name` versus `document_name` and nullability.
2. Add document-kind/submission columns as nullable.
3. Add legal version, submission, and acceptance tables with indexes and constraints.
4. Seed the three official active legal versions only when URLs, versions, dates, and hashes are supplied.
5. Preserve historical document rows and DTO fields.
6. Do not convert `TermsAccepted = true` into legal evidence.
7. Keep the boolean temporarily for old clients, but make eligibility depend on acceptance rows.
8. Deploy API before updated mobile/admin clients because client changes depend on additive routes and fields.
9. After client rollout and observation, remove or lock down legacy client-controlled completion behavior.

## Testing Strategy

### API

- COP 999,999 is rejected; COP 1,000,000 is accepted when all other conditions hold.
- Minimum bricks use ceiling division for representative `PricePerToken` values.
- Manipulated amount/bricks and fractional bricks are rejected.
- Profile/legal/wallet/campaign validation runs before blockchain invocation.
- Direct investment cannot bypass the shared policy.
- Profile submission requires front, back, signature, and exactly three current acceptances.
- Document ownership and kind mismatches are rejected.
- Idempotent retries return the prior result; changed payload reuse conflicts.
- Authentication-created users receive no fabricated legal acceptance.
- New legal versions force reconsent without clearing profile approval.
- Aggregate approval requires a complete submission and updates derived flags atomically.
- Aggregate rejection requires reason/observation and does not approve the user.
- Legacy document status updates do not approve a profile.
- User operations reject mismatched JWT IDs; admin operations reject non-admin authentication.
- Migrations work against both observed legacy document column variants or fail safely with a documented prerequisite.

### Mobile

- Wizard cannot submit with missing profile fields, either identity side, any acceptance, or signature.
- Upload IDs are retained and sent in the final request.
- Submission retries reuse idempotency keys.
- Existing users are routed to reconsent only when investing.
- Purchase controls enforce/display the minimum and calculated bricks.
- API error codes produce the correct message or corrective route.

### Admin

- Queue groups by submission and dashboard counts submissions.
- Detail renders all profile and evidence sections.
- Approval sends the expected revision and refreshes derived state.
- Rejection requires reason and observation.
- Stale-review conflict triggers refresh.
- Legacy documents remain visible without being treated as complete submissions.
- Admin routes send the required API-key policy headers during the compatibility period.

## Deployment Prerequisites

- Official artifact for each legal document.
- Stable version identifier, effective date, canonical URL, locale/jurisdiction, and SHA-256 for each artifact.
- Confirmed file size and image-format limits for identity and signature uploads.
- Confirmed rejection reason catalogue for admin review.
- Verification of production `user_documents` schema before applying the migration.
- Coordinated API, admin, and mobile release order.

## Out of Scope

- Asset-specific participation agreements.
- Requiring users to open every document before checking acceptance.
- Migrating the admin from browser-held API keys to a complete login/BFF architecture.
- A new external KYC provider, liveness, selfie, proof of address, or OCR.
- Changing the recharge minimum.
- Treating campaign `MinCapital` as the per-investment minimum; these are distinct concepts.
