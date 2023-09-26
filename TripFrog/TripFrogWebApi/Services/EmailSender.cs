using SendGrid.Helpers.Mail;
using SendGrid;

namespace TripFrogWebApi.Services;

public class EmailSender : IEmailSender
{
    private const string SenderEmail = "yourTripFrog@gmail.com";
    private const string SenderName = "Trip Frog";
    private readonly string _apiKey;

    public EmailSender(string apiKey)
    {
        _apiKey = apiKey;
    }
    
    public async Task SendRegistrationConfirmationEmailAsync(string receiverEmail, string message)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(SenderEmail, SenderName);
        var subject = "Registration confirmation";
        var to = new EmailAddress(receiverEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, message, string.Empty);
        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Message was not delivered");
        }
    }
}
