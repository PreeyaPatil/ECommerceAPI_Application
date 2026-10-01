using ECommerceAPI.Models;
using ECommerceAPI.Options;

namespace ECommerceAPI.Helpers
{
    public static class PricingHelper
    {
        // Calculates the complete pricing details based on
        // the given subtotal and pricing configuration.
        //
        // subtotal represents the total price of the selected items
        // before tax and shipping charges are added.
        public static PricingResult Calculate(decimal subtotal, PricingOptions options)
        {
            // Calculate the tax amount using the configured tax rate.
            // The result is rounded to 2 decimal places because
            // currency values are normally represented with two decimals.
            // MidpointRounding.AwayFromZero ensures values ending in .5
            // are rounded away from zero.
            var taxAmount = Math.Round(subtotal * options.TaxRate, 2, MidpointRounding.AwayFromZero);

            // Determine the shipping charge.
            // If the subtotal is greater than or equal to the configured
            // free-shipping threshold, no shipping charge is applied.
            // Otherwise, the configured flat shipping charge is added.
            var shippingAmount = subtotal >= options.FreeShippingThreshold ? 0 : options.FlatShippingCharge;

            // Create and return the complete pricing result
            // containing subtotal, tax, shipping, grand total, and currency.
            return new PricingResult
            {
                // Original amount before tax and shipping.
                Subtotal = subtotal,

                // Calculated tax amount.
                TaxAmount = taxAmount,

                // Shipping charge based on the free-shipping rule.
                ShippingAmount = shippingAmount,

                // Final amount that the customer has to pay.
                GrandTotal = subtotal + taxAmount + shippingAmount,

                // Currency used by this application.
                Currency = "INR"
            };
        }
    }
}
