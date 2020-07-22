using Castle.Core.Logging;
using Contracts;
using Entities.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Repository;
using StocksBackendServer.Controllers;
using System;
using System.Collections.Generic;
using Xunit;

namespace StocksBackendTests
{
    public class UsersControllerTests
    {
        private UsersController controller;
        
        public UsersControllerTests()
        {
            //arrange
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => string.IsNullOrWhiteSpace(s)))).Returns((User)null);
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Returns(new User());

            var accountRepository = new Mock<IAccountRepository>();
            accountRepository.Setup(rep => rep.Get(It.IsAny<string>())).Returns(new Account());

            var stockRepository = new Mock<IStockRepository>();
            stockRepository.Setup(rep => rep.Get(It.IsAny<string>())).Returns(new List<Stock>() { new Stock() });

            var mockLogger = new Mock<ILoggerManager>();
            var repoWrapper = new RepositoryWrapper(accountRepository.Object, userRepository.Object, stockRepository.Object);

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)), It.Is<string>(s => !string.IsNullOrWhiteSpace(s))))
                .Returns(new User());
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => string.IsNullOrWhiteSpace(s)), It.Is<string>(s => string.IsNullOrWhiteSpace(s))))
                .Returns((User)null);

            controller = new UsersController(mockUserService.Object, mockLogger.Object, repoWrapper);
        }

        [Fact]
        public void Authenticate_Ok()
        {
            //act
            var response = controller.Authenticate(new User() { Username = "user", Password = "pass" });
            //assert
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void Authenticate_Error()
        {
            //act
            var response = controller.Authenticate(new User());
            //assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public void GetUserData_Ok()
        {
            //act
            var response = controller.GetUserData("12345");
            //assert
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void GetUserData_Error()
        {
            //act
            var response = controller.GetUserData(string.Empty);
            //assert
            Assert.IsType<NotFoundResult>(response);
        }
    }
}
