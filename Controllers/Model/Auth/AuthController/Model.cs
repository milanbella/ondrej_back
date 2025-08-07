namespace RetailAppS.Controllers.Model.Auth.AuthController
{
    public record BrowserLoginRequest(string username, string password);
    public record BrowserLoginResponse(string error, string message);

    public record DeviceLoginRequest(string deviceIdentification,string userEmail, string password);
    public record DeviceLoginResponse(string error, string message, string jwt);

    public record DeviceRegisterRequest(string deviceIdentification,string email, string password, string firstName, string lastName);
    public record DeviceRegisterResponse(string error, string message, string jwt);

    public record GetUserResponse(bool isLoggedIn, string username, string userEmail, string firstName, string lastName);
}
