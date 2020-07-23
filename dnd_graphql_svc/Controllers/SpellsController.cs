﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Directory = Lucene.Net.Store.Directory;
using Lucene.Net.QueryParsers.Classic;
using System.IO;
using dnd_graphql_svc.Search;
using AutoMapper;
using dnd_service_logic.dto;
using dnd_service_logic;
using dnd_dal;

namespace dnd_graphql_svc.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class SpellsController : Controller
    {
        private readonly dndContext _context;
        public Search.Search _find;

        public IActionResult Index()
        {
            return View();
        }

        public SpellsController(dndContext context)
        {
            _context = context;
            _find = new Search.Search();
        }

        [HttpGet("{id}/class")]
        public async Task<ActionResult<List<SpellClassLevel>>> GetSpellClassLevel(long id)
        {
            //var query = new SpellQuery(_context);
            //List<SpellClassLevel> data = query.ClassAndLevelBySpell(id);

            //if (data != null)
            //{
            //    Console.WriteLine(string.Format("log - get spell class - id = {0}", id));  
            //    return data;
            //};

            //Console.WriteLine(string.Format("log - get spell class - id = {0}", id));
            return NotFound();
        }

        // Scaffold-DbContext "DataSource=D:\git\dnd_dal\dnd_dal\DataAccess\dnd.sqlite" Microsoft.EntityFra meworkCore.Sqlite
        // GET: api/Spells/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Spell>> GetSpell(string id)
        {
            int intId;
            DndSpell spelldb;
            Spell spell = new Spell();
            List<SpellSearch> searchresults;

            if (int.TryParse(id, out intId) == true)
            {
                spelldb = _context.DndSpell.Where(x => x.Id == intId).FirstOrDefault();
            } else {
                if (id.IndexOf(' ') > 0) {
                    spelldb = _context.DndSpell.Where(x => x.Name.ToLower() == HttpUtility.UrlDecode(id.ToLower())).FirstOrDefault();
                } else {
                    spelldb = _context.DndSpell.Where(x => x.Slug.ToLower() == id.ToLower()).FirstOrDefault();
                }
            }

            if (spelldb!= null)
            {
                //replace with automapper.
                spell.Id = spelldb.Id;
                spell.Description = spelldb.Description;
                spell.Name = spelldb.Name;
                spell.CastingTime = spelldb.CastingTime;
                spell.Range = spelldb.Range;
                spell.SavingThrow = spelldb.SavingThrow;
                spell.SpellResistance = spelldb.SpellResistance;
                spell.Duration = spelldb.Duration;
                spell.Target = spelldb.Target;
                spell.Slug = spelldb.Slug;
                spell.SubSchoolId = spelldb.SubSchoolId;
                spell.SchoolId = spelldb.SchoolId;
                spell.ArcaneFocusComponent = spelldb.ArcaneFocusComponent;
                spell.DivineFocusComponent = spelldb.DivineFocusComponent;
                spell.MaterialComponent = spelldb.MaterialComponent;
                spell.SomaticComponent = spelldb.SomaticComponent;
                spell.VerbalComponent = spelldb.VerbalComponent;
                spell.XpComponent = spelldb.XpComponent;
            }

            //var school = Query.SchoolBySpell(id);

            if (spell.Id > 0)
            {
                Console.WriteLine(string.Format("log - get spell - ({0}) name = {1}", id, spelldb.Name));
                return spell;
            };

            if (int.TryParse(id, out intId) == false) {
            //    searchresults = _find.WildcardSearch(id);
            //    spell.search = searchresults;
            }

            Console.WriteLine(string.Format("log - get spell - ({0}) - 404, not found", id));
            return spell;
        }

        [HttpGet("{id}/school")]
        public ActionResult<List<SpellSchoolSubSchool>> School(string id)
        {
            List<SpellSchoolSubSchool> results;
            var logic = new dnd_service_logic.BL.SpellLogic(_context);

            results = logic.getSchools(id);
 
            if (results != null)
            {
                Console.WriteLine(string.Format("log - searchSpellByClassAndLevel - Schools - returned {0} results", results.Count()));
                return results;
            }

            Console.WriteLine(string.Format("log - searchSpellByClassAndLevel - Schools - 404 Not found"));
            return NotFound();
        }

        [HttpGet("{id}/index")]
        public async Task<ActionResult<DndSpell>> updateIndex()
        {
            var AppLuceneVersion = Lucene.Net.Util.LuceneVersion.LUCENE_48;

            var indexLocation = @"C:\Index";
            var dir = FSDirectory.Open(indexLocation);

            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            var indexconfig = new Lucene.Net.Index.IndexWriterConfig(AppLuceneVersion, analyzer);
            var write = new IndexWriter(dir, indexconfig);

            var data = _context.DndSpell.ToList();

            foreach (var spell in data)
            {
                Document doc = new Document
                {
                    new StringField("name", spell.Name,Field.Store.YES),
                    new StringField("search_name", spell.Name.ToLower(),Field.Store.YES),
                    new StringField("id", spell.Id.ToString(),Field.Store.YES),
                    //new StringField("desc", spell.Description.ToLower(),Field.Store.YES),
                    new StringField("slug", spell.Slug.ToLower(),Field.Store.YES),
                };

                Console.WriteLine(string.Format(" writing {0}", spell.Name));
                write.AddDocument(doc);
                write.Flush(triggerMerge: false, applyAllDeletes: false);
                write.Commit();
            }

            return NotFound();
        }

        [HttpGet("{casterClass}/{casterlevel}")]
        public async Task<ActionResult<List<SpellClassLevel>>> searchSpellByClassAndLevel(String casterClass, string casterlevel)
        {

            //var query = new SpellQuery(_context);
            //var results = query.SpellsByClassAndLevel(casterClass, casterlevel);

            //if (results != null)
            //{
            //    if (results.Count != 0)
            //    {
            //        Console.WriteLine(string.Format("log - searchSpellByClassAndLevel - SpellController = {0}/{1} - returned {2} results", casterClass, casterlevel, results.Count()));
            //        return results;
            //    }
            //    else
            //    {
            //        Console.WriteLine(string.Format("log - searchSpellByClassAndLevel - SpellController - NO RESULTS", casterClass, casterlevel));
            //        return NotFound();
            //    }
            //};

            //Console.WriteLine(string.Format("log - searchSpellByClassAndLevel - SpellController = {0}/{1} - NOT FOUND", casterClass, casterlevel));
            return NotFound();
        }


    }
}