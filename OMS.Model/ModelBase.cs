using System.Collections.Generic;

namespace OMS.Model
{
    public abstract class ModelBase
    {
        public ModelBase()
        {
            CustomProperties = new Dictionary<string, object>();
        }

        public int Id { get; set; }

        public IDictionary<string, object> CustomProperties { get; set; }
    }
}
