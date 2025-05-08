namespace JO.Models.Responses
{
    public class GetCartItemResponse : BaseResponse
    {
        public CartItem? CartItem { get; set; }
        public bool Success { get; set; }
    }
}
