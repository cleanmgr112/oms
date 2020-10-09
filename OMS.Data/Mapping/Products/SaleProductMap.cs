using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class SaleProductMap : MapBase<SaleProduct>
    {
        public override Action<EntityTypeBuilder<SaleProduct>> BuilderAction { get; }

        public SaleProductMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SaleProduct");
                entry.HasOne(i => i.Product).WithMany(i => i.SaleProduct).HasForeignKey(i => i.ProductId);
              };
        }
    }
}
