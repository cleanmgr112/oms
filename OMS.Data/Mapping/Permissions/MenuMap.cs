using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Permissions
{
    public class MenuMap:MapBase<Menu>
    {
        public override Action<EntityTypeBuilder<Menu>> BuilderAction { get; }

        public MenuMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);

                // Properties
                // Table & Column Mappings
                entry.ToTable("Menu");
            };
        }

    }
}
