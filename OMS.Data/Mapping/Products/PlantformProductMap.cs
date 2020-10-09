using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class PlantformProductMap:MapBase<PlatformProduct>
    {
        public override Action<EntityTypeBuilder<PlatformProduct>> BuilderAction { get; }
        public PlantformProductMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("PlatformProduct");
            };
        }
    }
}
