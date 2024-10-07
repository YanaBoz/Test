using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.UI.Data;
using Test.UI.Models;

//автоматически созданный контроллер для доступа к данным таблицы  Oper_Clas
namespace Test.UI.Controllers
{
    public class Oper_ClassController : Controller
    {
        private readonly DBContext _context;

        public Oper_ClassController(DBContext context)
        {
            _context = context;
        }

        // GET: Oper_Class
        public async Task<IActionResult> Index()
        {
            return View(await _context.Oper_Classes.ToListAsync());
        }

        // GET: Oper_Class/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oper_Class = await _context.Oper_Classes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (oper_Class == null)
            {
                return NotFound();
            }

            return View(oper_Class);
        }

        // GET: Oper_Class/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Oper_Class/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Oper_Class oper_Class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(oper_Class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(oper_Class);
        }

        // GET: Oper_Class/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oper_Class = await _context.Oper_Classes.FindAsync(id);
            if (oper_Class == null)
            {
                return NotFound();
            }
            return View(oper_Class);
        }

        // POST: Oper_Class/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Oper_Class oper_Class)
        {
            if (id != oper_Class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oper_Class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Oper_ClassExists(oper_Class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(oper_Class);
        }

        // GET: Oper_Class/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oper_Class = await _context.Oper_Classes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (oper_Class == null)
            {
                return NotFound();
            }

            return View(oper_Class);
        }

        // POST: Oper_Class/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oper_Class = await _context.Oper_Classes.FindAsync(id);
            if (oper_Class != null)
            {
                _context.Oper_Classes.Remove(oper_Class);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Oper_ClassExists(int id)
        {
            return _context.Oper_Classes.Any(e => e.Id == id);
        }
    }
}
