using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Repository;
using StocksBackend.Controllers;
using System;
using System.Collections.Generic;
using Xunit;

namespace StocksBackendTests
{
    public class StocksControllerTests
    {
        private StocksController controller;
        private Mock<ILoggerManager> mockLogger;
        private RepositoryWrapper repoWrapper;

        public StocksControllerTests()
        {
            //arrange
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => string.IsNullOrWhiteSpace(s)))).Returns((User)null);
            userRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)))).Returns(new User());

            var accountRepository = new Mock<IAccountRepository>();
            accountRepository.Setup(rep => rep.Get(It.Is<string>(s => s.Equals("00000")))).Returns((Account)null);
            accountRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s) && !s.Equals("00000")))).Returns(new Account() { Funds = 10 });
            
            var stockRepository = new Mock<IStockRepository>();
            stockRepository.Setup(rep => rep.Get(It.Is<string>(s => s.Equals("00001")))).Returns((List<Stock>)null);
            stockRepository.Setup(rep => rep.Get(It.Is<string>(s => !string.IsNullOrWhiteSpace(s) && !s.Equals("00001")))).Returns(new List<Stock>() { new Stock() });
            stockRepository.Setup(rep => rep.Get(It.IsAny<string>(), It.Is<string>(s => s.Equals("ABC")))).Returns((Stock)null);
            stockRepository.Setup(rep => rep.Get(It.IsAny<string>(), It.Is<string>(s => s.Equals("DEF")))).Returns(new Stock { StockCode = "DEF", Ammount = 2 });

            mockLogger = new Mock<ILoggerManager>();
            repoWrapper = new RepositoryWrapper(accountRepository.Object, userRepository.Object, stockRepository.Object);

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => !string.IsNullOrWhiteSpace(s)), It.Is<string>(s => !string.IsNullOrWhiteSpace(s))))
                .Returns(new User());
            mockUserService.Setup(svc => svc.Authenticate(It.Is<string>(s => string.IsNullOrWhiteSpace(s)), It.Is<string>(s => string.IsNullOrWhiteSpace(s))))
                .Returns((User)null);

            var mockStocks = new Mock<IStocksManager>();
            mockStocks.Setup(stocks => stocks.stocksValues).Returns(new List<StockValue> { new StockValue { StockCode = "ABC", Value = 15 }, new StockValue { StockCode = "DEF", Value = 5 } });

            controller = new StocksController(mockLogger.Object, repoWrapper, mockStocks.Object);
        }

        [Fact]
        public void ListQuotes_Ok()
        {
            var response = controller.ListQuotes();
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void ListQuotes_Error()
        {
            var mockStocks = new Mock<IStocksManager>();
            mockStocks.Setup(stocks => stocks.stocksValues).Returns(new List<StockValue>());
            controller = new StocksController(mockLogger.Object, repoWrapper, mockStocks.Object);
            var response = controller.ListQuotes();
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public void ListInvestments_Ok()
        {
            var response = controller.ListInvestments("12345");
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public void ListInvestments_Error()
        {
            var response = controller.ListInvestments(string.Empty);
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public void BuyStocks_Ok()
        {
            var response = controller.BuyStocks("00001", new Stock() { Ammount = 2, StockCode = "DEF" });
            Assert.IsType<OkResult>(response);
        }

        [Fact]
        public void BuyStocks_Error()
        {
            //Bad request
            var response = controller.BuyStocks(string.Empty, null);
            Assert.IsType<BadRequestObjectResult>(response);

            //Buying 0 stocks
            response = controller.BuyStocks(string.Empty, new Stock());
            Assert.IsType<BadRequestObjectResult>(response);

            //User not found
            response = controller.BuyStocks(string.Empty, new Stock() { Ammount = 1 });
            Assert.IsType<NotFoundResult>(response);

            //Account not found
            response = controller.BuyStocks("00000", new Stock() { Ammount = 1, StockCode = "ZZZ" });
            Assert.IsType<NotFoundObjectResult>(response);

            //Requested stock not found
            response = controller.BuyStocks("12345", new Stock() { Ammount = 1, StockCode = "ZZZ" });
            Assert.IsType<NotFoundObjectResult>(response);

            //Insuficient Funds
            response = controller.BuyStocks("12345", new Stock() { Ammount = 1, StockCode = "ABC" });
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public void SellStocks_Ok()
        {
            //User selling more than he has
            var response = controller.SellStocks("12345", new Stock() { Ammount = 1, StockCode = "DEF" });
            Assert.IsType<OkResult>(response);
        }

        [Fact]
        public void SellStocks_Error()
        {
            //Bad request
            var response = controller.SellStocks(string.Empty, null);
            Assert.IsType<BadRequestObjectResult>(response);

            //Selling 0 stocks
            response = controller.SellStocks(string.Empty, new Stock());
            Assert.IsType<BadRequestObjectResult>(response);

            //User not found
            response = controller.SellStocks(string.Empty, new Stock() { Ammount = 1 });
            Assert.IsType<NotFoundResult>(response);

            //Account not found
            response = controller.SellStocks("00000", new Stock() { Ammount = 1, StockCode = "ZZZ" });
            Assert.IsType<NotFoundObjectResult>(response);

            //Requested stock not found
            response = controller.SellStocks("12345", new Stock() { Ammount = 1, StockCode = "ZZZ" });
            Assert.IsType<NotFoundObjectResult>(response);

            //User doesnt have that stock
            response = controller.SellStocks("12345", new Stock() { Ammount = 1, StockCode = "ABC" });
            Assert.IsType<NotFoundObjectResult>(response);

            //User selling more than he has
            response = controller.SellStocks("12345", new Stock() { Ammount = 3, StockCode = "DEF" });
            Assert.IsType<BadRequestObjectResult>(response);
        }
    }
}
