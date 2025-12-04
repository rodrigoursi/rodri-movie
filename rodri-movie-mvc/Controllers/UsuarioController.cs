using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rodri_movie_mvc.Models;

namespace rodri_movie_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario>? _userManager;
        private readonly SignInManager<Usuario>? _signInManager;

        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel usuario)
        {
            if (_userManager is null) return NotFound();
            if (_signInManager is null) return NotFound();
            if (usuario == null) return NotFound();

            if(!ModelState.IsValid) return View(usuario);

            bool recordarme = false;

            var resultado = await _signInManager.PasswordSignInAsync(usuario.Email, usuario.Clave, recordarme, lockoutOnFailure: false);
            if(resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            // Manejo de errores
            if (resultado.IsLockedOut)
                ModelState.AddModelError("", "La cuenta está bloqueada temporalmente.");
            else if (resultado.IsNotAllowed)
                ModelState.AddModelError("", "No tenés permitido iniciar sesión. ¿Verificaste el email?");
            else if (resultado.RequiresTwoFactor)
                ModelState.AddModelError("", "Se requiere autenticación en dos pasos.");
            else
                ModelState.AddModelError("", "Email o contraseña incorrectos.");

            return View(usuario);
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(UsuarioViewModel usuario)
        {
            if(_userManager is null) return NotFound();
            if (_signInManager is null) return NotFound();
            if (usuario == null) return NotFound();
            if (!validarClave(usuario.Clave, usuario.ConfirmarClave)) return NotFound();
            var nuevoUsuario = new Usuario();
            nuevoUsuario.UserName = usuario.Email;
            nuevoUsuario.Email = usuario.Email;
            nuevoUsuario.Nombre = usuario.Nombre;
            nuevoUsuario.Apellido = usuario.Apellido;
            nuevoUsuario.ImagenUrlPerfil = "default-profile.png";
            var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Clave);

            if(resultado.Succeeded)
            {
                await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            } 
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
                return View(usuario);
        }

        public async Task<IActionResult> Logout()
        {
            if (_signInManager is null) return NotFound();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            if (_userManager is null) return NotFound();
            var usuario = await _userManager.GetUserAsync(User);
            PerfilViewModel usuarioPerfil = new PerfilViewModel();
            usuarioPerfil.Nombre = usuario.Nombre;
            usuarioPerfil.Apellido = usuario.Apellido;
            usuarioPerfil.Email = usuario.Email;
            return View(usuarioPerfil);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel usuarioVM)
        {
            if (_userManager is null) return NotFound();
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if(usuario == null) return NotFound();
                usuario.Nombre = usuarioVM.Nombre;
                usuario.Apellido = usuarioVM.Apellido;

                var resultado = await _userManager.UpdateAsync(usuario);

                if (resultado.Succeeded)
                {
                    ViewBag.Mensaje = "Perfil actualizado con éxito.";
                    return View(usuarioVM);
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(usuarioVM);
        }


        private bool validarClave(string clave, string confirmar)
        {
            if (string.IsNullOrEmpty(clave)) return false;
            if (clave != confirmar) return false;
            return true;
        }
    }
}
