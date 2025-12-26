using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rodri_movie_mvc.Data;
using rodri_movie_mvc.Models;

namespace rodri_movie_mvc.Controllers
{
    public class ReviewController : Controller
    {
        private readonly MovieDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        public ReviewController(UserManager<Usuario> userManager, MovieDbContext context) 
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ReviewController
        public async Task<ActionResult> Index()
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();
            var Reviews = await _context.Reviews
                .Include(r => r.Pelicula)
                .Where(r => r.UsuarioId == userId).ToListAsync();
            

            return View(Reviews);
        }

        // GET: ReviewController/Details/5
        public ActionResult Details(int id)
        {
            
            return View();
        }

        // GET: ReviewController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReviewController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ReviewCreateViewModel ReviewVM)
        {
            try
            {
                ReviewVM.UsuarioId = _userManager.GetUserId(User);

                var existe = _context.Reviews.FirstOrDefaultAsync(r =>
                r.PeliculaId == ReviewVM.PeliculaId &&
                r.UsuarioId == ReviewVM.UsuarioId);

                if (existe != null)
                {
                    TempData["ReviewExiste"] = "Ya has realizado una reseña para esta pelicula";
                    return RedirectToAction("Details", "Home", new { id = ReviewVM.PeliculaId });
                }

                if (ModelState.IsValid)
                {
                    var nuevaRev = new Review
                    {
                        PeliculaId = ReviewVM.PeliculaId ?? 0,
                        UsuarioId = ReviewVM.UsuarioId,
                        Rating = ReviewVM.Rating,
                        Comentario = ReviewVM.Comentario,
                        FechaReview = DateTime.Now,
                    };
                    _context.Add(nuevaRev);
                    _context.SaveChanges();
                }
                return RedirectToAction("Details", "Home", new { id = ReviewVM.PeliculaId });
            }
            catch
            {
                return RedirectToAction("Details", "Home", new { id = ReviewVM.PeliculaId });
            }
        }

        // GET: ReviewController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var Review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id);
            if (Review == null) return NotFound();

            return View(Review);
        }

        // POST: ReviewController/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReviewCreateViewModel reviewVM)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    string? userId = _userManager.GetUserId(User);
                    if (userId == null) return NotFound();
                    var review = _context.Reviews.Find(reviewVM.Id);
                    if (review == null) return NotFound();
                    if(review.UsuarioId != userId) return Forbid();
                    review.Rating = reviewVM.Rating;
                    review.Comentario = reviewVM.Comentario;
                    _context.Reviews.Update(review);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                return View(reviewVM);
            }
            catch
            {
                return View(reviewVM);
            }
        }

        // GET: ReviewController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReviewController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
