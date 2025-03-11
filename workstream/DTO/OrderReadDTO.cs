using System;
using System.Collections.Generic;

namespace workstream.DTO
{
    public class OrderReadDTO
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public int TenantId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<OrderItemReadDTO> OrderItems { get; set; } = new List<OrderItemReadDTO>();
    }
}
