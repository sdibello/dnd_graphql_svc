﻿using System;
using System.Collections.Generic;
using System.Linq;
using dnd_dal.dto;

namespace dnd_dal.query.Lookup
{
    public class LookupQuery
    {
        private readonly dndContext _context;

        public LookupQuery(dndContext context)
        {
            _context = context;
        }

        public List<DndRulebook> Query_dndRuleBooks(List<long> ids)
        {
            var query =
                    from rulebook in _context.DndRulebook
                    where ids.Contains(rulebook.Id)
                    select rulebook;

            return query.ToList();
        }

        public List<DndRulebook> Query_dndRuleBooks()
        {
            var dndContext = new dndContext();

            var query =
                    from rulebook in dndContext.DndRulebook
                    select rulebook;

            return query.ToList();
        }

    }
}