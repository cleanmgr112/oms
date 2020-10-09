using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class ShopMap : MapBase<Shop>
    {
        public override Action<EntityTypeBuilder<Shop>> BuilderAction { get; }

        public ShopMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Shop");
            };
        }
    }
}
