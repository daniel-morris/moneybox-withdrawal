using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = GetAccount(fromAccountId);
            var to = GetAccount(toAccountId);

            ProcessTransfer(from, to, amount);

            SendNotifications(from, to);

            Save(from, to);
        }

        protected Account GetAccount(Guid accountId)
        {
            return this.accountRepository.GetAccountById(accountId);
        }

        protected void ProcessTransfer(Account from, Account to, decimal amount)
        {
            from.Withdraw(amount);
            to.Deposit(amount);
        }

        protected void SendNotifications(Account from, Account to)
        {
            if (from.FundsLow())
            {
                this.notificationService.NotifyFundsLow(from.User.Email);
            }

            if (to.ApproachingPayInLimit())
            {
                this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
        }

        protected void Save(Account from, Account to)
        {
            this.accountRepository.Update(from);
            this.accountRepository.Update(to);
        }
    }
}