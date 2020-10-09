using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Purchasings
{
    public class PurchasingProductsMap:MapBase<PurchasingProducts>
    {
        public override Action<EntityTypeBuilder<PurchasingProducts>> BuilderAction { get; }

        public PurchasingProductsMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("PurchasingProducts");
            };
        }
    }
}
