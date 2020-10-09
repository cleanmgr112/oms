using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;

namespace OMS.Data.Mapping
{
    public class CategoryMap : MapBase<Category>
    {
        public override Action<EntityTypeBuilder<Category>> BuilderAction { get; }

        public CategoryMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("Category");
            };
        }
    }
}
