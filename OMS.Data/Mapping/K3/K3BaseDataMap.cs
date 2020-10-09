using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.K3
{
    public class K3BaseDataMap : MapBase<Domain.K3BaseData>
    {
        public override Action<EntityTypeBuilder<Domain.K3BaseData>> BuilderAction { get; }

        public K3BaseDataMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("K3BaseData");
            };
        }
    }
}
