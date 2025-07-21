namespace PhantomMaskAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
