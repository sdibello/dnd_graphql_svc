﻿using System;
using System.Collections.Generic;

namespace dnd_dal
{
    public partial class DndItemproperty
    {
        public DndItemproperty()
        {
            DndItem = new HashSet<DndItem>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }

        public virtual ICollection<DndItem> DndItem { get; set; }
    }
}
