using Nethereum.Web3.Accounts;

namespace BricklePlatform.Test.Integration;

public sealed class AmoyRelayerTestConfig
{
    public const string MockErc20 = "0x8216d4b1d7cceeb924db5c82cd64f934cabdd930";
    public const string Paymaster = "0x98b711341c701166c3c9492ae1a8577d5bc29eae";
    public const string ThresholdFactory = "0x09f35875810547963d45e6e6feb45a82040b6e64";
    public const string BrickleNft = "0x9fd448a25284cdbb2a0841980d7a92390d0bc910";
    public const string TestUserAddress = "0x05703526dB38D9b2C661c9807367C14EB98b6c54";

    private AmoyRelayerTestConfig(string rpcUrl, string walletPrivateKey, string relayerPrivateKey, string userPrivateKey)
    {
        RpcUrl = rpcUrl;
        WalletPrivateKey = walletPrivateKey;
        RelayerPrivateKey = relayerPrivateKey;
        UserPrivateKey = userPrivateKey;
        WalletAddress = new Account(walletPrivateKey).Address;
        RelayerAddress = new Account(relayerPrivateKey).Address;
        UserAddress = new Account(userPrivateKey).Address;
    }

    public string RpcUrl { get; }
    public string WalletPrivateKey { get; }
    public string RelayerPrivateKey { get; }
    public string UserPrivateKey { get; }
    public string WalletAddress { get; }
    public string RelayerAddress { get; }
    public string UserAddress { get; }

    public static bool TryLoad(out AmoyRelayerTestConfig? config, out string skipReason)
    {
        config = null;
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_AMOY_RELAYER_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            skipReason = "Set RUN_AMOY_RELAYER_TESTS=true to run Polygon Amoy relayer integration tests.";
            return false;
        }

        var rpcUrl = Environment.GetEnvironmentVariable("AMOY_RPC_URL");
        var walletPrivateKey = Environment.GetEnvironmentVariable("AMOY_WALLET_PRIVATE_KEY");
        var relayerPrivateKey = Environment.GetEnvironmentVariable("AMOY_RELAYER_PRIVATE_KEY");
        var userPrivateKey = Environment.GetEnvironmentVariable("AMOY_USER_PRIVATE_KEY");

        var missing = new[]
        {
            (Name: "AMOY_RPC_URL", Value: rpcUrl),
            (Name: "AMOY_WALLET_PRIVATE_KEY", Value: walletPrivateKey),
            (Name: "AMOY_RELAYER_PRIVATE_KEY", Value: relayerPrivateKey),
            (Name: "AMOY_USER_PRIVATE_KEY", Value: userPrivateKey)
        }.Where(item => string.IsNullOrWhiteSpace(item.Value)).Select(item => item.Name).ToArray();

        if (missing.Length > 0)
        {
            skipReason = "Missing Amoy integration environment variables: " + string.Join(", ", missing);
            return false;
        }

        config = new AmoyRelayerTestConfig(rpcUrl!, walletPrivateKey!, relayerPrivateKey!, userPrivateKey!);
        if (!string.Equals(config.UserAddress, TestUserAddress, StringComparison.OrdinalIgnoreCase))
        {
            skipReason = $"AMOY_USER_PRIVATE_KEY resolves to {config.UserAddress}, expected {TestUserAddress}.";
            config = null;
            return false;
        }

        skipReason = string.Empty;
        return true;
    }
}
