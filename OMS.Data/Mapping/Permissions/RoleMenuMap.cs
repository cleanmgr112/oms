using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Permissions
{
    public class RoleMenuMap:MapBase<RoleMenu>
    {
        public override Action<EntityTypeBuilder<RoleMenu>> BuilderAction { get; }
        public RoleMenuMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);

                // Properties
                // Table & Column Mappings
                entry.ToTable("RoleMenu");
            };
        }
    }
}
