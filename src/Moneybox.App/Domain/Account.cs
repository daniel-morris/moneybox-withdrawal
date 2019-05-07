using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal PayInLimit = 4000m;
        public const decimal LowFundsThreshold = 500m;
        public const decimal RemainingPayInLimitThreshold = 500m;

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
            return PayInLimit - PaidIn < RemainingPayInLimitThreshold;
        }

        private void ValidateFundsForWithdrawal(decimal amount)
        {
            var newBalance = Balance - amount;
            if (newBalance < decimal.Zero)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }
        }

        private void ValidatePaidInLimitForDeposit(decimal amount)
        {
            var newPaidInValue = PaidIn + amount;
            if (newPaidInValue > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }
        }

        private void IncreaseBalance(decimal amount)
        {
            Balance += amount;
        }
        private void DecreaseBalance(decimal amount)
        {
            Balance -= amount;
        }
        private void IncreaseWithdrawn(decimal amount)
        {
            Withdrawn += amount;
        }
        private void IncreasePaidIn(decimal amount)
        {
            PaidIn += amount;
        }
    }
}