using OMS.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace OMS.Data.Mapping
{
    public class UserMap : MapBase<User>
    {
        public override Action<EntityTypeBuilder<User>> BuilderAction { get; }

        public UserMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);



                // Properties
                // Table & Column Mappings
                entry.ToTable("User");
            };
        }
    }
}
