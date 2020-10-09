using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.SalesMans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class SalesManMap: MapBase<SalesMan>
    {
        public override Action<EntityTypeBuilder<SalesMan>> BuilderAction { get; }

        public SalesManMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SalesMan");
            };
        }
    }
}
