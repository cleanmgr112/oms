using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class OrderApprovalMap: MapBase<OrderApproval>
    {
        public override Action<EntityTypeBuilder<OrderApproval>> BuilderAction { get; }

        public OrderApprovalMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("OrderApproval");
                entry.HasOne(i => i.User).WithMany(i => i.OrderApproval).HasForeignKey(i => i.UserId);
                entry.HasOne(i => i.Order).WithMany(i => i.OrderApproval).HasForeignKey(i => i.OrderId);
            };
        }
    }
}
