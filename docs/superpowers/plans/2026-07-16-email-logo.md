# Brickle Email Logo Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate and validate the official Brickle email logo locally, obtain explicit visual approval, then publish it to Azure Blob Storage and use it in every email template.

**Architecture:** Keep the approved SVG and its deterministic 2x PNG in `assets/branding/email`. Validate the PNG independently before any cloud operation and preview it in the shared email header. After the user approves the preview, upload a versioned immutable blob and configure `EmailService` to render that PNG through its existing shared layout.

**Tech Stack:** .NET 8, xUnit 2.8.1, macOS `sips`, Azure.Storage.Blobs 12.24.0, Resend HTML email templates, Superpowers Visual Companion.

## Global Constraints

- The SVG source is the exact `259 x 82` artwork supplied by the user: navy/orange Brickle wordmark plus "Donde crecer es más fácil".
- Generate a real RGBA PNG at exactly `518 x 164` pixels with a transparent background.
- Do not connect to Azure or upload any file before explicit user approval of the local preview.
- Upload to `branding/email/brickle-logo-2026-07.png`; never overwrite or delete an existing blob.
- Publish the blob as `image/png` and use an HTTPS read URL.
- Do not store Azure connection strings or SAS tokens in the repository.
- Do not restore the old green/purple HTML wordmark or the old Brickle WebP as a fallback.
- Preserve unrelated worktree changes.
- Do not create a git commit unless the user explicitly requests one.

---

### Task 1: Reproducible Local Logo Asset

**Files:**
- Create: `assets/branding/email/brickle-email-logo.svg`
- Create: `assets/branding/email/brickle-email-logo.png`
- Create: `test/BricklePlatform.Test/Services/EmailLogoAssetTests.cs`

**Interfaces:**
- Consumes: The exact SVG payload supplied in the feature request.
- Produces: `assets/branding/email/brickle-email-logo.png`, a `518 x 164` RGBA PNG used by the preview and Azure upload tasks.

- [ ] **Step 1: Write the failing asset test**

Create `EmailLogoAssetTests.cs` with a repository-root lookup and direct PNG header validation so no image library is required:

```csharp
using System.Buffers.Binary;
using Xunit;

namespace BricklePlatform.Test.Services;

public class EmailLogoAssetTests
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    [Fact]
    public void OfficialEmailLogoHasExpectedSvgSourceAndPngOutput()
    {
        var root = FindRepositoryRoot();
        var svgPath = Path.Combine(root, "assets", "branding", "email", "brickle-email-logo.svg");
        var pngPath = Path.Combine(root, "assets", "branding", "email", "brickle-email-logo.png");

        Assert.True(File.Exists(svgPath), $"Missing SVG source: {svgPath}");
        var svg = File.ReadAllText(svgPath);
        Assert.Contains("viewBox=\"0 0 259 82\"", svg);
        Assert.Contains("#1C3647", svg);
        Assert.Contains("#EB7F58", svg);

        Assert.True(File.Exists(pngPath), $"Missing PNG output: {pngPath}");
        var png = File.ReadAllBytes(pngPath);
        Assert.True(png.Length > 33, "PNG is too short to contain IHDR data.");
        Assert.Equal(PngSignature, png[..8]);
        Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(png, 12, 4));
        Assert.Equal(518, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
        Assert.Equal(164, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        Assert.Contains(png[25], new byte[] { 4, 6 }); // Grayscale+alpha or RGBA.
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BricklePlatform.Api.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate BricklePlatform.Api.sln.");
    }
}
```

- [ ] **Step 2: Run the test and verify the missing-asset failure**

Run:

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~EmailLogoAssetTests
```

Expected: FAIL with `Missing SVG source`.

- [ ] **Step 3: Save the approved SVG source exactly**

Create `assets/branding/email/brickle-email-logo.svg` by saving the complete `<svg width="259" height="82" viewBox="0 0 259 82" ...>...</svg>` block supplied in the request verbatim. Do not simplify paths, replace colors, add a background rectangle, or remove the paths that form the tagline.

Verify the immutable source characteristics before rasterization:

```bash
file assets/branding/email/brickle-email-logo.svg
```

Expected: output identifies an SVG/XML text document.

- [ ] **Step 4: Generate the 2x PNG locally**

Run:

```bash
sips -s format png -z 164 518 assets/branding/email/brickle-email-logo.svg --out assets/branding/email/brickle-email-logo.png
```

Expected: `assets/branding/email/brickle-email-logo.png` is created without an error.

- [ ] **Step 5: Run automatic image metadata checks**

Run:

```bash
sips -g format -g pixelWidth -g pixelHeight -g hasAlpha assets/branding/email/brickle-email-logo.png
```

Expected: `format: png`, `pixelWidth: 518`, `pixelHeight: 164`, and `hasAlpha: yes`.

- [ ] **Step 6: Run the asset test**

Run:

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~EmailLogoAssetTests
```

