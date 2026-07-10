using System.Numerics;
using BricklePlatform.Domain.Models;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BricklePlatform.Test.Integration;

public sealed class Eip2612PermitSigner
{
    private const string Erc20PermitAbi = """
    [
      { "type": "function", "name": "name", "inputs": [], "outputs": [{ "name": "", "type": "string" }], "stateMutability": "view" },
      { "type": "function", "name": "nonces", "inputs": [{ "name": "owner", "type": "address" }], "outputs": [{ "name": "", "type": "uint256" }], "stateMutability": "view" }
    ]
    """;

    private readonly Web3 _web3;

    public Eip2612PermitSigner(string rpcUrl)
    {
        _web3 = new Web3(rpcUrl);
    }

    public async Task<PermitSignatureDto> SignPermitAsync(string ownerPrivateKey, string tokenAddress, string spender, BigInteger value, BigInteger deadline)
    {
        var owner = new Account(ownerPrivateKey);
        var contract = _web3.Eth.GetContract(Erc20PermitAbi, tokenAddress);
        var name = await contract.GetFunction("name").CallAsync<string>().ConfigureAwait(false);
        var nonce = await contract.GetFunction("nonces").CallAsync<BigInteger>(owner.Address).ConfigureAwait(false);
        var chainId = await _web3.Eth.ChainId.SendRequestAsync().ConfigureAwait(false);

        var typedData = EIP2612TypeFactory.GetTypedDefinition();
        typedData.Domain = new Nethereum.ABI.EIP712.Domain
        {
            Name = name,
            Version = "1",
            ChainId = chainId.Value,
            VerifyingContract = tokenAddress
        };

        var permit = new Permit
        {
            Owner = owner.Address,
            Spender = spender,
            Value = value,
            Nonce = nonce,
            Deadline = deadline
        };

        var digest = Eip712TypedDataEncoder.Current.EncodeAndHashTypedData(permit, typedData);
        var signature = new MessageSigner().SignAndCalculateV(digest, ownerPrivateKey);
        var v = signature.V[0] < 27 ? (short)(signature.V[0] + 27) : (short)signature.V[0];

        return new PermitSignatureDto
        {
            V = v,
            R = "0x" + Convert.ToHexString(signature.R).ToLowerInvariant(),
            S = "0x" + Convert.ToHexString(signature.S).ToLowerInvariant()
        };
    }
}
