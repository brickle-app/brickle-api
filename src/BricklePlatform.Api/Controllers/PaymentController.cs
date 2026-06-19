using BricklePlatform.Api.Application.Commands.Payment;
using BricklePlatform.Api.Attributes;
using BricklePlatform.Api.Models;
using BricklePlatform.Domain.DTOs;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Controlador responsable de la gestión de pagos de leasing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserLeasingAgreementRepository _agreementRepository;
    private readonly ILeasingCoreService _leasingCoreService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IMediator mediator,
        IUserLeasingAgreementRepository agreementRepository,
        ILeasingCoreService leasingCoreService,
        ILogger<PaymentController> logger)
    {
        _mediator = mediator;
        _agreementRepository = agreementRepository;
        _leasingCoreService = leasingCoreService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el estado del contrato LeasingCore por dirección (para verificación).
    /// Útil cuando se tiene la dirección del contrato directamente.
    /// </summary>
    [HttpGet("leasing-state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeasingStateByAddress([FromQuery] string address)
    {
        if (string.IsNullOrWhiteSpace(address) || !address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Se requiere una dirección de contrato válida (0x...)" });

        var state = await _leasingCoreService.GetLeasingContractStateAsync(address);
        if (state == null)
            return NotFound(new { error = "No se pudo leer el estado del contrato" });

        return Ok(new
        {
            expectedAmount = state.ExpectedAmount.ToString(),
            isResidualPayment = state.IsResidualPayment,
            currentMonth = state.CurrentMonth,
            termMonths = state.TermMonths,
            lastPaymentMade = state.LastPaymentMade,
            residualValue = state.ResidualValue?.ToString(),
            finalPaymentAmount = state.FinalPaymentAmount?.ToString(),
            leasingTokenAddress = state.LeasingTokenAddress,
            leasingTokenTotalSupply = state.LeasingTokenTotalSupply?.ToString()
        });
    }

    /// <summary>
    /// Obtiene el monto esperado y estado del contrato LeasingCore.
    /// - Cuotas mensuales: totalMonthlyPayment.
    /// - Última cuota: residualValue (cuando currentMonth == termMonths y !lastPaymentMade).
    /// </summary>
    [HttpGet("expected-amount/{agreementId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExpectedAmount(Guid agreementId)
    {
        var agreement = await _agreementRepository.GetByIdAsync(agreementId);
        if (agreement == null)
            return NotFound(new { error = "Acuerdo no encontrado" });

        if (string.IsNullOrWhiteSpace(agreement.LeasingCoreAddress))
            return NotFound(new { error = "El acuerdo no tiene LeasingCore configurado (campaña no finalizada)" });

        var state = await _leasingCoreService.GetLeasingContractStateAsync(agreement.LeasingCoreAddress);
        if (state == null)
        {
            var fallback = await _leasingCoreService.GetExpectedMonthlyPaymentAsync(agreement.LeasingCoreAddress);
            if (!fallback.HasValue || fallback.Value == 0)
                return NotFound(new { error = "No se pudo leer monto esperado del contrato" });
            return Ok(new
            {
                expectedAmount = fallback.Value.ToString(),
                isResidualPayment = false,
                currentMonth = 0,
                termMonths = 0,
                lastPaymentMade = false
            });
        }

        return Ok(new
        {
            expectedAmount = state.ExpectedAmount.ToString(),
            isResidualPayment = state.IsResidualPayment,
            currentMonth = state.CurrentMonth,
            termMonths = state.TermMonths,
            lastPaymentMade = state.LastPaymentMade,
            residualValue = state.ResidualValue?.ToString(),
            finalPaymentAmount = state.FinalPaymentAmount?.ToString(),
            leasingTokenAddress = state.LeasingTokenAddress,
            leasingTokenTotalSupply = state.LeasingTokenTotalSupply?.ToString()
        });
    }

    /// <summary>
    /// Crea un nuevo pago para un leasing específico.
    /// </summary>
    /// <param name="header">Información de cabecera que incluye el CorrelationId para seguimiento de la solicitud.</param>
    /// <param name="paymentDto">Datos del pago a crear.</param>
    /// <returns>
    /// 201 Created: Retorna el pago creado exitosamente.
    /// 400 Bad Request: Si los datos del pago son inválidos.
    /// 500 Internal Server Error: En caso de error interno del servidor.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreatePaymentResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePayment(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] PaymentDto paymentDto)
    {
        try
        {
            _logger.LogInformation("Creando nuevo pago para para el contrato de arrendamiento de usuario: {UserLeasingAgreementId}",
            paymentDto.UserLeasingAgreementId);

            CreatePaymentCommand command = new CreatePaymentCommand(header, paymentDto);
            var response = await _mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Error de aplicación al crear pago - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InfrastructureException ex)
        {
            _logger.LogError(ex, "Error de infraestructura (webhook/blockchain) al crear pago - CorrelationId: {CorrelationId}", header.CorrelationId);
            var amount = 0m;
            try { amount = decimal.Parse(paymentDto.PaymentAmount) / (decimal)Math.Pow(10, 6); } catch { /* ignorar */ }
            return StatusCode(StatusCodes.Status201Created, new CreatePaymentResponse(false, string.Empty, amount, 0m, ex.Message));
        }
    }

    /// <summary>
    /// Ejecuta el pago final (valor residual): firma en servidor con <c>WalletPrivateKey</c> (gas y ejecución de la tx) y llama
    /// <c>makeLastLeasingPayment</c> en LeasingCore. Acumula residual + incentivo final en <c>totalClaimableByUser</c> (inversores reclaman con <c>claimEarnings</c>).
    /// <c>ClientAddress</c> es referencia en la API; requiere LeasingCore con esa semántica.
    /// </summary>
    [HttpPost("finalize-residual")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreatePaymentResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinalizeResidualPayment(
        [FromHeaderModel] HeaderRequestModel header,
        [FromBody] FinalizeResidualPaymentDto body)
    {
        try
        {
            var command = new FinalizeResidualPaymentCommand(header, body);
            var response = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Error al finalizar pago residual - CorrelationId: {CorrelationId}", header.CorrelationId);
            return BadRequest(new { error = ex.Message });
        }
    }
}