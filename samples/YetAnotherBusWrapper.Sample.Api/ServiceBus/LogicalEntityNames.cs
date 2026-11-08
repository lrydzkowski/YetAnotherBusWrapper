namespace YetAnotherBusWrapper.Sample.Api.ServiceBus;

internal static class LogicalEntityNames
{
    public static class Queues
    {
        public const string Orders = "orders";

        public const string Payments = "payments";

        public const string Shipments = "shipments";

        public const string Reports = "reports";

        public const string Audit = "audit";
    }

    public static class Topics
    {
        public const string Prices = "prices";

        public const string CacheInvalidation = "cache-invalidation";

        public const string Notifications = "notifications";
    }

    public static class Subscriptions
    {
        public const string PriceCache = "cache";

        public const string PriceSearch = "search";

        public const string LocalCache = "local-cache";

        public const string Email = "email";
    }
}
