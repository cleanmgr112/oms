using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class CustomersMap:MapBase<Customers>
    {
        public override Action<EntityTypeBuilder<Customers>> BuilderAction { get; }
        public CustomersMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("Customer");

                entry.HasOne(i => i.Dictionary).WithMany().HasForeignKey(i => i.CustomerTypeId);
            };
        }
    }
}
