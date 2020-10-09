using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class OrderProductMap : MapBase<OrderProduct>
    {
        public override Action<EntityTypeBuilder<OrderProduct>> BuilderAction { get; }

        public OrderProductMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("OrderProduct");
                entry.HasOne(i => i.Order).WithMany(i => i.OrderProduct).HasForeignKey(i => i.OrderId);
                entry.HasOne(i => i.SaleProduct).WithMany(i => i.OrderProduct).HasForeignKey(i => i.SaleProductId);
               
            };
        }
    }
}
