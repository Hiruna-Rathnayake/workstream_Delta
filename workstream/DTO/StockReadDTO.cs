using System;

namespace workstream.DTO
{
    public class StockReadDTO
    {
        public int StockId { get; set; }
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }
        public int Quantity { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string BatchNumber { get; set; }
    }
}

