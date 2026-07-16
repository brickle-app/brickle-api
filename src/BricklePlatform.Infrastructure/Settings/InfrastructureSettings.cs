namespace BricklePlatform.Infrastructure.Settings;

public class InfrastructureSettings
{
    public DatabaseSettings DatabaseSettings { get; set; } = new();
    public AzureSettings AzureSettings { get; set; } = new();
    public Web3Settings Web3Settings { get; set; } = new();
    public WebhookSettings WebhookSettings { get; set; } = new();
    public HttpClientSettings HttpClientSettings { get; set; } = new();
    public ExpoSettings ExpoSettings { get; set; } = new();
    public EmailSettings EmailSettings { get; set; } = new();
    public JwtSettings JwtSettings { get; set; } = new();
    public FirebaseSettings FirebaseSettings { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; }
}

public class AzureSettings
{
    public string ConnectionString { get; set; }
    public string BlobName { get; set; }
    public string LogsTableName { get; set; }
    public string UserActivityLogsTableName { get; set; }
}

public class Web3Settings
{
    public string RpcUrl { get; set; }
    public string Network { get; set; } = "testnet";
    /// <summary>
    /// Clave de la wallet de operaciones en servidor. Firma transacciones que escriben en cadena (campañas, cierre residual, etc.).
    /// Para <c>makeLastLeasingPayment</c>, esta cuenta paga el gas; el stablecoin del cierre sale del saldo del LeasingCore (acumulado a inversores), no de esta wallet.
    /// Debe tener saldo nativo suficiente en la red configurada (<see cref="RpcUrl"/>).
    /// </summary>
    public string WalletPrivateKey { get; set; } = string.Empty;
    /// <summary>
    /// Dedicated private key for the gas sponsor relayer. This key signs Paymaster transactions only.
    /// Do not reuse the Brickle operations wallet here.
    /// </summary>
    public string RelayerPrivateKey { get; set; } = string.Empty;
    /// <summary>
    /// Minimum native balance, in ether units, considered healthy for the relayer status endpoint.
    /// </summary>
    public decimal RelayerMinNativeBalance { get; set; } = 0.01m;
    public string BASE_TOKEN { get; set; }
    public string THRESHOLD_FACTORY { get; set; }
    public string PAYMASTER { get; set; }
    public string BRICKLE_NFT { get; set; }
    /// <summary>
    /// Wallet Brickle (operaciones internas / fallback). En el flujo Paymaster, el EIP-2612 permit lo firma
    /// la wallet del pagador (típicamente <c>User.WalletAddress</c>), no esta dirección.
    /// </summary>
    public string PaymentWalletAddress { get; set; } = "0xB818f59e7D46b5F17CfE66ef42cd01155a052e7C";
}

public class WebhookSettings
{
    public string Url { get; set; }
}

public class HttpClientSettings
{
    public int TimeoutSeconds { get; set; }
    public int MaxRetries { get; set; }
    public int RetryDelaySeconds { get; set; }
}

public class ExpoSettings
{
    public string PushEndpoint { get; set; }
}

public class EmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string LogoImageUrl { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

public class FirebaseSettings
{
    public string ProjectId { get; set; } = string.Empty;
    /// <summary>
    /// Path to the Firebase service account JSON file.
    /// In production, prefer using GOOGLE_APPLICATION_CREDENTIALS env var.
    /// </summary>
    public string CredentialsFilePath { get; set; } = string.Empty;
}
