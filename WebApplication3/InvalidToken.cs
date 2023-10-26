namespace WebApplication3
{
    public class InvalidToken
    {
        public int Id { get; set; }
        public string TokenId { get; set; } // уникальный идентификатор токена (jti claim, если он есть в вашем JWT)
        public DateTime ExpiryDate { get; set; }
    }
}
