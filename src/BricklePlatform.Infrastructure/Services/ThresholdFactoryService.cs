using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Domain.DTOs;
using Microsoft.Extensions.Options;
using BricklePlatform.Infrastructure.Settings;
using System.Reflection;
using Newtonsoft.Json;
using BricklePlatform.Infrastructure.Models;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.RPC.Eth.DTOs;

namespace BricklePlatform.Infrastructure.Services;

public class ThresholdFactoryService : IThresholdFactoryService
{

  private readonly IOptions<InfrastructureSettings> _settings;
  private readonly IWeb3Service _web3Service;

  public ThresholdFactoryService(IWeb3Service web3Service, IOptions<InfrastructureSettings> settings)
  {
    _web3Service = web3Service;
    _settings = settings;
  }

  public async Task<(string ContractAddress, string TransactionHash)> CreateCampaign(CampaignInfoDto campaignInfo, LeasingInfoDto leasingInfo)
  {
    try
    {
      if (campaignInfo == null && leasingInfo == null)
      {
        throw new ArgumentNullException();
      }

      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.ThresholdFactoryAbi.json");
      using var reader = new StreamReader(stream);
      string tokenAbiJson = reader.ReadToEnd();

      var parameters = new object[] { campaignInfo, leasingInfo };

      // createCampaign hace new Campaign() + initialize(); en Amoy suele usar ~2.95M gas. Límite 5M evita revert por gas.
      var transaction = await _web3Service.ExecuteContractFunctionAsync(
        _settings.Value.Web3Settings.WalletPrivateKey,
        _settings.Value.Web3Settings.THRESHOLD_FACTORY,
        tokenAbiJson,
        "createCampaign",
        parameters,
        networkName: null,
        gasLimit: 5_000_000
      );

      var receipt = transaction.Receipt;
      var txHash = receipt.TransactionHash ?? string.Empty;

      if (receipt.Status != null && receipt.Status.Value == 0)
        throw new InvalidOperationException(
          $"La transacción de creación de campaña falló (revert). Revisa el contrato o el saldo de la wallet que firma. TxHash: {txHash}. Ver motivo del revert: https://amoy.polygonscan.com/tx/{txHash}");

      var eventCampaignCreation = transaction.Contract.GetEvent("CampaignCreated");
      var eventOutputs = eventCampaignCreation.DecodeAllEventsDefaultForEvent(receipt.Logs);
      if (eventOutputs == null || eventOutputs.Count == 0)
        throw new InvalidOperationException($"No se encontró el evento CampaignCreated en la transacción. La tx pudo haber revertido o el contrato tener otra versión. TxHash: {txHash}");

      var jObjectEvent = eventOutputs[0].Event.ConvertToJObject();
      var campaignEvent = JsonConvert.DeserializeObject<CampaignCreatedEvent>(jObjectEvent.ToString());
      if (campaignEvent?.Campaign == null)
        throw new InvalidOperationException($"No se pudo obtener la dirección de la campaña del evento. TxHash: {txHash}");

      return (campaignEvent.Campaign, txHash);
    }
    catch (IndexOutOfRangeException ex)
    {
      throw new InvalidOperationException(
        "No se pudo decodificar el evento CampaignCreated de la transacción (posible versión distinta del contrato). Revise el TxHash en el explorador de Polygon Amoy.", ex);
    }
    catch (Exception ex)
    {
      var message = ex.InnerException?.Message ?? ex.Message;
      if (message.Contains("insufficient funds", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException(
          "La wallet que firma la transacción no tiene suficiente MATIC para pagar el gas en Polygon Amoy. Recargue esa wallet con MATIC de testnet (faucet) y vuelva a intentar.");
      throw new Exception(ex.Message);
    }
  }

  public async Task<byte> GetCampaignStateAsync(string campaignAddress)
  {
    if (string.IsNullOrEmpty(campaignAddress))
      throw new ArgumentNullException(nameof(campaignAddress));

    var assembly = Assembly.GetExecutingAssembly();
    using var streamCampaign = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.CampaignAbi.json");
    using var readerCampaign = new StreamReader(streamCampaign!);
    string campaignAbiJson = readerCampaign.ReadToEnd();

    var state = await _web3Service.CallContractFunctionAsync<byte>(
      campaignAddress,
      campaignAbiJson,
      "state",
      Array.Empty<object>()
    );
    return state;
  }

  public async Task<(string LeasingCoreAddress, string TokenAddress, string TransactionHash)> FinalizeCampaign(string campaignAddress, bool brickleAssumeInsurance = false)
  {
    try
    {
      if (string.IsNullOrEmpty(campaignAddress))
        throw new ArgumentNullException(nameof(campaignAddress));

      var assembly = Assembly.GetExecutingAssembly();
      using var streamThreshold = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.ThresholdFactoryAbi.json");
      using var readerThreshold = new StreamReader(streamThreshold!);
      string thresholdAbiJson = readerThreshold.ReadToEnd();

      var parameters = new object[] { campaignAddress, brickleAssumeInsurance };
      // finalizeCampaign crea LeasingCore + LeasingToken + mint NFT + distributeTokens; consume ~3.85M gas
      var transaction = await _web3Service.ExecuteContractFunctionAsync(
        _settings.Value.Web3Settings.WalletPrivateKey,
        _settings.Value.Web3Settings.THRESHOLD_FACTORY,
        thresholdAbiJson,
        "finalizeCampaign",
        parameters,
        networkName: null,
        gasLimit: 5_000_000
      );

      var receipt = transaction.Receipt;
      var txHash = receipt.TransactionHash ?? string.Empty;

      if (receipt.Status != null && receipt.Status.Value == 0)
        throw new InvalidOperationException(
          $"La transacción de finalización de campaña falló (revert). TxHash: {txHash}. Ver: https://amoy.polygonscan.com/tx/{txHash}");

      // Extraer leasingCore del evento LeasingCreated en el receipt (más fiable que leer del Campaign).
      // El evento es emitido por LeasingCore.initialize(); el Address del log es el contrato que emitió = LeasingCore.
      string leasingCoreAddress;
      using (var streamLeasingCore = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.LeasingCoreAbi.json"))
      using (var readerLeasingCore = new StreamReader(streamLeasingCore!))
      {
        var leasingCoreAbiJson = readerLeasingCore.ReadToEnd();
        var web3 = new Nethereum.Web3.Web3(_settings.Value.Web3Settings.RpcUrl);
        var leasingCoreContract = web3.Eth.GetContract(leasingCoreAbiJson, campaignAddress);
        var eventLeasingCreated = leasingCoreContract.GetEvent("LeasingCreated");
        var leasingCreatedOutputs = eventLeasingCreated.DecodeAllEventsDefaultForEvent(receipt.Logs);
        if (leasingCreatedOutputs == null || leasingCreatedOutputs.Count == 0)
          throw new InvalidOperationException(
            $"No se encontró el evento LeasingCreated en la transacción. No se puede obtener leasingCore. TxHash: {txHash}");
        leasingCoreAddress = leasingCreatedOutputs[0].Log.Address;
        if (string.IsNullOrWhiteSpace(leasingCoreAddress))
          throw new InvalidOperationException(
            $"No se pudo obtener leasingCore del evento LeasingCreated. TxHash: {txHash}");
      }

      // baseToken se lee del Campaign (no cambia durante finalizeCampaign)
      using var streamCampaign = assembly.GetManifestResourceStream("BricklePlatform.Infrastructure.Constants.Contracts.CampaignAbi.json");
      using var readerCampaign = new StreamReader(streamCampaign!);
      string campaignAbiJson = readerCampaign.ReadToEnd();
      var tokenAddress = await _web3Service.CallContractFunctionAsync<string>(
        campaignAddress,
        campaignAbiJson,
        "baseToken",
        Array.Empty<object>()
      );

      return (leasingCoreAddress, tokenAddress ?? string.Empty, txHash);
    }
    catch (Exception ex)
    {
      throw new Exception(ex.Message);
    }
  }
}