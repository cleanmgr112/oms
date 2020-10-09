using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Purchasings;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
   public class PurchasingMap: MapBase<Purchasing>
    {
        public override Action<EntityTypeBuilder<Purchasing>> BuilderAction { get; }

        public PurchasingMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Purchasing");
            };
        }
    }
}
