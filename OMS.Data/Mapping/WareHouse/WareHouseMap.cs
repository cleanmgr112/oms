using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class WareHouseMap : MapBase<WareHouse>
    {
        public override Action<EntityTypeBuilder<WareHouse>> BuilderAction { get; }

        public WareHouseMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("WareHouse");
            };
        }
    }
}
