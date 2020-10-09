using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class SuitProductsMap : MapBase<SuitProducts>
    {
        public override Action<EntityTypeBuilder<SuitProducts>> BuilderAction { get; }

        public SuitProductsMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("SuitProducts");
            };
        }
    }
}
