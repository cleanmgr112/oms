using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class ScheduleTaskMap:MapBase<ScheduleTask>
    {
        public override Action<EntityTypeBuilder<ScheduleTask>> BuilderAction { get; }

        public ScheduleTaskMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(t=>t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("ScheduleTask");
            };
        }
    }
}
