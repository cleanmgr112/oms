using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class OrderPayPriceMap : MapBase<OrderPayPrice>
    {
        public override Action<EntityTypeBuilder<OrderPayPrice>> BuilderAction { get; }

        public OrderPayPriceMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("OrderPayPrice");
                entry.HasOne(i => i.Order).WithMany(i => i.OrderPayPrice).HasForeignKey(i => i.OrderId);
            };
        }
    }
}
