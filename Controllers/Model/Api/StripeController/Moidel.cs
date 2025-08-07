namespace RetailAppS.Controllers.Model.Api.StripeController
{
    public record NewCustomerResponse(int id, string stripeCustomerId); 

    public record GetEphemeralKeyRequest(string stripeCustomerId);
    public record GetEphemeralKeyResponse(string id, string secret, DateTime expires);

    public record CreateSetupIntentRequest(string stripeCustomerId);
    public record CreateSetupIntentResponse(int id, string stripeSetupIntentId, string stripeClientSecret); 

    public record CreatePaymentIntentRequest(string stripeCustomerId, float amount, string currency);
    public record CreatePaymentIntentResponse(int id, string stripePaymentIntentId, string stripeClientSecret);

}
