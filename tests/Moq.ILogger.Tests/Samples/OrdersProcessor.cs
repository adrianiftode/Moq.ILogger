using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;


// ReSharper disable once CheckNamespace
namespace Moq.Tests.Samples
{
    public class PaymentsProcessor
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsProcessor> _logger;

        public PaymentsProcessor(IOrdersRepository ordersRepository, 
            IPaymentService paymentService, 
            ILogger<PaymentsProcessor> logger)
        {
            _ordersRepository = ordersRepository;
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task ProcessOutstandingOrders()
        {
            var outstandingOrders = await _ordersRepository.GetOutstandingOrders();
            foreach (var order in outstandingOrders)
            {
                try
                {
                    var paymentTransaction = await _paymentService.CompletePayment(order);
                    _logger.LogInformation("Order with {orderReference} was paid {at} by {customerEmail}, having {transactionId}", order.OrderReference, paymentTransaction.CreateOn, order.CustomerEmail, paymentTransaction.TransactionId);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "An exception occurred while completing the payment for {orderReference}", order.OrderReference);
                }
            }
            _logger.LogInformation("A batch of {0} outstanding orders was completed", outstandingOrders.Count);
        }
    }

    public interface IPaymentService
    {
        Task<PaymentTransaction> CompletePayment(Order order);
    }

    public interface IOrdersRepository
    {
        Task<IReadOnlyCollection<Order>> GetOutstandingOrders();
    }

    public class PaymentTransaction
    {
        public DateTime CreateOn { get; set; }
        public string TransactionId { get; set; }
    }

    public class Order
    {
        public string OrderReference { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class PaymentsProcessorTests
    {
        [Fact]
        public async Task Processing_outstanding_orders_logs_order_and_transaction_data_for_each_completed_payment()
        {
            // Arrange
            var ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(c => c.GetOutstandingOrders())
                .ReturnsAsync(GenerateOutstandingOrders(100));

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock
                .Setup(c => c.CompletePayment(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => new PaymentTransaction
                {
                    TransactionId = $"TRX-{order.OrderReference}"
                });

            var loggerMock = new Mock<ILogger<PaymentsProcessor>>();

            var sut = new PaymentsProcessor(ordersRepositoryMock.Object, paymentServiceMock.Object, loggerMock.Object);

            // Act
            await sut.ProcessOutstandingOrders();

            // Assert
            loggerMock.VerifyLog(logger => logger.LogInformation("Order with {orderReference} was paid {at} by {customerEmail}, having {transactionId}",
                It.Is<string>(orderReference => orderReference.StartsWith("Reference")),
                It.IsAny<DateTime>(),
                It.Is<string>(customerEmail => customerEmail.Contains("@")),
                It.Is<string>(transactionId => transactionId.StartsWith("TRX"))),
                Times.Exactly(100));
        }

        [Fact]
        public async Task Processing_outstanding_orders_logs_batch_size()
        {
            // Arrange
            var ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(c => c.GetOutstandingOrders())
                .ReturnsAsync(GenerateOutstandingOrders(100));

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock
                .Setup(c => c.CompletePayment(It.IsAny<Order>()))
                .ReturnsAsync((Order order) => new PaymentTransaction
                {
                    TransactionId = $"TRX-{order.OrderReference}"
                });

            var loggerMock = new Mock<ILogger<PaymentsProcessor>>();

            var sut = new PaymentsProcessor(ordersRepositoryMock.Object, paymentServiceMock.Object, loggerMock.Object);

            // Act
            await sut.ProcessOutstandingOrders();

            // Assert
            loggerMock.VerifyLog(c => c.LogInformation("A batch of 100 outstanding orders was completed"));
        }

        [Fact]
        public async Task Processing_outstanding_orders_logs_a_warning_when_payment_fails()
        {
            // Arrange
            var ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(c => c.GetOutstandingOrders())
                .ReturnsAsync(GenerateOutstandingOrders(2));

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock
                .SetupSequence(c => c.CompletePayment(It.IsAny<Order>()))
                .ReturnsAsync(new PaymentTransaction
                {
                    TransactionId = "TRX-1",
                    CreateOn = DateTime.Now.AddMinutes(-new Random().Next(100)),
                })
                .Throws(new Exception("Payment exception"));

            var loggerMock = new Mock<ILogger<PaymentsProcessor>>();

            var sut = new PaymentsProcessor(ordersRepositoryMock.Object, paymentServiceMock.Object, loggerMock.Object);

            // Act
            await sut.ProcessOutstandingOrders();

            // Assert
            loggerMock.VerifyLog(c => c.LogWarning(It.Is<Exception>(paymentException => paymentException.Message.Contains("Payment exception")), "*exception*Reference 2"));
        }

        private static List<Order> GenerateOutstandingOrders(int count) =>
            Enumerable.Range(1, count).Select(i => new Order
            {
                OrderReference = $"Reference {i}",
                CustomerEmail = $"customer-{i}@mailinator.com"
            }).ToList();
    }
}
