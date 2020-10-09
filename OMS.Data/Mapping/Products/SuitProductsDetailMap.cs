using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class SuitProductsDetailMap : MapBase<SuitProductsDetail>
    {
        public override Action<EntityTypeBuilder<SuitProductsDetail>> BuilderAction { get; }

        public SuitProductsDetailMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SuitProductsDetail");
            };
        }
    }
}
