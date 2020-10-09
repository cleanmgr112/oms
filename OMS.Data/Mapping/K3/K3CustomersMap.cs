using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.K3
{
    public class K3CustomersMap : MapBase<Domain.K3Customers>
    {
        public override Action<EntityTypeBuilder<Domain.K3Customers>> BuilderAction { get; }

        public K3CustomersMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("K3Customers");
            };
        }
    }
}
