using OMS.Data.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Dictionaries
{
    public class DictionaryModel
    {
        public int Id { get; set; }
        public DictionaryType Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public string IsSyncToWMS { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
