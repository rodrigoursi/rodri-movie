using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using rodri_movie_mvc.Models;

namespace rodri_movie_mvc.Data
{
    public class MovieDbContext : IdentityDbContext<Usuario> // en este caso tengo q agregarle el Usuario, porq no estoy usando solo la tabla de identity si no combinandola con una propia
    {
        // armo el constructor
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        // armo los dbSet
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Plataforma> Plataformas { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }

    }
}
