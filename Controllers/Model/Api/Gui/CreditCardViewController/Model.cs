namespace RetailAppS.Controllers.Model.Api.Gui.CreditCardViewController
{
    public record SaveCardDetailRequest(string cardHolderName, string cardNumber, string expirationDate, string cvv);
    public record SaveCardDetailResponse(int creditCardId);

    public record GetCardDetailResponse(string cardHolderName, string cardNumber, string expirationDate, string cvv); 

    public record SetCreditCardResponse(int id, string stripeCustomerId); 
}
