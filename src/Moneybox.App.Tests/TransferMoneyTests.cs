using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Xunit;
using Moq;
using Moneybox.App.Features;

namespace Moneybox.App.Tests
{
    public class TransferMoneyTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private Guid _fromAccountId;
        private Guid _toAccountId;
        private Account _fromAccount;
        private Account _toAccount;
        private User _fromUser;
        private User _toUser;
        private const decimal _transferAmount = 600m;
        
        [Fact]
        public void CanCreateTransferMoneyFeature()
        {
            // Arrange
            InitialiseDependencies();
            // Act
            var transferMoneyFeature = CreateTransferMoneyFeature();
            // Assert
            Assert.NotNull(transferMoneyFeature);
        }

        [Fact]
        public void FailedTransfer_WithoutSufficientFunds_ThrowsException()
        {
            // Arrange
            InitialiseDependencies();

            // make sure insufficient funds to transfer 600
            _fromAccount.Balance = 100m;
            
            // Act and Assert (due to exception checking)
            Assert.Throws<InvalidOperationException>(() => CreateTransferMoneyFeatureAndExecute());
        }

        [Fact]
        public void FailedTransfer_WithoutSufficientPaidInLimit_ThrowsException()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 5000m;
            _fromAccount.Withdrawn = 1000m;
            
            _toAccount.Balance = 3500m;
            _toAccount.PaidIn = 3500m; // limit is 4000, so shouldn't be able to transfer 600
            
            // Act and Assert (due to exception checking)
            Assert.Throws<InvalidOperationException>(() => CreateTransferMoneyFeatureAndExecute());
        }

        [Fact]
        public void SuccessfulTransfer_ResultingInLowFunds_SendsNotification()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 1000m; // withdraw 600, balance will be below 500
            _fromAccount.Withdrawn = 100m;
            
            _toAccount.Balance = 100m;
            _toAccount.PaidIn = 100m;
            
            // Act
            CreateTransferMoneyFeatureAndExecute();

            // Assert
            _mockNotificationService.Verify(f => f.NotifyFundsLow(_fromUser.Email), Times.Once);
        }

        [Fact]
        public void SuccessfulTransfer_ResultingInApproachingPaidInLimit_SendsNotification()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 2000m;
            _fromAccount.Withdrawn = 100m;
            
            _toAccount.Balance = 100m;
            _toAccount.PaidIn = 3000m; // pay in 600, approaching limit, with 400 remaining
            
            // Act
            CreateTransferMoneyFeatureAndExecute();

            // Assert
            _mockNotificationService.Verify(f => f.NotifyApproachingPayInLimit(_toUser.Email), Times.Once);
        }

        [Fact]
        public void SuccessfulTransfer_WithoutApproachingAnyLimits_DoesNotSendNotifications()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 2000m; // withdraw 600, funds not low
            _fromAccount.Withdrawn = 100m;
            
            _toAccount.Balance = 100m;
            _toAccount.PaidIn = 2000m; // pay in 600, not apporaching PaidIn l
            
            // Act
            CreateTransferMoneyFeatureAndExecute();

            // Assert
            _mockNotificationService.Verify(f => f.NotifyFundsLow(_fromUser.Email), Times.Never);
            _mockNotificationService.Verify(f => f.NotifyApproachingPayInLimit(_toUser.Email), Times.Never);
        }

        [Fact]
        public void SuccessfulTransfer_UpdatesInRepository()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 2000m; // withdraw 600, funds not low
            _fromAccount.Withdrawn = 100m;
            
            _toAccount.Balance = 100m;
            _toAccount.PaidIn = 2000m; // pay in 600, not apporaching PaidIn limit
            
            // Act
            CreateTransferMoneyFeatureAndExecute();

            // Assert
            _mockAccountRepository.Verify(f => f.Update(_fromAccount), Times.Once);
            _mockAccountRepository.Verify(f => f.Update(_toAccount), Times.Once);
        }

        [Theory]
        [InlineData(2000,2000,1000,2000)]
        [InlineData(1000,1500,1000,2500)]
        public void SuccessfulTransfer_AccountObjectsHaveCorrectValues(decimal fromAccountInitialBalanceValue, 
            decimal toAccountInitialBalanceValue, decimal fromAccountInitialWithdrawnValue, decimal toAccountInitialPaidInValue)
        {
            // Arrange
            InitialiseDependencies();

            _fromAccount.Balance = fromAccountInitialBalanceValue;
            _fromAccount.Withdrawn = fromAccountInitialWithdrawnValue;

            _toAccount.Balance = toAccountInitialBalanceValue;
            _toAccount.PaidIn = toAccountInitialPaidInValue;

            var expectedFromAccountBalanceValue = fromAccountInitialBalanceValue - _transferAmount;
            var expectedFromAccountWithdrawnValue = fromAccountInitialWithdrawnValue + _transferAmount;

            var expectedToAccountBalanceValue = toAccountInitialBalanceValue + _transferAmount;
            var expectedToAccountPaidInValue = toAccountInitialPaidInValue + _transferAmount;

            // Act
            CreateTransferMoneyFeatureAndExecute();

            // Assert
            Assert.Equal(expectedFromAccountBalanceValue, _fromAccount.Balance);
            Assert.Equal(expectedFromAccountWithdrawnValue, _fromAccount.Withdrawn);

            Assert.Equal(expectedToAccountBalanceValue, _toAccount.Balance);
            Assert.Equal(expectedToAccountPaidInValue, _toAccount.PaidIn);

        }

        private void InitialiseDependencies()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _fromUser = new User { Email = "from@user.com" };
            _toUser = new User { Email = "to@user.com" };

            _fromAccountId = Guid.NewGuid();
            _toAccountId = Guid.NewGuid();

            _fromAccount = new Account { Id = _fromAccountId, User = _fromUser, Balance = 0m, PaidIn = 0m, Withdrawn = 0m};
            _toAccount = new Account { Id = _toAccountId, User = _toUser, Balance = 0m, PaidIn = 0m, Withdrawn = 0m };

            _mockAccountRepository.Setup(f => f.GetAccountById(_fromAccountId)).Returns(_fromAccount);
            _mockAccountRepository.Setup(f => f.GetAccountById(_toAccountId)).Returns(_toAccount);
        }
        
        private TransferMoney CreateTransferMoneyFeature()
        {
            return new TransferMoney(_mockAccountRepository.Object, _mockNotificationService.Object);
        }

        private void CreateTransferMoneyFeatureAndExecute()
        {
            var transferMoneyFeature = CreateTransferMoneyFeature();

            transferMoneyFeature.Execute(_fromAccountId, _toAccountId, _transferAmount);
        }
    }
}
