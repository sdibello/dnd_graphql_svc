﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using dnd_dal.dto;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using System.Security.Cryptography;
using Lucene.Net.Index;
using System.Web;

namespace dnd_dal.query.spell
{
    public class SpellQuery
    {
        private readonly dndContext _context;

        public SpellQuery(dndContext context)
        {
            _context = context;
        }

        #region SpellsByClassAndLevel 

        private List<SpellClassLevel> Query_SpellsByClassAndLevel(string CasterClass, long CasterLevel)
        {
            var query =
                    from cc in _context.DndCharacterclass
                    join scl in _context.DndSpellclasslevel.DefaultIfEmpty() on cc.Id equals scl.CharacterClassId into spell_class_level
                    from spellcl in spell_class_level.DefaultIfEmpty()
                    join s in _context.DndSpell.DefaultIfEmpty() on spellcl.SpellId equals s.Id into sp
                    from spell in sp.DefaultIfEmpty()
                    where cc.Slug.ToLower() == CasterClass.ToLower()
                    where spellcl.Level == CasterLevel
                    select new SpellClassLevel
                    {
                        SpellId = spell.Id,
                        ClassId = spellcl.CharacterClassId,
                        Level = spellcl.Level,
                        ClassName = cc.Name,
                        SpellName = spell.Name
                    };


            return query.ToList();
        }

        private List<SpellClassLevel> Query_ClassAndLevelBySpell(long SpellId)
        {
            var query =
                    from spellclasslevel in _context.DndSpellclasslevel
                    join cc in _context.DndCharacterclass.DefaultIfEmpty() on spellclasslevel.CharacterClassId equals cc.Id into char_class
                    from characterclass in char_class.DefaultIfEmpty()
                    where spellclasslevel.SpellId == SpellId
                    select new SpellClassLevel
                    {
                        SpellId = spellclasslevel.SpellId,
                        ClassId = spellclasslevel.CharacterClassId,
                        Level = spellclasslevel.Level,
                        ClassName = characterclass.Name
                    };

            return query.ToList();
        }

        public List<SpellClassLevel> ClassAndLevelBySpell(long spellId)
        {
            Console.WriteLine(string.Format("log - ClassAndLevelBySpell - PARAMS {0}", spellId.ToString()));

            var query = Query_ClassAndLevelBySpell(spellId);

            if (query != null) {
                Console.WriteLine(string.Format("log - ClassAndLevelBySpell - ByClassAndLevel results {0}", query.Count()));
                return query;
            };

            Console.WriteLine(string.Format("log - ClassAndLevelBySpell - Not Spells Found"));
            return null;
        }

        /// <summary>
        ///   Pull a list of "SpellClassLewvels items from the database, which lists all spells by class level.
        /// </summary>
        /// <param name="context">Database Concept</param>
        /// <param name="CasterClass">string value of the character class</param>
        /// <param name="CasterLevel">string value of the character level</param>
        /// <returns>A list of SpellClassLevels </returns>
        public List<SpellClassLevel> SpellsByClassAndLevel(string CasterClass, string CasterLevel)
        {
            long llevel;

            Console.WriteLine(string.Format("log - SpellQuery - SpellsByClassAndLevel - ByClassAndLevel PARAMS {0} {1}", CasterClass, CasterLevel));

            if (long.TryParse(CasterLevel, out llevel))
            {
                var query = Query_SpellsByClassAndLevel(CasterClass, llevel);

                if (query != null)
                {
                    Console.WriteLine(string.Format("log - SpellQuery - SpellsByClassAndLevel - ByClassAndLevel - ByClassAndLevel results {0}", query.Count()));
                    return query;
                };
            }

            Console.WriteLine(string.Format("log - SpellQuery - SpellsByClassAndLevel - ByClassAndLevel - Not Spells Found", CasterClass, CasterLevel));
            return null;
        }

        #endregion

        #region Spell School

        /// <summary>
        /// Returns a list of spell schools for a spell, given the spell slug
        /// </summary>
        /// <param name="slug">slug of a spell</param>
        /// <returns>a list of SpellSchoolSubShool objects</returns>
        public List<DndSpellschool> Query_schoolsBySlug(string slug)
        {
            //Couldn't get these to work right in 1 query, tried many approaches.
            // spell.schoolId is a long, and not nullable, so no null checked needed here.
            var SchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Slug.ToLower() == slug.ToLower()
                select spellschool;

            //subschoolId is nullable, so need to check for nulls, with the where, we get an empty set, which is better then a null value in a list.
            var SubSchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SubSchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Slug.ToLower() == slug.ToLower()
                where spell.SubSchoolId != null
                select spellschool;

            // combine the two queries.
            var returnList = SchoolQuery.Concat(SubSchoolQuery);

            return returnList.ToList();
        }

        public List<DndSpellschool> Query_schoolsByName(string Name)
        {
            //Couldn't get these to work right in 1 query, tried many approaches.
            // spell.schoolId is a long, and not nullable, so no null checked needed here.
            var SchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Name.ToLower() == Name.ToLower()
                select spellschool;

            //subschoolId is nullable, so need to check for nulls, with the where, we get an empty set, which is better then a null value in a list.
            var SubSchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SubSchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Name.ToLower() == Name.ToLower()
                where spell.SubSchoolId != null
                select spellschool;

            // combine the two queries.
            var returnList = SchoolQuery.Concat(SubSchoolQuery);

            return returnList.ToList();
        }

        /// <summary>
        /// Retruns a list of spell schools by spell ID
        /// </summary>
        /// <param name="spellId">the LONG spell ID</param>
        /// <returns>a list of SpellSchooLSubSchool</returns>
        public List<DndSpellschool> Query_schoolsById(long spellId)
        {
            //Couldn't get these to work right in 1 query, tried many approaches.
            // spell.schoolId is a long, and not nullable, so no null checked needed here.
            var SchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Id == spellId
                select spellschool;

            //subschoolId is nullable, so need to check for nulls, with the where, we get an empty set, which is better then a null value in a list.
            var SubSchoolQuery =
                from spell in _context.DndSpell
                join school in _context.DndSpellschool on spell.SubSchoolId equals school.Id into s
                from spellschool in s.DefaultIfEmpty()
                where spell.Id == spellId
                where spell.SubSchoolId != null
                select spellschool;

            // combine the two queries.
            var returnList = SchoolQuery.Concat(SubSchoolQuery);

            return returnList.ToList();
        }

        #endregion
    }
}
