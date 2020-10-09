using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class CMBOrderLogMap : MapBase<CMBOrderLog>
    {
        public override Action<EntityTypeBuilder<CMBOrderLog>> BuilderAction { get; }

        public CMBOrderLogMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("CMBOrderLog");
            };
        }
    }
}
