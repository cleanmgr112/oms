using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace OMS.Data.Mapping
{
    public abstract class MapBase<TEntity> where TEntity : class
    {
        public abstract Action<EntityTypeBuilder<TEntity>> BuilderAction { get; }

        public virtual void Map(ModelBuilder modelBuilder)
        {
            if (BuilderAction != null)
            {
                modelBuilder.Entity(BuilderAction);
            }
        }
    }
}
