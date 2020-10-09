using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class DeliveryMap : MapBase<Delivery>
    {
        public override Action<EntityTypeBuilder<Delivery>> BuilderAction { get; }

        public DeliveryMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Delivery");
            };
        }
    }
}
