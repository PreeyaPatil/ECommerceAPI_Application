using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    // Derived Controllers will automatically get API Controller behavior
    // such as automatic model validation and parameter binding.
    [ApiController]
    public abstract class AuthenticatedControllerBase : ControllerBase
    {
        // Extract the authenticated Customer Id from the JWT NameIdentifier claim.
        protected int GetCustomerId()
        {
            // Read the NameIdentifier claim from the authenticated User.
            // This claim should contain the Customer Id that was added
            // when the JWT Access Token was generated.
            var customerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Try to convert the claim value into an integer Customer Id.
            // If the claim is missing or contains an invalid value,
            // the token cannot be trusted for Customer identification.
            if (!int.TryParse(customerIdValue, out var customerId))
            {
                throw new UnauthorizedAccessException("Invalid authentication token.");
            }

            // Return the authenticated Customer Id, so derived Controllers can use it.
            return customerId;
        }
    }
}