Expected: PASS.

---

### Task 2: Local Email Preview and Approval Gate

**Files:**
- Create temporarily through Visual Companion: `.superpowers/brainstorm/<session>/content/brickle-email-logo.png`
- Create temporarily through Visual Companion: `.superpowers/brainstorm/<session>/content/email-logo-preview.html`
- Modify: `.gitignore`

**Interfaces:**
- Consumes: `assets/branding/email/brickle-email-logo.png` from Task 1.
- Produces: Explicit user decision `approved` or `changes requested`. Task 3 is forbidden unless the result is `approved`.

- [ ] **Step 1: Keep preview session files out of git**

Add this block to `.gitignore` if it is not already present:

```gitignore
# Superpowers local visual previews
.superpowers/
```

- [ ] **Step 2: Start the Visual Companion**

Run:

```bash
/Users/pivelcode/.cache/opencode/packages/superpowers@git+https:/github.com/obra/superpowers.git/node_modules/superpowers/skills/brainstorming/scripts/start-server.sh --project-dir /Volumes/B/Projects/Brickle/BricklePlatform-api --open
```

Expected: JSON containing `url`, `screen_dir`, and `state_dir`. Preserve those exact values for the remaining preview steps.

- [ ] **Step 3: Check the server before publishing the preview**

Read `<state_dir>/server-info` and confirm `<state_dir>/server-stopped` does not exist.

Expected: the server information exists and the stopped marker is absent.

- [ ] **Step 4: Put the approved local PNG beside the preview HTML**

Copy `assets/branding/email/brickle-email-logo.png` to `<screen_dir>/brickle-email-logo.png`. This is a local copy for browser rendering only; do not upload it to any network service.

- [ ] **Step 5: Create the email-header preview**

Create `<screen_dir>/email-logo-preview.html` with this fragment:

```html
<h2>Revisión final del logo para emails</h2>
<p class="subtitle">Esta es la imagen PNG local. Azure todavía no se ha utilizado.</p>

<div class="split">
  <div class="mockup">
    <div class="mockup-header">Tamaño real del recurso: 518 x 164</div>
    <div class="mockup-body" style="background:#fff;padding:32px;text-align:center">
      <img src="brickle-email-logo.png" alt="Brickle - Donde crecer es más fácil"
           width="518" height="164" style="display:block;max-width:100%;height:auto;margin:0 auto">
    </div>
  </div>
  <div class="mockup">
    <div class="mockup-header">Encabezado del email: 259 x 82</div>
    <div class="mockup-body" style="background:#E8F5E9;padding:24px 16px 32px">
      <div style="max-width:560px;margin:0 auto;text-align:center">
        <img src="brickle-email-logo.png" alt="Brickle - Donde crecer es más fácil"
             width="259" height="82" style="display:block;width:259px;max-width:100%;height:auto;margin:0 auto 20px;border:0">
        <div style="height:4px;background:#85FA8F;border-radius:16px 16px 0 0"></div>
        <div style="background:#fff;border:1px solid #E5E7EB;padding:28px 24px;text-align:left;font-family:Arial,sans-serif;color:#1C3647">
          <h3 style="margin:0 0 8px">Verifica tu correo electrónico</h3>
          <p style="margin:0;color:#6B7280">Ingresa el código en la aplicación para continuar.</p>
        </div>
      </div>
    </div>
  </div>
</div>

<div class="options">
  <div class="option" data-choice="approved" onclick="toggleSelect(this)">
    <div class="letter">A</div>
    <div class="content"><h3>Aprobar</h3><p>La calidad, colores, proporción y transparencia son correctos.</p></div>
  </div>
  <div class="option" data-choice="changes-requested" onclick="toggleSelect(this)">
    <div class="letter">B</div>
    <div class="content"><h3>Solicitar cambios</h3><p>No subir a Azure; regenerar el PNG según mis observaciones.</p></div>
  </div>
</div>
```

- [ ] **Step 6: Request final visual approval and stop**

Share the complete Visual Companion URL including its `?key=...` query string. Ask the user to inspect both render sizes and respond in the terminal.

Expected: STOP. Do not execute Task 3 or any Azure command until the user explicitly says the image is approved.

- [ ] **Step 7: Handle the decision**

