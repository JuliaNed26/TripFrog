namespace TripFrogWebApi.Services;

public interface IEmailSender
{
    Task SendRegistrationConfirmationEmailAsync(string receiverEmail, string message);
}
