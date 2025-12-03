using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace rodri_movie_mvc.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
        public string ImagenUrlPerfil { get; set; }
        public List<Favorito>? PeliculasFavoritas { get; set; }
        public List<Review>? ReviewsUsuario { get; set; }
    }

    public class UsuarioViewModel
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        [Required]
        [StringLength(50)]
        public string Apellido { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [PasswordPropertyText]
        [Required]
        public string Clave { get; set; }
        [PasswordPropertyText]
        public string ConfirmarClave { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Clave { get; set; }
    }
}
