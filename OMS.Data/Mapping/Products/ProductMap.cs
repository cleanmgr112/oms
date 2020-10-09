using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class ProductMap:MapBase<Product>
    {
        public override Action<EntityTypeBuilder<Product>> BuilderAction { get; }

        public ProductMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("Product");
                entry.HasOne(i => i.Dictionary).WithMany().HasForeignKey(i => i.CategoryId);
            };
        }
    }
}
