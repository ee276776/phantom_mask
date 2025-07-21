namespace PhantomMaskAPI.Models.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CashBalance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
