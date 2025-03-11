using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using workstream.Model;

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int InventoryItemId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [NotMapped]
    public decimal TotalPrice => Price * Quantity;

    public Order Order { get; set; }

    public InventoryItem InventoryItem { get; set; }

    [NotMapped]
    public string InventoryItemName => InventoryItem?.Name ?? "Unknown";
}
