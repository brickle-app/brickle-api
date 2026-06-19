using BricklePlatform.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BricklePlatform.Api.Controllers;

/// <summary>
/// Endpoint público que expone la configuración de contratos blockchain para el cliente.
/// Permite actualizar direcciones de contratos sin redesplegar la app móvil.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigController : ControllerBase
{
    private readonly Web3Settings _web3Settings;

    public ConfigController(IOptions<InfrastructureSettings> settings)
    {
        _web3Settings = settings.Value.Web3Settings;
    }

    /// <summary>
    /// Obtiene las direcciones de contratos blockchain.
    /// Permite actualizar contratos sin redesplegar la app móvil.
    /// </summary>
    [HttpGet("blockchain")]
    [ProducesResponseType(typeof(BlockchainConfigResponse), StatusCodes.Status200OK)]
    public ActionResult<BlockchainConfigResponse> GetBlockchainConfig()
    {
        var isTestnet = string.Equals(_web3Settings.Network, "testnet", StringComparison.OrdinalIgnoreCase);
        return Ok(new BlockchainConfigResponse
        {
            BaseToken = _web3Settings.BASE_TOKEN ?? string.Empty,
            PaymasterAddress = _web3Settings.PAYMASTER ?? string.Empty,
            ThresholdFactory = _web3Settings.THRESHOLD_FACTORY ?? string.Empty,
            BrickleNft = _web3Settings.BRICKLE_NFT ?? string.Empty,
            ChainId = isTestnet ? 80002 : 137
        });
    }
}

public record BlockchainConfigResponse
{
    public string BaseToken { get; init; } = string.Empty;
    public string PaymasterAddress { get; init; } = string.Empty;
    public string ThresholdFactory { get; init; } = string.Empty;
    public string BrickleNft { get; init; } = string.Empty;
    public int ChainId { get; init; }
}
