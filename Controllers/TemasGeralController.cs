﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EFS_23298_23327.Data;
using EFS_23298_23327.Models;
using EFS_23298_23327.ViewModel;
using System.Security.Claims;

namespace EFS_23298_23327.Controllers
{
    public class TemasGeralController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TemasGeralController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TemasGeral
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Temas.Include(m => m.ListaFotos.Where(f => f.Deleted != true)).Where(m => m.Deleted != true).OrderByDescending(m => m.DataCriacao);
            ICollection<TemasFotoViewModel> TmVm = new List<TemasFotoViewModel>();
            var lista = await applicationDbContext.ToListAsync();
            if (lista != null)
            {
                if (lista.Any())
                {
                    foreach (var tema in lista)
                    {
                        if (tema.ListaFotos.Any())
                        {
                            foreach (var foto in tema.ListaFotos)
                            {
                                TmVm.Add(new TemasFotoViewModel(tema.Nome, foto.Nome));
                            }
                        }

                    }
                }
            }
            return View(lista);
        }

        // GET: TemasGeral/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temas = await _context.Temas
                .Include(t => t.Sala)
                .FirstOrDefaultAsync(m => m.TemaId == id);
            if (temas == null)
            {
                return NotFound();
            }

            return View(temas);
        }

        // GET: TemasGeral/Create
        public IActionResult Create()
        {
            ViewData["SalaID"] = new SelectList(_context.Salas, "SalaId", "SalaId");
            return View();
        }

        // POST: TemasGeral/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TemaId,Nome,Descricao,TempoEstimado,MinPessoas,MaxPessoas,Dificuldade,SalaID,DataCriacao,Deleted,CriadoPorOid,CriadoPorUsername")] Temas temas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(temas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalaID"] = new SelectList(_context.Salas, "SalaId", "SalaId", temas.SalaID);
            return View(temas);
        }

        // GET: TemasGeral/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temas = await _context.Temas.FindAsync(id);
            if (temas == null)
            {
                return NotFound();
            }
            ViewData["SalaID"] = new SelectList(_context.Salas, "SalaId", "SalaId", temas.SalaID);
            return View(temas);
        }

        // POST: TemasGeral/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TemaId,Nome,Descricao,TempoEstimado,MinPessoas,MaxPessoas,Dificuldade,SalaID,DataCriacao,Deleted,CriadoPorOid,CriadoPorUsername")] Temas temas)
        {
            if (id != temas.TemaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(temas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TemasExists(temas.TemaId))
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
            ViewData["SalaID"] = new SelectList(_context.Salas, "SalaId", "SalaId", temas.SalaID);
            return View(temas);
        }

        // GET: TemasGeral/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var temas = await _context.Temas
                .Include(t => t.Sala)
                .FirstOrDefaultAsync(m => m.TemaId == id);
            if (temas == null)
            {
                return NotFound();
            }

            return View(temas);
        }

        public async Task<IActionResult> Reserva(int? id) {

            var tema = await _context.Temas.Include(f => f.ListaFotos.Where(f => f.Deleted == false)).Include(s => s.Sala).ThenInclude(s => s.ListaReservas).ThenInclude(s => s.Cliente).Where(r => !r.Deleted).Where(s => s.SalaID == id).FirstOrDefaultAsync();
            if (tema == null) {
                return NotFound();
            }

            ReservaViewModel rvm = new ReservaViewModel(tema.Sala, tema);
            if (TempData["viewStart"] != null && TempData["viewEnd"] != null) {
                rvm.viewStart = (DateTime)TempData["viewStart"];
                rvm.viewEnd = (DateTime)TempData["viewEnd"];
            }

            return View(rvm);
        }

        [HttpPost]
        public async Task<IActionResult> ReservaData(DateTime dateI, int salaId, string viewType, DateTime viewStart, DateTime viewEnd) {


            var tema = await _context.Temas.Include(f => f.ListaFotos.Where(f => f.Deleted == false)).Include(s => s.Sala).ThenInclude(s => s.ListaReservas).ThenInclude(s => s.Cliente).Where(r => !r.Deleted).Where(s => s.SalaID == salaId).FirstOrDefaultAsync();


            if (tema == null) {
                return NotFound();
            }
            ReservaViewModel rvm = new ReservaViewModel(tema.Sala, tema);
            rvm.dataI = dateI;
            rvm.viewType = viewType;
            rvm.viewStart = viewStart;
            rvm.viewStart = viewEnd;



            return PartialView("_reservasModals", rvm);

        }


        [HttpPost]
        public async Task<IActionResult> Reserva(int id,ReservaViewModel rvm) {

            if (ModelState.IsValid) {
                var t = "s";
            }

            Clientes u = null;

            u = await _context.Clientes.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (u == null) {
                TempData["ErroCliente"] = "Erro! Apenas clientes podem fazer reservas!";
                return NotFound();
            }

            var r = new Reservas(u);

            var tema = await _context.Temas.Include(s => s.Sala).Where(r => !r.Deleted).Where(s => s.SalaID == rvm.Sala.SalaId).FirstOrDefaultAsync();
            if (tema == null) {
                TempData["ErroCliente"] = "Erro! Não é possível reservar uma sala sem tema atribuído!";
                return View("Privacy");
            }
            var endDate = rvm.dataI.AddMinutes(tema.TempoEstimado);
            var sala = await _context.Salas.FindAsync(tema.SalaID);
            r.SalaId = sala.SalaId;
            r.NumPessoas = rvm.nPessoas;
            r.Sala = tema.Sala;
            r.ReservaDate = rvm.dataI;
            r.ReservaEndDate = endDate;
            tema.Sala.ListaReservas.Add(r);

            _context.Update(tema.Sala);
            await _context.SaveChangesAsync();
            TempData["viewEnd"] = rvm.viewEnd;
            TempData["viewStart"] = rvm.viewStart;

            TempData["ReservaSucesso"] = "Reserva para " + r.ReservaDate.ToString("dd-MM-yyyy HH:mm:ss") + " efetuada com sucesso";
            return RedirectToAction(nameof(Reserva),rvm.Sala.SalaId);






        }





        // POST: TemasGeral/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var temas = await _context.Temas.FindAsync(id);
            if (temas != null)
            {
                _context.Temas.Remove(temas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TemasExists(int id)
        {
            return _context.Temas.Any(e => e.TemaId == id);
        }
    }
}
