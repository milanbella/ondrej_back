namespace Ondrej.Controllers.Model.Api.ShoppingCustomer
{
    public class Model
    {
        public record NewCustomerRequest();
        public record NewCustomerResponse(string customerId);
        public record CheckEntranceBarcodeScanResultRequest(string customerId);
        public record CheckEntranceBarcodeScanResultResponse(bool doorOpened , int shopId, string customerId);
    }
}
