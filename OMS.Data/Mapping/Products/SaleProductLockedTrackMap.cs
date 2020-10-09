using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class SaleProductLockedTrackMap : MapBase<SaleProductLockedTrack>
    {
        public override Action<EntityTypeBuilder<SaleProductLockedTrack>> BuilderAction { get; }

        public SaleProductLockedTrackMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("SaleProductLockedTrack");
                
            };
        }
    }
}