If the user requests changes, update the SVG or rasterization settings, generate a new PNG, rerun every Task 1 check, and publish a new preview filename. If the user approves, record that approval in the working session and proceed to Task 3.

---

### Task 3: Versioned Azure Blob Publication

**Files:**
- Create: `tools/BricklePlatform.EmailAssets/BricklePlatform.EmailAssets.csproj`
- Create: `tools/BricklePlatform.EmailAssets/Program.cs`

**Interfaces:**
- Consumes: Explicit approval from Task 2, local PNG path, `InfrastructureSettings__AzureSettings__ConnectionString`, and `InfrastructureSettings__AzureSettings__BlobName` environment variables.
- Produces: A non-overwritten blob at `branding/email/brickle-logo-2026-07.png` and an HTTPS read URL for `EmailSettings.LogoImageUrl`.

- [ ] **Step 1: Confirm approval and credentials without printing secrets**

Do not run this step unless Task 2 ended in explicit approval. Then run:

```bash
test -n "$InfrastructureSettings__AzureSettings__ConnectionString" && test -n "$InfrastructureSettings__AzureSettings__BlobName"
```

Expected: exit code `0` and no output. If it fails, stop and request that the existing Azure settings be made available in the shell; never paste them into a tracked file.

- [ ] **Step 2: Create the uploader project**

Create `BricklePlatform.EmailAssets.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Storage.Blobs" Version="12.24.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Implement fail-closed versioned upload**

Create `Program.cs`:

```csharp
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

const string BlobPath = "branding/email/brickle-logo-2026-07.png";

if (args.Length != 1)
    throw new ArgumentException("Usage: BricklePlatform.EmailAssets <local-png-path>");

var localPath = Path.GetFullPath(args[0]);
if (!File.Exists(localPath))
    throw new FileNotFoundException("Logo PNG was not found.", localPath);

var connectionString = Environment.GetEnvironmentVariable(
    "InfrastructureSettings__AzureSettings__ConnectionString");
var containerName = Environment.GetEnvironmentVariable(
    "InfrastructureSettings__AzureSettings__BlobName");

if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
    throw new InvalidOperationException("Azure connection string and blob container must be supplied through environment variables.");

var container = new BlobContainerClient(connectionString, containerName);
var blob = container.GetBlobClient(BlobPath);

if (await blob.ExistsAsync())
    throw new InvalidOperationException($"Refusing to overwrite existing blob: {BlobPath}");

await using var stream = File.OpenRead(localPath);
await blob.UploadAsync(stream, new BlobUploadOptions
{
    HttpHeaders = new BlobHttpHeaders
    {
        ContentType = "image/png",
        CacheControl = "public, max-age=31536000, immutable"
    }
});

var sas = new BlobSasBuilder
{
    BlobContainerName = container.Name,
    BlobName = BlobPath,
    Resource = "b",
    ExpiresOn = DateTimeOffset.Parse("2038-01-01T00:00:00Z")
};
sas.SetPermissions(BlobSasPermissions.Read);

