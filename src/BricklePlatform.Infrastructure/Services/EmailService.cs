using System.Net;
using BricklePlatform.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace BricklePlatform.Infrastructure.Services;

public interface IEmailService
{
    Task SendRechargeNotificationAsync(string userEmail, string userName, decimal amount, string receipt, string walletAddress);
    Task SendWithdrawNotificationAsync(string userEmail, string userName, decimal amount, string bankAccountInfo, string tokenBurnLink);
    Task SendRechargeConfirmationAsync(string userEmail, string userName, decimal amount);
    Task SendWithdrawConfirmationAsync(string userEmail, string userName, decimal amount);
    Task SendLeasingActiveNotificationAsync(string userEmail, string userName, string campaignName);
    Task SendProfileUnderReviewAsync(string userEmail, string userName);
    Task SendProfileApprovedAsync(string userEmail, string userName);
    Task SendOtpEmailAsync(string userEmail, string userName, string otpCode);
}

public class EmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IOptions<InfrastructureSettings> _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IResend resend,
        IOptions<InfrastructureSettings> settings,
        ILogger<EmailService> logger)
    {
        _resend = resend;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>Logo horizontal público. En email, WebP se rechaza y cae a wordmark HTML por compatibilidad.</summary>
    private const string DefaultBrickleLogoImageUrl = "https://brickle.app/assets/logo_green-B0JL5kO0.webp";

    /// <summary>URL pública del logo para &lt;img&gt;; si la config está vacía se usa el logo por defecto. Si la URL no es http(s) o no es formato email-safe, usa wordmark HTML.</summary>
    private string? EmailLogoImageUrl =>
        string.IsNullOrWhiteSpace(_settings.Value.EmailSettings.LogoImageUrl)
            ? DefaultBrickleLogoImageUrl
            : _settings.Value.EmailSettings.LogoImageUrl.Trim();

    public async Task SendRechargeNotificationAsync(string userEmail, string userName, decimal amount, string receipt, string walletAddress)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = _settings.Value.EmailSettings.AdminEmail,
                Subject = "Brickle · Nueva solicitud de recarga",
                HtmlBody = GenerateRechargeNotificationTemplate(userEmail, userName, amount, receipt, walletAddress)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Recharge notification email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recharge notification email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendWithdrawNotificationAsync(string userEmail, string userName, decimal amount, string bankAccountInfo, string tokenBurnLink)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = _settings.Value.EmailSettings.AdminEmail,
                Subject = "Brickle · Nueva solicitud de retiro",
                HtmlBody = GenerateWithdrawNotificationTemplate(userEmail, userName, amount, bankAccountInfo, tokenBurnLink)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Withdraw notification email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending withdraw notification email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendRechargeConfirmationAsync(string userEmail, string userName, decimal amount)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · Tu recarga fue confirmada",
                HtmlBody = GenerateRechargeConfirmationTemplate(userName, amount)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Recharge confirmation email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recharge confirmation email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendWithdrawConfirmationAsync(string userEmail, string userName, decimal amount)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · Tu retiro fue procesado",
                HtmlBody = GenerateWithdrawConfirmationTemplate(userName, amount)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Withdraw confirmation email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending withdraw confirmation email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendLeasingActiveNotificationAsync(string userEmail, string userName, string campaignName)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · Tu inversión ya está activa",
                HtmlBody = GenerateLeasingActiveNotificationTemplate(userName, campaignName)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Leasing active notification email sent successfully. User: {UserEmail}, Campaign: {CampaignName}", userEmail, campaignName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending leasing active notification email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendOtpEmailAsync(string userEmail, string userName, string otpCode)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · Tu código de verificación",
                HtmlBody = GenerateOtpTemplate(userName, otpCode)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("OTP email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    // —— Brickle brand (alineado con app móvil) ——
    private const string BrandNavy = "#1C3647";
    private const string BrandMint = "#85FA8F";
    private const string BrandMintSoft = "#D1F5BA";
    private const string BrandOrange = "#EB7F58";
    private const string BrandPurple = "#44235C";
    private const string BrandBurgundy = "#510032";
    private const string BrandLilac = "#9B6FEB";
    private const string BrandBg = "#E8F5E9";
    private const string BrandCard = "#FFFFFF";
    private const string BrandLine = "#E5E7EB";
    private const string BrandText = "#1C3647";
    private const string BrandMuted = "#6B7280";
    private const int EmailMaxWidthPx = 560;

    private static string H(string? s) => string.IsNullOrEmpty(s) ? "" : WebUtility.HtmlEncode(s);

    private static string SafeHref(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "#";
        var t = url.Trim();
        if (!t.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !t.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return "#";
        return WebUtility.HtmlEncode(t);
    }

    private static bool TryGetTrustedLogoImageSrc(string? logoImageUrl, out string htmlEncodedAbsoluteUri)
    {
        htmlEncodedAbsoluteUri = "";
        if (string.IsNullOrWhiteSpace(logoImageUrl)) return false;
        if (!Uri.TryCreate(logoImageUrl.Trim(), UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
        var path = uri.AbsolutePath;
        if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
            !path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            return false;
        htmlEncodedAbsoluteUri = WebUtility.HtmlEncode(uri.ToString());
        return true;
    }

    private static string BuildHtmlWordmark()
    {
        return $@"
              <div aria-label=""Brickle"" style=""font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:42px;line-height:1;font-weight:900;letter-spacing:-0.06em;color:{BrandMint};text-align:center;white-space:nowrap;"">
                <span style=""display:inline-block;color:{BrandMint};"">Br</span><span style=""display:inline-block;width:17px;height:48px;margin:0 1px -10px 1px;background:repeating-linear-gradient(135deg,{BrandLilac} 0,{BrandLilac} 5px,#7D52D9 5px,#7D52D9 10px);vertical-align:baseline;border-radius:2px;"">&nbsp;</span><span style=""display:inline-block;color:{BrandMint};"">ckle</span>
              </div>";
    }

    private static string BuildBrandHeaderRow(string? logoImageUrl)
    {
        if (TryGetTrustedLogoImageSrc(logoImageUrl, out var src))
        {
            return $@"
          <tr>
            <td style=""padding:0 0 20px 0;text-align:center;"">
              <img src=""{src}"" alt=""Brickle"" width=""200"" height=""38"" style=""width:200px;height:38px;display:block;margin:0 auto;border:0;outline:none;text-decoration:none;"" />
              <div style=""font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:11px;color:{BrandMuted};margin-top:10px;letter-spacing:0.04em;"">Inversión en activos reales</div>
            </td>
          </tr>";
        }

        return $@"
          <tr>
            <td style=""padding:0 0 20px 0;text-align:center;"">
              {BuildHtmlWordmark()}
              <div style=""font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:11px;color:{BrandMuted};margin-top:10px;letter-spacing:0.04em;"">Inversión en activos reales</div>
            </td>
          </tr>";
    }

    /// <summary>
    /// Layout único: fondo Brickle, tarjeta centrada con ancho máximo (desktop y móvil).
    /// </summary>
    private static string BrickleEmailDocument(string pageTitle, string accentColor, string headline, string subline, string innerCardHtml, string? logoImageUrl)
    {
        var w = EmailMaxWidthPx;
        return $@"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
  <title>{H(pageTitle)}</title>
  <!--[if mso]><style type=""text/css"">table {{ border-collapse: collapse; }}</style><![endif]-->
</head>
<body style=""margin:0;padding:0;background-color:{BrandBg};"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:{BrandBg};"">
    <tr>
      <td align=""center"" style=""padding:24px 16px 32px 16px;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:{w}px;width:100%;"">
          {BuildBrandHeaderRow(logoImageUrl)}
          <tr>
            <td style=""background-color:{BrandCard};border-radius:16px;overflow:hidden;border:1px solid {BrandLine};box-shadow:0 4px 24px rgba(28,54,71,0.08);"">
              <div style=""height:4px;background:{accentColor};""></div>
              <div style=""padding:28px 24px 8px 24px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
                <h1 style=""margin:0 0 8px 0;font-size:20px;line-height:1.3;color:{BrandText};font-weight:700;"">{headline}</h1>
                <p style=""margin:0 0 20px 0;font-size:14px;line-height:1.5;color:{BrandMuted};"">{subline}</p>
              </div>
              <div style=""padding:0 24px 28px 24px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:15px;line-height:1.6;color:{BrandText};"">
                {innerCardHtml}
              </div>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 8px 0 8px;text-align:center;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:12px;color:{BrandMuted};line-height:1.5;"">
              Este mensaje fue enviado por <strong style=""color:{BrandNavy};"">Brickle</strong>.<br/>
              Si no esperabas este correo, puedes ignorarlo o contactar a soporte.
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    private static string CardSection(string titleHtml, string bodyHtml, string leftBorderColor, string? backgroundColor = null)
    {
        var bg = backgroundColor ?? "#F9FAFB";
        return $@"
<div style=""background:{bg};border-radius:12px;border-left:4px solid {leftBorderColor};padding:16px 18px;margin:0 0 18px 0;"">
  <p style=""margin:0 0 10px 0;font-size:13px;font-weight:700;color:{BrandText};text-transform:uppercase;letter-spacing:0.04em;"">{titleHtml}</p>
  <div style=""font-size:14px;color:{BrandText};line-height:1.55;"">{bodyHtml}</div>
</div>";
    }

    private static string LinkPrimary(string href, string label) =>
        $@"<a href=""{href}"" target=""_blank"" rel=""noopener noreferrer"" style=""display:inline-block;margin-top:4px;color:{BrandNavy};font-weight:600;text-decoration:underline;"">{H(label)}</a>";

    private string GenerateRechargeNotificationTemplate(string userEmail, string userName, decimal amount, string receipt, string walletAddress)
    {
        var receiptHref = SafeHref(receipt);
        var details = $@"
<p style=""margin:0 0 6px 0;""><strong>Usuario</strong><br/><span style=""color:{BrandMuted};"">{H(userName)}</span></p>
<p style=""margin:0 0 6px 0;""><strong>Correo</strong><br/><a href=""mailto:{H(userEmail)}"" style=""color:{BrandNavy};font-weight:600;"">{H(userEmail)}</a></p>
<p style=""margin:0 0 6px 0;""><strong>Wallet</strong><br/><code style=""display:inline-block;margin-top:4px;padding:6px 10px;background:#F3F4F6;border-radius:8px;font-size:12px;word-break:break-all;color:{BrandText};"">{H(walletAddress)}</code></p>
<p style=""margin:0 0 6px 0;""><strong>Monto</strong><br/><span style=""font-size:22px;font-weight:800;color:{BrandNavy};"">${amount:N2} COP</span></p>
<p style=""margin:0 0 6px 0;""><strong>Comprobante</strong><br/>{LinkPrimary(receiptHref, "Ver comprobante")}</p>
<p style=""margin:0;""><strong>Fecha (UTC)</strong><br/><span style=""color:{BrandMuted};"">{H(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))}</span></p>";

        var inner = CardSection("Detalles de la solicitud", details, BrandNavy) + $@"
<p style=""margin:0 0 8px 0;font-weight:700;color:{BrandText};"">Acciones sugeridas</p>
<ul style=""margin:0;padding-left:20px;color:{BrandText};"">
  <li style=""margin-bottom:6px;"">Verificar el comprobante de pago.</li>
  <li style=""margin-bottom:6px;"">Confirmar la recarga en el panel de administración.</li>
  <li>Notificar al usuario el resultado.</li>
</ul>";

        return BrickleEmailDocument(
            "Nueva solicitud de recarga",
            BrandNavy,
            "Nueva solicitud de recarga",
            "Se registró una solicitud de recarga en la plataforma Brickle.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateWithdrawNotificationTemplate(string userEmail, string userName, decimal amount, string bankAccountInfo, string tokenBurnLink)
    {
        var burnHref = SafeHref(tokenBurnLink);
        var details = $@"
<p style=""margin:0 0 6px 0;""><strong>Usuario</strong><br/><span style=""color:{BrandMuted};"">{H(userName)}</span></p>
<p style=""margin:0 0 6px 0;""><strong>Correo</strong><br/><a href=""mailto:{H(userEmail)}"" style=""color:{BrandNavy};font-weight:600;"">{H(userEmail)}</a></p>
<p style=""margin:0 0 6px 0;""><strong>Monto</strong><br/><span style=""font-size:22px;font-weight:800;color:{BrandOrange};"">${amount:N2} COP</span></p>
<p style=""margin:0;""><strong>Fecha (UTC)</strong><br/><span style=""color:{BrandMuted};"">{H(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))}</span></p>";

        var bank = $@"
<p style=""margin:0;white-space:pre-wrap;word-break:break-word;font-family:ui-monospace,monospace;font-size:13px;color:{BrandText};"">{H(bankAccountInfo)}</p>";

        var burn = $@"
<p style=""margin:0 0 8px 0;color:#F3F4F6;font-size:14px;line-height:1.5;"">Revisa la transacción de quema asociada (si aplica):</p>
<p style=""margin:0;""><a href=""{burnHref}"" target=""_blank"" rel=""noopener noreferrer"" style=""color:{BrandMint};font-weight:700;text-decoration:underline;"">Ver en el explorador →</a></p>";

        var inner = CardSection("Detalles de la solicitud", details, BrandOrange)
                    + CardSection("Cuenta bancaria", bank, BrandMint, BrandMintSoft)
                    + CardSection("Quema de tokens", burn, BrandLilac, BrandBurgundy)
                    + $@"
<p style=""margin:0 0 8px 0;font-weight:700;color:{BrandText};"">Acciones sugeridas</p>
<ul style=""margin:0;padding-left:20px;color:{BrandText};"">
  <li style=""margin-bottom:6px;"">Verificar saldo y límites del usuario.</li>
  <li style=""margin-bottom:6px;"">Validar datos bancarios y comprobante de quema si corresponde.</li>
  <li>Procesar el retiro y notificar al usuario.</li>
</ul>";

        return BrickleEmailDocument(
            "Nueva solicitud de retiro",
            BrandOrange,
            "Nueva solicitud de retiro",
            "Se registró una solicitud de retiro en la plataforma Brickle.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateRechargeConfirmationTemplate(string userName, decimal amount)
    {
        var box = $@"
<p style=""margin:0 0 8px 0;""><strong>Monto acreditado</strong><br/><span style=""font-size:22px;font-weight:800;color:{BrandNavy};"">${amount:N2} COP</span></p>
<p style=""margin:0 0 8px 0;""><strong>Fecha de confirmación (UTC)</strong><br/><span style=""color:{BrandMuted};"">{H(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))}</span></p>
<p style=""margin:0;""><strong>Estado</strong><br/><span style=""color:{BrandNavy};font-weight:800;"">CONFIRMADA</span></p>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Tu recarga fue confirmada. El saldo ya está disponible en tu cuenta Brickle para invertir.</p>
" + CardSection("Resumen", box, BrandMint, BrandMintSoft) + $@"
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Gracias por confiar en Brickle.</p>";

        return BrickleEmailDocument(
            "Recarga confirmada",
            BrandMint,
            "Recarga confirmada",
            "Tu saldo ya está listo para usar.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateWithdrawConfirmationTemplate(string userName, decimal amount)
    {
        var box = $@"
<p style=""margin:0 0 8px 0;""><strong>Monto</strong><br/><span style=""font-size:22px;font-weight:800;color:{BrandNavy};"">${amount:N2} COP</span></p>
<p style=""margin:0 0 8px 0;""><strong>Fecha de procesamiento (UTC)</strong><br/><span style=""color:{BrandMuted};"">{H(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))}</span></p>
<p style=""margin:0;""><strong>Estado</strong><br/><span style=""color:{BrandNavy};font-weight:800;"">PROCESADO</span></p>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Tu solicitud de retiro fue procesada. El valor será abonado a tu cuenta bancaria registrada en los plazos habituales de la red bancaria.</p>
" + CardSection("Resumen", box, BrandNavy, "#F3F4F6") + $@"
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Si tienes dudas, responde a este correo o escribe a soporte desde la app Brickle.</p>";

        return BrickleEmailDocument(
            "Retiro procesado",
            BrandNavy,
            "Retiro procesado",
            "Hemos registrado tu retiro correctamente.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateLeasingActiveNotificationTemplate(string userName, string campaignName)
    {
        var highlight = $@"
<ul style=""margin:0;padding-left:20px;color:{BrandText};"">
  <li style=""margin-bottom:8px;"">Los fondos quedaron vinculados a tu participación.</li>
  <li style=""margin-bottom:8px;"">El contrato de leasing está activo.</li>
  <li>Puedes revisar rentas desde <strong>Mis inversiones</strong> en la app.</li>
</ul>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">El activo <strong style=""color:{BrandPurple};"">{H(campaignName)}</strong> ya está activo en Brickle.</p>
" + CardSection("¿Qué sigue?", highlight, BrandPurple, BrandMintSoft) + $@"
<p style=""margin:0 0 12px 0;font-weight:700;color:{BrandText};"">Renta</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Según el calendario del activo, podrás ver y reclamar rentas desde la app cuando estén disponibles.</p>
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Gracias por invertir con Brickle.</p>";

        return BrickleEmailDocument(
            "Inversión activa",
            BrandPurple,
            "Tu inversión está activa",
            "Buenas noticias: tu participación ya está en marcha.",
            inner,
            EmailLogoImageUrl);
    }

    public async Task SendProfileUnderReviewAsync(string userEmail, string userName)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · Tu perfil está en revisión",
                HtmlBody = GenerateProfileUnderReviewTemplate(userName)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Profile under review email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending profile under review email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task SendProfileApprovedAsync(string userEmail, string userName)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                From = _settings.Value.EmailSettings.FromEmail,
                To = userEmail,
                Subject = "Brickle · ¡Tu perfil fue aprobado!",
                HtmlBody = GenerateProfileApprovedTemplate(userName)
            };

            await _resend.EmailSendAsync(emailMessage);

            _logger.LogInformation("Profile approved email sent successfully. User: {UserEmail}", userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending profile approved email for user: {UserEmail}", userEmail);
            throw;
        }
    }

    private string GenerateProfileUnderReviewTemplate(string userName)
    {
        var steps = $@"
<ul style=""margin:0;padding-left:20px;color:{BrandText};"">
  <li style=""margin-bottom:8px;"">Nuestro equipo validará tu identidad y documentos.</li>
  <li style=""margin-bottom:8px;"">El proceso generalmente toma <strong>1 a 3 días hábiles</strong>.</li>
  <li>Te notificaremos por correo y en la app cuando tengamos novedades.</li>
</ul>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Recibimos tu información correctamente. Tu perfil está siendo revisado por nuestro equipo de cumplimiento.</p>
" + CardSection("¿Qué pasa ahora?", steps, BrandNavy, "#F0F9FF") + $@"
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Gracias por tu paciencia. Si tienes alguna duda, escríbenos desde la app Brickle.</p>";

        return BrickleEmailDocument(
            "Perfil en revisión",
            BrandNavy,
            "Tu perfil está en revisión",
            "Hemos recibido tu solicitud y la estamos procesando.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateProfileApprovedTemplate(string userName)
    {
        var highlight = $@"
<ul style=""margin:0;padding-left:20px;color:{BrandText};"">
  <li style=""margin-bottom:8px;"">Ya puedes explorar y comprar activos en <strong>Descubrir</strong>.</li>
  <li style=""margin-bottom:8px;"">Recarga tu billetera para empezar a invertir.</li>
  <li>Revisa tu portafolio en la sección <strong>Portafolio</strong> de la app.</li>
</ul>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Excelentes noticias: <strong style=""color:{BrandNavy};"">tu perfil fue verificado y aprobado</strong>. Ya tienes acceso completo a la plataforma Brickle.</p>
" + CardSection("¡Empieza a invertir!", highlight, BrandMint, BrandMintSoft) + $@"
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Bienvenido a Brickle. Gracias por confiar en nosotros.</p>";

        return BrickleEmailDocument(
            "Perfil aprobado",
            BrandMint,
            "¡Tu perfil fue aprobado!",
            "Ya puedes invertir en activos reales con Brickle.",
            inner,
            EmailLogoImageUrl);
    }

    private string GenerateOtpTemplate(string userName, string otpCode)
    {
        var otpBox = $@"
<div style=""text-align:center;padding:16px 0;"">
  <div style=""display:inline-block;background:{BrandNavy};color:#ffffff;font-size:32px;font-weight:800;letter-spacing:8px;padding:16px 32px;border-radius:12px;font-family:ui-monospace,monospace;"">{H(otpCode)}</div>
</div>
<p style=""margin:0;color:{BrandMuted};font-size:13px;text-align:center;"">Este código expira en 5 minutos. No lo compartas con nadie.</p>";

        var inner = $@"
<p style=""margin:0 0 16px 0;color:{BrandText};"">Hola <strong>{H(userName)}</strong>,</p>
<p style=""margin:0 0 18px 0;color:{BrandText};"">Usa el siguiente código para verificar tu correo electrónico en Brickle:</p>
" + CardSection("Código de verificación", otpBox, BrandMint, BrandMintSoft) + $@"
<p style=""margin:0;color:{BrandMuted};font-size:14px;"">Si no solicitaste este código, puedes ignorar este mensaje.</p>";

        return BrickleEmailDocument(
            "Código de verificación",
            BrandMint,
            "Verifica tu correo electrónico",
            "Ingresa el código en la aplicación para continuar.",
            inner,
            EmailLogoImageUrl);
    }
}
