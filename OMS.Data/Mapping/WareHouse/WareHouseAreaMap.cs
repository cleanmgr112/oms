using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class WareHouseAreaMap : MapBase<WareHouseArea>
    {
        public override Action<EntityTypeBuilder<WareHouseArea>> BuilderAction { get; }

        public WareHouseAreaMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(x => x.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("WareHouseArea");
            };
        }
    }
}
