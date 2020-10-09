using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping.Approval
{
   public class ApprovalProcessMap:MapBase<ApprovalProcess>
    {
        public override Action<EntityTypeBuilder<ApprovalProcess>> BuilderAction { get; }
        public ApprovalProcessMap() {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                entry.ToTable("ApprovalProcess");
                //Relationships
                entry.HasMany(i => i.ApprovalProcessDetail).WithOne(p=>p.ApprovalProcess).HasForeignKey(i => i.ApprovalProcessId);
            };
        }
    }
}
