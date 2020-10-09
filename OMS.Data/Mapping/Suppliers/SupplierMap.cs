using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Suppliers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class SupplierMap:MapBase<Supplier>
    {
        public override Action<EntityTypeBuilder<Supplier>> BuilderAction { get; }
        public SupplierMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Supplier");
            };
        }
    }
}
