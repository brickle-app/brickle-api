using BricklePlatform.Domain.DTOs.Wallet;
using BricklePlatform.Domain.Entities;
using BricklePlatform.Domain.Interfaces;
using BricklePlatform.Infrastructure.Services;
using Moq;
using Xunit;

namespace BricklePlatform.Test.Services;

public class WalletBackupServiceTests
{
    [Fact]
    public async Task SaveAsyncStoresEncryptedBackupForAuthenticatedUsersWallet()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "0x1111111111111111111111111111111111111111");
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        walletBackupRepository.Setup(r => r.UpsertAsync(It.IsAny<WalletBackup>()))
            .ReturnsAsync((WalletBackup backup) => backup);
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        var request = ValidRequest(user.WalletAddress!);

        var result = await service.SaveAsync(userId, request);

        walletBackupRepository.Verify(r => r.UpsertAsync(It.Is<WalletBackup>(backup =>
            backup.UserId == userId &&
            backup.WalletAddress == user.WalletAddress &&
            backup.EncryptedPrivateKey == request.EncryptedPrivateKey)), Times.Once);
        Assert.Equal(user.WalletAddress, result.WalletAddress);
        Assert.Equal(request.EncryptedPrivateKey, result.EncryptedPrivateKey);
    }

    [Fact]
    public async Task SaveAsyncRejectsWalletAddressThatDoesNotBelongToAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(CreateUser(userId, "0x1111111111111111111111111111111111111111"));
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(userId, ValidRequest("0x2222222222222222222222222222222222222222")));

        walletBackupRepository.Verify(r => r.UpsertAsync(It.IsAny<WalletBackup>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsyncRejectsPayloadsThatLookLikePlaintextPrivateKeys()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "0x1111111111111111111111111111111111111111");
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        var request = ValidRequest(user.WalletAddress!);
        request.EncryptedPrivateKey = "0x" + new string('a', 64);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveAsync(userId, request));

        walletBackupRepository.Verify(r => r.UpsertAsync(It.IsAny<WalletBackup>()), Times.Never);
    }

    [Fact]
    public async Task UpgradeActiveWalletAsyncUpdatesUserWalletAndStoresEncryptedBackup()
    {
        var userId = Guid.NewGuid();
        var newWalletAddress = "0x2222222222222222222222222222222222222222";
        var user = CreateUser(userId, "0x1111111111111111111111111111111111111111");
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        userRepository.Setup(r => r.GetByWalletAddressAsync(newWalletAddress)).ReturnsAsync((User?)null);
        walletBackupRepository.Setup(r => r.UpsertAsync(It.IsAny<WalletBackup>()))
            .ReturnsAsync((WalletBackup backup) => backup);
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        var request = ValidRequest(newWalletAddress);
        request.EncryptionVersion = "ethers-keystore-v1-backup-code";

        var result = await service.UpgradeActiveWalletAsync(userId, request);

        Assert.Equal(newWalletAddress, user.WalletAddress);
        Assert.Equal(newWalletAddress, result.WalletAddress);
        userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        walletBackupRepository.Verify(r => r.UpsertAsync(It.Is<WalletBackup>(backup =>
            backup.UserId == userId &&
            backup.WalletAddress == newWalletAddress &&
            backup.EncryptedPrivateKey == request.EncryptedPrivateKey)), Times.Once);
    }

    [Fact]
    public async Task UpgradeActiveWalletAsyncRejectsWalletAddressOwnedByAnotherUser()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var newWalletAddress = "0x2222222222222222222222222222222222222222";
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(CreateUser(userId, "0x1111111111111111111111111111111111111111"));
        userRepository.Setup(r => r.GetByWalletAddressAsync(newWalletAddress))
            .ReturnsAsync(CreateUser(otherUserId, newWalletAddress));
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        var request = ValidRequest(newWalletAddress);
        request.EncryptionVersion = "ethers-keystore-v1-backup-code";

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpgradeActiveWalletAsync(userId, request));

        userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        walletBackupRepository.Verify(r => r.UpsertAsync(It.IsAny<WalletBackup>()), Times.Never);
    }

    [Fact]
    public async Task UpgradeActiveWalletAsyncRequiresBackupCodeEncryptionVersion()
    {
        var userId = Guid.NewGuid();
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(CreateUser(userId, "0x1111111111111111111111111111111111111111"));
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpgradeActiveWalletAsync(userId, ValidRequest("0x2222222222222222222222222222222222222222")));

        walletBackupRepository.Verify(r => r.UpsertAsync(It.IsAny<WalletBackup>()), Times.Never);
    }

    [Fact]
    public async Task GetAsyncReturnsStoredEncryptedBackupForAuthenticatedUser()
    {
        var userId = Guid.NewGuid();
        var backup = WalletBackup.Create(
            userId,
            "0x1111111111111111111111111111111111111111",
            "{\"crypto\":{}}",
            "ethers-keystore-v1",
            "ethers-json-keystore",
            "scrypt",
            "{\"n\":131072}");
        var userRepository = new Mock<IUserRepository>();
        var walletBackupRepository = new Mock<IWalletBackupRepository>();
        userRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(CreateUser(userId, backup.WalletAddress));
        walletBackupRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(backup);
        var service = new WalletBackupService(userRepository.Object, walletBackupRepository.Object);

        var result = await service.GetAsync(userId);

        Assert.Equal(backup.WalletAddress, result.WalletAddress);
        Assert.Equal(backup.EncryptedPrivateKey, result.EncryptedPrivateKey);
    }

    private static WalletBackupRequestDto ValidRequest(string walletAddress) => new()
    {
        WalletAddress = walletAddress,
        EncryptedPrivateKey = "{\"crypto\":{\"ciphertext\":\"abc\"}}",
        EncryptionVersion = "ethers-keystore-v1",
        Cipher = "ethers-json-keystore",
        Kdf = "scrypt",
        KdfParams = new Dictionary<string, object> { ["n"] = 131072, ["r"] = 8, ["p"] = 1 }
    };

    private static User CreateUser(Guid id, string walletAddress)
    {
        var user = User.Create(
            firstName: "Ada",
            lastName: "Lovelace",
            email: $"{id:N}@example.com",
            phoneNumber: "3000000000",
            termsAccepted: true,
            passwordHash: Array.Empty<byte>(),
            passwordSalt: Array.Empty<byte>(),
            walletAddress: walletAddress);

        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);
        return user;
    }
}
