namespace LogisticsFlow.Application.CustomExceptions;

public class OrderNotFoundException(Guid orderId) : Exception($"Order with  id {orderId} not found.");