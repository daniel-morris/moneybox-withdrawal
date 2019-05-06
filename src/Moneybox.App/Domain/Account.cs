using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal LowFundsThreshold = 500m;
        public const decimal RmainingPayInLimitThreshold = 500m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void Withdraw(decimal amount)
        {
            ValidateFundsForWithdrawal(amount);

            DecreaseBalance(amount);
            IncreaseWithdrawn(amount);
        }

        public void Deposit(decimal amount)
        {
            ValidatePaidInLimitForDeposit(amount);

            IncreaseBalance(amount);
            IncreasePaidIn(amount);
        }

        public bool FundsLow()
        {
            return Balance < LowFundsThreshold;
        }

        public bool ApproachingPayInLimit()
        {
            return PayInLimit - PaidIn < RmainingPayInLimitThreshold;
        }

        protected void ValidateFundsForWithdrawal(decimal amount)
        {
            var newBalance = Balance - amount;
            if (newBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }
        }

        protected void ValidatePaidInLimitForDeposit(decimal amount)
        {
            var newPaidInValue = PaidIn + amount;
            if (newPaidInValue > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }
        }

        protected void IncreaseBalance(decimal amount)
        {
            Balance += amount;
        }
        protected void DecreaseBalance(decimal amount)
        {
            Balance -= amount;
        }
        protected void IncreaseWithdrawn(decimal amount)
        {
            Withdrawn += amount;
        }
        protected void DecreaseWithdrawn(decimal amount)
        {
            Withdrawn -= amount;
        }
        protected void IncreasePaidIn(decimal amount)
        {
            PaidIn += amount;
        }
        protected void DecreasePaidIn(decimal amount)
        {
            PaidIn -= amount;
        }
    }
}