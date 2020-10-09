using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Permissions
{
    public class RolePermissionMap:MapBase<RolePermission>
    {
        public override Action<EntityTypeBuilder<RolePermission>> BuilderAction { get; }
        public RolePermissionMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);

                // Properties
                // Table & Column Mappings
                entry.ToTable("RolePermission");
            };
        }
    }
}
