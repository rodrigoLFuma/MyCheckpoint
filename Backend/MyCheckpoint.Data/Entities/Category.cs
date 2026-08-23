using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<GameCategory> GameCategories { get; set; }
    }
}
