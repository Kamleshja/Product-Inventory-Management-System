using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIMS.Domain.Entities
{
    public class InventoryTransaction
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public int QuantityChanged { get; set; }

        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByUserId { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
