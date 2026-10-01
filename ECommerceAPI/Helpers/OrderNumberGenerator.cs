namespace ECommerceAPI.Helpers
{
    public static class OrderNumberGenerator
    {
        // Generates an order number in the following format:
        // ORD-20260909-7A3F91B2
        // ORD      -> Indicates that the value is an order number.
        // 20260909 -> Current UTC date in yyyyMMdd format.
        // 7A3F91B2 -> First 8 characters of a newly generated GUID.
        public static string Generate()
        {
            return
                // Add the "ORD-" prefix and current UTC date.
                // Example: ORD-20260909-
                $"ORD-{DateTime.UtcNow:yyyyMMdd}-" +

                // Generate a new GUID, remove hyphens using "N" format,
                // take the first 8 characters, and convert them to uppercase.
                // Example GUID:
                // 7a3f91b2c6e84f1484c2369b7f2c951d
                // First 8 characters:
                // 7a3f91b2
                // After converting to uppercase:
                // 7A3F91B2
                $"{Guid.NewGuid()
                    .ToString("N")[..8]
                    .ToUpperInvariant()}";
        }
    }
}
