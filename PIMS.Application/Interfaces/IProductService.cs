using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIMS.Application.DTOs.Product;

namespace PIMS.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto);
        Task<List<ProductResponseDto>> GetAllProductsAsync();
        Task<List<ProductResponseDto>> GetProductsAsync(ProductQueryDto query);

    }
}
