using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Repository;
using StocksBackend.Controllers;
using System;
using System.Collections.Generic;
using Xunit;

namespace StocksBackendTests
{
    public class AccountsControllerTests
    {
        private AccountsController controller;

        public AccountsControllerTests()
        {
            //arrange
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => string.IsNullOrWhiteSpace(s)))).Returns((User)null);
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Returns(new User());

            var accountRepository = new Mock<IAccountRepository>();
            accountRepository.Setup(rep => rep.Get(It.Is<string>(s => s.Equals("00000")))).Returns((Account)null);
            accountRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s) && !s.Equals("00000")))).Returns(new Account() { Funds = 10 });

            var stockRepository = new Mock<IStockRepository>();
            stockRepository.Setup(rep => rep.Get(It.Is<string>(s => string.IsNullOrWhiteSpace(s)))).Returns((List<Stock>)null);
            stockRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Returns(new List<Stock>() { new Stock() });

            var mockLogger = new Mock<ILoggerManager>();
            var repoWrapper = new RepositoryWrapper(accountRepository.Object, userRepository.Object, stockRepository.Object);

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)), It.Is<string>(s => !string.IsNullOrWhiteSpace(s))))
                .Returns(new User());
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => string.IsNullOrWhiteSpace(s)), It.Is<string>(s => string.IsNullOrWhiteSpace(s))))
                .Returns((User)null);

            controller = new AccountsController(mockLogger.Object, repoWrapper);
        }

        [Fact]
        public void ViewFunds_Ok()
        {
            var response = controller.ViewFunds("12345");
            Assert.IsType<OkObjectResult>(response);
        }
        
        [Fact]
        public void ViewFunds_Error()
        {
            var response = controller.ViewFunds(string.Empty);
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public void DepositFunds_Ok()
        {
            var response = controller.DepositFunds("00000", new Account() { Ammount = 1 });
            Assert.IsType<OkResult>(response);
        }
        [Fact]
        public void DepositFunds_Error()
        {
            //Bad request
            var response = controller.DepositFunds(string.Empty, null);
            Assert.IsType<BadRequestObjectResult>(response);

            //depositing value of 0
            response = controller.DepositFunds(string.Empty, new Account());
            Assert.IsType<BadRequestObjectResult>(response);

            //User not found
            response = controller.DepositFunds(string.Empty, new Account() { Ammount = 1 });
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public void WithdrawFunds_Ok()
        {
            var response = controller.WithdrawFunds("12345", new Account() { Ammount = 1 });
            Assert.IsType<OkResult>(response);
        }

        [Fact]
        public void WithdrawFunds_Error()
        {
            //bad request
            var response = controller.WithdrawFunds(string.Empty, null);
            Assert.IsType<BadRequestObjectResult>(response);

            //withdrawing 0
            response = controller.WithdrawFunds(string.Empty, new Account());
            Assert.IsType<BadRequestObjectResult>(response);

            //user not found
            response = controller.WithdrawFunds(string.Empty, new Account() { Ammount = 1 });
            Assert.IsType<NotFoundResult>(response);

            //user doesnt have funds
            response = controller.WithdrawFunds("00000", new Account() { Ammount = 1 });
            Assert.IsType<BadRequestObjectResult>(response);
        }
    }
}
