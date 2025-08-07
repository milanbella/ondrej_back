namespace RetailAppS.Controllers.Model.ShopApi.ShoppingCustomer
{
    public record VerifyScannedEntranceBarcodeRequest(int shopId, string customerId);
    public record VerifyScannedEntranceBarcodeResponse(int shopId, string customerId);

    public record CustomerDetailRequest(int shopId, string customerId);
    public record CustomerDetailRespomse(int shopId, string customerId,  int userId, System.DateTime? createdAt,  System.DateTime? shopEnteredAt, System.DateTime? shopLeftAt);

}
