using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var account = GetAccount(fromAccountId);

            ProcessWithdrawal(account, amount);

            SendNotifications(account);

            Save(account);
        }

        protected Account GetAccount(Guid accountId)
        {
            return this.accountRepository.GetAccountById(accountId);
        }

        protected void ProcessWithdrawal(Account account, decimal amount)
        {
            account.Withdraw(amount);
        }

        protected void SendNotifications(Account account)
        {
            if (account.FundsLow())
            {
                this.notificationService.NotifyFundsLow(account.User.Email);
            }
        }

        protected void Save(Account account)
        {
            this.accountRepository.Update(account);
        }
    }
}