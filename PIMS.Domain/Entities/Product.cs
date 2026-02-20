using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIMS.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int LowStockThreshold { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public Inventory? Inventory { get; set; }
        public ICollection<ProductPriceHistory> PriceHistories { get; set; } = new List<ProductPriceHistory>();
    }
}
