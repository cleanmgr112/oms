using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class InvoiceInfoMap : MapBase<InvoiceInfo>
    {
        public override Action<EntityTypeBuilder<InvoiceInfo>> BuilderAction
        {
            get;
        }
        public InvoiceInfoMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("InvoiceInfo");
                //一对一关系
                entry.HasOne(i => i.Order).WithOne(i => i.InvoiceInfo).HasForeignKey<InvoiceInfo>(i=>i.OrderId);
            };
        }
    }
}