Console.WriteLine(blob.GenerateSasUri(sas));
```

- [ ] **Step 4: Build without contacting Azure**

Run:

```bash
dotnet build tools/BricklePlatform.EmailAssets/BricklePlatform.EmailAssets.csproj
```

Expected: build succeeds with zero errors.

- [ ] **Step 5: Upload once after the approval gate**

Run:

```bash
export BRICKLE_EMAIL_LOGO_URL="$(dotnet run --no-build --project tools/BricklePlatform.EmailAssets/BricklePlatform.EmailAssets.csproj -- assets/branding/email/brickle-email-logo.png)"
test -n "$BRICKLE_EMAIL_LOGO_URL"
```

Expected: the environment variable contains one read-only HTTPS SAS URI. A pre-existing path causes a hard failure and is not overwritten. Do not add the URI to any tracked file.

- [ ] **Step 6: Verify blob headers and access**

Use the URI retained in the current shell without writing it to disk:

```bash
curl --fail --silent --show-error --head "$BRICKLE_EMAIL_LOGO_URL"
```

Expected: HTTP success and `Content-Type: image/png`. Keep `BRICKLE_EMAIL_LOGO_URL` in the deployment secret/environment configuration only.

---

### Task 4: Shared Email Header Integration

**Files:**
- Modify: `test/BricklePlatform.Test/Services/EmailTemplateLogoTests.cs`
- Modify: `src/BricklePlatform.Infrastructure/Services/EmailService.cs:38-45,212-256,261-302`
- Modify: `src/BricklePlatform.Infrastructure/Settings/InfrastructureSettings.cs:76-86`
- Modify: `src/BricklePlatform.Api/appsettings.Development.json:52-57`
- Modify: `src/BricklePlatform.Api/appsettings.Production.json:52-57`

**Interfaces:**
- Consumes: Approved HTTPS PNG URL through `EmailSettings.LogoImageUrl` / `InfrastructureSettings__EmailSettings__LogoImageUrl`.
- Produces: `BuildBrandHeaderRow(string? logoImageUrl)` rendering the official `259 x 82` image in every `BrickleEmailDocument` email.

- [ ] **Step 1: Replace the old logo tests with failing official-logo tests**

Keep the existing reflection helper and replace its facts with:

```csharp
[Fact]
public void BrandHeaderRendersOfficialPngWithCorrectProportion()
{
    var html = BuildBrandHeaderRow(
        "https://account.blob.core.windows.net/container/branding/email/brickle-logo-2026-07.png?sig=test");

    Assert.Contains("<img", html, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("width=\"259\"", html);
    Assert.Contains("height=\"82\"", html);
    Assert.Contains("alt=\"Brickle - Donde crecer es más fácil\"", html);
    Assert.DoesNotContain("Inversión en activos reales", html);
    Assert.DoesNotContain("logo_green", html);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("https://brickle.app/old-logo.webp")]
[InlineData("javascript:alert(1)")]
public void BrandHeaderFallsBackToTextWithoutOldBranding(string? logoUrl)
{
    var html = BuildBrandHeaderRow(logoUrl);

    Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
    Assert.Contains(">Brickle<", html);
    Assert.DoesNotContain("#85FA8F", html, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("#9B6FEB", html, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("logo_green", html, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run the tests and verify they fail against the current header**

Run:

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter FullyQualifiedName~EmailTemplateLogoTests
```

Expected: FAIL because the current image is `200 x 38`, includes the old tagline, and uses the green/purple HTML fallback.

- [ ] **Step 3: Make logo configuration fail closed**

In `InfrastructureSettings.cs`, replace the old default URL with:

```csharp
public string LogoImageUrl { get; set; } = string.Empty;
```

In development and production appsettings, set:

```json
"LogoImageUrl": ""
```

Remove `DefaultBrickleLogoImageUrl` from `EmailService` and simplify the property:

```csharp
private string? EmailLogoImageUrl =>
    string.IsNullOrWhiteSpace(_settings.Value.EmailSettings.LogoImageUrl)
        ? null
        : _settings.Value.EmailSettings.LogoImageUrl.Trim();
```

- [ ] **Step 4: Render only the official image or neutral text fallback**

Delete `BuildHtmlWordmark`. Keep `TryGetTrustedLogoImageSrc` restricted to absolute HTTP(S) PNG/JPEG/GIF URLs. Replace `BuildBrandHeaderRow` with:

```csharp
private static string BuildBrandHeaderRow(string? logoImageUrl)
{
    if (TryGetTrustedLogoImageSrc(logoImageUrl, out var src))
    {
        return $@"
          <tr>
            <td style=""padding:0 0 20px 0;text-align:center;"">
              <img src=""{src}"" alt=""Brickle - Donde crecer es más fácil"" width=""259"" height=""82"" style=""display:block;width:259px;max-width:100%;height:auto;margin:0 auto;border:0;outline:none;text-decoration:none;"" />
            </td>
          </tr>";
    }

    return $@"
          <tr>
            <td style=""padding:0 0 20px 0;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:28px;line-height:1.2;font-weight:800;color:{BrandNavy};"">
              Brickle
            </td>
          </tr>";
}
```

- [ ] **Step 5: Run focused email and asset tests**

Run:

```bash
dotnet test test/BricklePlatform.Test/BricklePlatform.Test.csproj --filter "FullyQualifiedName~EmailTemplateLogoTests|FullyQualifiedName~EmailLogoAssetTests"
```

Expected: PASS.

- [ ] **Step 6: Configure the approved URL outside git**

Set `InfrastructureSettings__EmailSettings__LogoImageUrl` in the runtime secret/environment configuration to the verified `BRICKLE_EMAIL_LOGO_URL`. Do not place a SAS query string in either appsettings file.

- [ ] **Step 7: Run full verification**

Run:

```bash
dotnet test BricklePlatform.Api.sln
```

Expected: all tests pass with zero failures. If unrelated pre-existing failures occur, report their exact names and retain the focused email test evidence.

- [ ] **Step 8: Review the final diff without committing**

Run:

```bash
git status --short
```

Expected: only intended email-logo changes plus previously existing unrelated worktree changes; no whitespace errors and no secrets.
