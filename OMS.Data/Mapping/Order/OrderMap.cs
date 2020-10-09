using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class OrderMap : MapBase<Order>
    {
        public override Action<EntityTypeBuilder<Order>> BuilderAction { get; }

        public OrderMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Order");
                entry.HasOne(i => i.Shop).WithMany(i => i.Order).HasForeignKey(i => i.ShopId);
                entry.HasOne(i => i.Delivery).WithMany(i => i.Order).HasForeignKey(i => i.DeliveryTypeId);
                entry.HasOne(i => i.WareHouse).WithMany(i => i.Order).HasForeignKey(i => i.WarehouseId);
            };
        }
    }
}
