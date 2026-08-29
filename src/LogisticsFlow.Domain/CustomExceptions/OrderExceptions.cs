using LogisticsFlow.Domain.Enums;

namespace LogisticsFlow.Domain.CustomExceptions;

public class OrderWithInvalidStatusWhenBeginningDispatchException(OrderStatus currentStatus) : Exception(
    $"To begin a dispatch, the order status must be {nameof(OrderStatus.Created)}. Currently it is {currentStatus.ToString()}");

public class OrderWithInvalidStatusWhenDispatchingException(OrderStatus currentStatus) : Exception(
    $"To dispatch, the order status must be {nameof(OrderStatus.Processing)}. Currently it is {currentStatus.ToString()}");

public class OrderWithInvalidStatusWhenCompletingException(OrderStatus currentStatus) : Exception(
    $"To complete, the order status must be {nameof(OrderStatus.Dispatched)}. Currently it is {currentStatus.ToString()}");

public class OrderWithInvalidStatusWhenCancellingException(OrderStatus currentStatus) : Exception(
    $"To cancel, the order status must be one of the following statuses: {nameof(OrderStatus.Processing)}; {nameof(OrderStatus.Created)}. Currently it is {currentStatus.ToString()}");