using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.K3
{
    public class K3BillNoRelatedMap : MapBase<Domain.K3BillNoRelated>
    {
        public override Action<EntityTypeBuilder<Domain.K3BillNoRelated>> BuilderAction { get; }

        public K3BillNoRelatedMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("K3BillNoRelated");
            };
        }
    }
}
