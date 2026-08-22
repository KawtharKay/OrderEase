namespace Domain.Enums
{
    public static class OrderStatusTransitions
    {
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            { OrderStatus.Received,       new[] { OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.Processing,     new[] { OrderStatus.Dispatched, OrderStatus.ReadyForPickup, OrderStatus.Cancelled } },
            { OrderStatus.Dispatched,     new[] { OrderStatus.Delivered } },
            { OrderStatus.ReadyForPickup, new[] { OrderStatus.Delivered } },
            { OrderStatus.Delivered,      new[] { OrderStatus.Returned } },
            { OrderStatus.Cancelled,      Array.Empty<OrderStatus>() },
            { OrderStatus.Returned,       Array.Empty<OrderStatus>() },
        };

        public static bool IsValidTransition(OrderStatus from, OrderStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        public static IReadOnlyCollection<OrderStatus> GetAllowedNextStatuses(OrderStatus from)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) ? allowed : Array.Empty<OrderStatus>();
        }
    }
}
