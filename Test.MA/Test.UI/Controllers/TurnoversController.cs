using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Test.UI.Data;
using Test.UI.Models;

//автоматически созданный контроллер для доступа к данным таблицы Turnover
namespace Test.UI.Controllers
{
    public class TurnoversController : Controller
    {
        private readonly DBContext _context;

        public TurnoversController(DBContext context)
        {
            _context = context;
        }

        // GET: Turnovers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Turnover.ToListAsync());
        }

        // GET: Turnovers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turnover = await _context.Turnover
                .FirstOrDefaultAsync(m => m.Id == id);
            if (turnover == null)
            {
                return NotFound();
            }

            return View(turnover);
        }

        // GET: Turnovers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Turnovers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Group_ID,Class_ID,Account,Start_Active,Start_Passive,Turn_Debit,Turn_Credit")] Turnover turnover)
        {
            if (ModelState.IsValid)
            {
                _context.Add(turnover);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(turnover);
        }

        // GET: Turnovers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turnover = await _context.Turnover.FindAsync(id);
            if (turnover == null)
            {
                return NotFound();
            }
            return View(turnover);
        }

        // POST: Turnovers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Group_ID,Class_ID,Account,Start_Active,Start_Passive,Turn_Debit,Turn_Credit")] Turnover turnover)
        {
            if (id != turnover.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(turnover);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TurnoverExists(turnover.Id))
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
            return View(turnover);
        }

        // GET: Turnovers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var turnover = await _context.Turnover
                .FirstOrDefaultAsync(m => m.Id == id);
            if (turnover == null)
            {
                return NotFound();
            }

            return View(turnover);
        }

        // POST: Turnovers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var turnover = await _context.Turnover.FindAsync(id);
            if (turnover != null)
            {
                _context.Turnover.Remove(turnover);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TurnoverExists(int id)
        {
            return _context.Turnover.Any(e => e.Id == id);
        }
    }
}
