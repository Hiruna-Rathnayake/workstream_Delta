namespace workstream.DTO
{
    public class OrderItemReadDTO
    {
        public int OrderItemId { get; set; }
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
