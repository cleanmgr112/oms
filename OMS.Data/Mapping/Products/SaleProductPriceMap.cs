using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class SaleProductPriceMap : MapBase<SaleProductPrice>
    {
        public override Action<EntityTypeBuilder<SaleProductPrice>> BuilderAction { get; }

        public SaleProductPriceMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SaleProductPrice");
                entry.HasOne(i => i.SaleProduct).WithMany(i => i.SaleProductPrice).HasForeignKey(i => i.SaleProductId);
               };
        }
    }
}
