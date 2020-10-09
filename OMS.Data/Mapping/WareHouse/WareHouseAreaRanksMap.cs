using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class WareHouseAreaRanksMap:MapBase<WareHouseAreaRanks>
    {
        public override Action<EntityTypeBuilder<WareHouseAreaRanks>> BuilderAction { get; }

        public WareHouseAreaRanksMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(x => x.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("WareHouseAreaRanks");
                entry.HasOne(i => i.WareHouse).WithMany(i => i.WareHouseAreaRanks).HasForeignKey(i => i.WhId);
            };
        }
    }
}
