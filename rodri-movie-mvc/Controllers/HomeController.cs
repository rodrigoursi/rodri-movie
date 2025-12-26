using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using rodri_movie_mvc.Data;
using rodri_movie_mvc.Models;
using System.Diagnostics;
using System.Linq;

namespace rodri_movie_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MovieDbContext _context;
        private readonly int muestraXpag = 4;

        public HomeController(ILogger<HomeController> logger, MovieDbContext contex)
        {
            _logger = logger;
            _context = contex;
        }

        public async Task<IActionResult> Index(int pagina = 1, string fraseBuscar = "", int genero = 0)
        {
            // preparar listado genero
            var generos = await _context.Generos.OrderBy(g => g.Descripcion).ToListAsync();
            generos.Insert(0, new Genero { Id = 0, Descripcion = "Todos" });
            ViewBag.Generos = new SelectList(generos, "Id", "Descripcion", genero);

            //var peliculas = await _context.Peliculas.ToListAsync();
            if (pagina < 1) pagina = 1;

            IQueryable<Pelicula> peliculasQuery = _context.Peliculas;
            if (!string.IsNullOrEmpty(fraseBuscar))
            {
                peliculasQuery = peliculasQuery.Where(p => p.Titulo.Contains(fraseBuscar));
                ViewBag.FraseBuscar = fraseBuscar;
            }

            if (genero > 0) peliculasQuery = peliculasQuery.Where(p => p.GeneroId == genero);

            int totalPelis = await peliculasQuery.CountAsync();
            //int totalPelis = await _context.Peliculas.CountAsync();
            /*int totalPag = totalPelis / muestraXpag;
            int resto = totalPelis % muestraXpag;
            if (resto != 0) totalPag += 1;*/
            // esto es equivalente a lo de arriba comentado
            int totalPag = (int)Math.Ceiling(totalPelis / (double)muestraXpag); // esto redondea para arriba siempre que sea con coma.

            if (pagina > totalPag) pagina = totalPag; // si la pagina mandada es mayor al total real seteo con el total real
            int skip = (pagina - 1) * muestraXpag; // aca preparo la variable de salteo

            List<Pelicula> peliculas = new List<Pelicula>();
            if(skip > -1)
            {
                peliculas = await peliculasQuery
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(muestraXpag)
                .ToListAsync();
            }
            

            ViewBag.TotalPag = totalPag;
            ViewBag.PaginaActual = pagina;
            ViewBag.GeneroId = genero;

            return View(peliculas);
        }

        public async Task<IActionResult> Details(int Id)
        {
            var Pelicula = await _context.Peliculas.Include(p => p.Genero)
                .Include(p => p.ListaReviews)
                .ThenInclude(lr => lr.Usuario)
                .FirstOrDefaultAsync(p => p.Id == Id);
            return View(Pelicula);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
