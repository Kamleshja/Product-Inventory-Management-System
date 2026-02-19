using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIMS.Domain.Entities
{
    public class Inventory
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public string WarehouseLocation { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
