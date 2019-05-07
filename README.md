# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* You should spend no more than 1 hour on this task, although there is no time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must compile and run first time
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed your work, send us a link to your public repository.

Good luck!

## Details around decisions, assumptions and what's missing

* Decision not to add new interfaces, as I didn't have access to your container and assume that this is part of a bigger solution, to be tested within.
* Decision to change the calculation for the "Withdrawn" property when Withdrawing from an Account, I feel the amount withdrawn should increase not decrease. I did this with the assumption that "Withdrawn" is the amount currently withdrawn, rather than the remaining limit that could be withdrawn.
* Decision to change the order of the logic within TransferMoney.Execute, such that if a transfer would fail due to the PaidIn limit on the "to account", the "from account user" wouldn't get a low funds notification. As no funds from the "from account" would have actually been withdrawn, in the failed transfer attempt.
* I feel that if there is logic in the Domain objects, then they should also be tested with unit tests. This is good for regression reasons and also to be careful and accurate, due to this code handling peoples money. I missed these due to lack of time.
* An alternative way of solving this, would be to have a folder of Validators, that would do the validation work. These would be injected into the Features, shared between them and tested separately. As a result adding new features could be quicker.
* I decided to keep my solution simple and clean, opting for readability and maintainability.
