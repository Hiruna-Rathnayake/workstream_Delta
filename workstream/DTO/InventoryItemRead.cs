using System.Collections.Generic;

namespace workstream.DTO
{
    public class InventoryItemReadDTO
    {
        public int InventoryItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int TenantId { get; set; } // Add TenantId if needed

        // Change this to StockReadDTO to return necessary stock info
        public ICollection<StockReadDTO> Stocks { get; set; } // Navigation property to Stocks (DTO)
    }
}
