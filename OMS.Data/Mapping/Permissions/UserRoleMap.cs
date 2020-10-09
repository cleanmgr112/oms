using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Permissions
{
    public class UserRoleMap:MapBase<UserRole>
    {
        public override Action<EntityTypeBuilder<UserRole>> BuilderAction { get; }
        public UserRoleMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);

                // Properties
                // Table & Column Mappings
                entry.ToTable("UserRole");
            };
        }

    }
}
