using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Data.Mapping
{
    public class DictionaryMap:MapBase<Domain.Dictionary>
    {
        public override Action<EntityTypeBuilder<Domain.Dictionary>> BuilderAction { get; }

        public DictionaryMap()
        {
            BuilderAction = entry =>
            {
                entry.HasKey(t => t.Id);
                // Properties
                // Table & Column Mappings
                entry.ToTable("Dictionary");
            };
        }
    }
}
