using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Data.Domain
{
    [Serializable]
    public abstract partial class EntityBase
    {
        public EntityBase()
        {
            Isvalid = true;
            CreatedTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public  int Id { get; set; }

        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime ModifiedTime { get; set; }

        public bool Isvalid { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
