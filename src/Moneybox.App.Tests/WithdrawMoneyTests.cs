using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Xunit;
using Moq;
using Moneybox.App.Features;

namespace Moneybox.App.Tests
{
    public class WithdrawMoneyTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<INotificationService> _mockNotificationService;
        private Guid _fromAccountId;
        private Account _fromAccount;
        private User _fromUser;
        private const decimal _withdrawalAmount = 600m;
        
        [Fact]
        public void CanCreateWithdrawMoneyFeature()
        {
            // Arrange
            InitialiseDependencies();
            // Act
            var withdrawMoneyFeature = CreateWithdrawMoneyFeature();
            // Assert
            Assert.NotNull(withdrawMoneyFeature);
        }

        [Fact]
        public void FailedWithdrawal_WithoutSufficientFunds_ThrowsException()
        {
            // Arrange
            InitialiseDependencies();

            // make sure insufficient funds to withdraw 600
            _fromAccount.Balance = 100m;
            
            // Act and Assert (due to exception checking)
            Assert.Throws<InvalidOperationException>(() => CreateWithdrawMoneyFeatureAndExecute());
        }
        
        [Fact]
        public void SuccessfulWithdrawal_ResultingInLowFunds_SendsNotification()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 1000m; // withdraw 600, balance will be below 500
            _fromAccount.Withdrawn = 100m;

            // Act
            CreateWithdrawMoneyFeatureAndExecute();

            // Assert
            _mockNotificationService.Verify(f => f.NotifyFundsLow(_fromUser.Email), Times.Once);
        }
        
        [Fact]
        public void SuccessfulWithdrawal_WithoutApproachingFundsLowLimit_DoesNotSendNotification()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 2000m; // withdraw 600, funds not low
            _fromAccount.Withdrawn = 100m;

            // Act
            CreateWithdrawMoneyFeatureAndExecute();

            // Assert
            _mockNotificationService.Verify(f => f.NotifyFundsLow(_fromUser.Email), Times.Never);
        }

        [Fact]
        public void SuccessfulWithdrawal_UpdatesInRepository()
        {
            // Arrange
            InitialiseDependencies();
            
            _fromAccount.Balance = 2000m; // withdraw 600, funds not low
            _fromAccount.Withdrawn = 100m;

            // Act
            CreateWithdrawMoneyFeatureAndExecute();

            // Assert
            _mockAccountRepository.Verify(f => f.Update(_fromAccount), Times.Once);
        }

        [Theory]
        [InlineData(2000,1000)]
        [InlineData(1000,1000)]
        public void SuccessfulWithdrawal_AccountObjectsHaveCorrectValues(decimal accountInitialBalanceValue, decimal accountInitialWithdrawnValue)
        {
            // Arrange
            InitialiseDependencies();

            _fromAccount.Balance = accountInitialBalanceValue;
            _fromAccount.Withdrawn = accountInitialWithdrawnValue;

            var expectedAccountBalanceValue = accountInitialBalanceValue - _withdrawalAmount;
            var expectedAccountWithdrawnValue = accountInitialWithdrawnValue + _withdrawalAmount;

            // Act
            CreateWithdrawMoneyFeatureAndExecute();

            // Assert
            Assert.Equal(expectedAccountBalanceValue, _fromAccount.Balance);
            Assert.Equal(expectedAccountWithdrawnValue, _fromAccount.Withdrawn);
        }

        private void InitialiseDependencies()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _fromUser = new User { Email = "account@user.com" };

            _fromAccountId = Guid.NewGuid();

            _fromAccount = new Account { Id = _fromAccountId, User = _fromUser, Balance = 0m, PaidIn = 0m, Withdrawn = 0m};

            _mockAccountRepository.Setup(f => f.GetAccountById(_fromAccountId)).Returns(_fromAccount);
        }
        
        private WithdrawMoney CreateWithdrawMoneyFeature()
        {
            return new WithdrawMoney(_mockAccountRepository.Object, _mockNotificationService.Object);
        }

        private void CreateWithdrawMoneyFeatureAndExecute()
        {
            var withdrawMoneyFeature = CreateWithdrawMoneyFeature();

            withdrawMoneyFeature.Execute(_fromAccountId, _withdrawalAmount);
        }
    }
}
