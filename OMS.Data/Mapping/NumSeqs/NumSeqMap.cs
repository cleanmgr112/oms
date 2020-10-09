using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class NumSeqMap:MapBase<NumSeq>
    {
        public override Action<EntityTypeBuilder<NumSeq>> BuilderAction { get; }
        public NumSeqMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);


                // Properties
                // Table & Column Mappings
                entry.ToTable("NumSeq");
                //entry.HasOne(i => i.Dictionary).WithMany().HasForeignKey(i => i.CategoryId);
            };
        }
    }
}
