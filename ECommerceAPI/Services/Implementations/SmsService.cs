using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class SmsService : ISmsService
    {
        // Stores the Twilio configuration values such as
        // Account SID, Auth Token, and sender phone number.
        private readonly TwilioOptions _twilioOptions;

        // Represents the Twilio client used to communicate with the Twilio REST API.
        private readonly ITwilioRestClient _twilioClient;

        // IOptions<TwilioOptions> provides the strongly typed Twilio
        // configuration registered through ASP.NET Core Dependency Injection.
        public SmsService(IOptions<TwilioOptions> twilioOptions)
        {
            // Extract the actual TwilioOptions object from IOptions<T>.
            _twilioOptions = twilioOptions.Value;

            // Create the Twilio REST client using the Account SID
            // and Auth Token provided in the application configuration.
            _twilioClient = new TwilioRestClient(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
        }

        // Sends an SMS message asynchronously.
        // toPhoneNumber : Recipient's mobile number including country code.
        //                 Example: +919876543210
        // message       : The text message that should be sent.
        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            // Remove any leading or trailing spaces from the recipient's phone number.
            toPhoneNumber = toPhoneNumber.Trim();

            // Twilio expects phone numbers in international format.
            // If the '+' symbol is missing, add it automatically.
            // Example: 919876543210
            // becomes +919876543210
            if (!toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+" + toPhoneNumber;
            }

            // Create the SMS message configuration.
            // CreateMessageOptions specifies:
            // 1. To   - Recipient phone number
            // 2. From - Twilio sender phone number
            // 3. Body - SMS message content
            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                // The SMS-enabled Twilio phone number from which the message will be sent.
                From = new PhoneNumber(_twilioOptions.FromPhoneNumber),

                // The actual text content of the SMS.
                Body = message
            };

            // Send the SMS through the Twilio REST API.
            // Twilio processes the request and attempts
            // to deliver the SMS to the recipient.
            await MessageResource.CreateAsync(messageOptions, _twilioClient);
        }
    }
}
