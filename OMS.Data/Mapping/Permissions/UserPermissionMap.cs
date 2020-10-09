using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Permissions
{
    public class UserPermissionMap:MapBase<UserPermission>
    {
        public override Action<EntityTypeBuilder<UserPermission>> BuilderAction { get; }
        public UserPermissionMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);

                // Properties
                // Table & Column Mappings
                entry.ToTable("UserPermission");
            };
        }

    }
}
