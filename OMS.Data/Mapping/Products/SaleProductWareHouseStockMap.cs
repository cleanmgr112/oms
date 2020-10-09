using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Products
{
    public class SaleProductWareHouseStockMap:MapBase<SaleProductWareHouseStock>
    {
        public override Action<EntityTypeBuilder<SaleProductWareHouseStock>> BuilderAction { get; }

        public SaleProductWareHouseStockMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SaleProductWareHouseStock");
            };
        }
    }
}
