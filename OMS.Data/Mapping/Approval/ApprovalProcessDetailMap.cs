using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class ApprovalProcessDetailMap : MapBase<ApprovalProcessDetail>
    {
        public override Action<EntityTypeBuilder<ApprovalProcessDetail>> BuilderAction { get; }
        public ApprovalProcessDetailMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                entry.ToTable("ApprovalProcessDetail");
                entry.HasOne(i => i.User).WithMany(p => p.ApprovalProcessDetail).HasForeignKey(i => i.UserId);
            };
        }
    }
}
