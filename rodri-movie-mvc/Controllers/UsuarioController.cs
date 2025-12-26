using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using rodri_movie_mvc.Models;
using rodri_movie_mvc.Service;

namespace rodri_movie_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario>? _userManager;
        private readonly SignInManager<Usuario>? _signInManager;
        private readonly RoleManager<IdentityRole>? _roleManager;
        private readonly ImagenStorage _imagenStorage;

        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, RoleManager<IdentityRole> roleManager, ImagenStorage imagenStorage)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _imagenStorage = imagenStorage;
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
            usuarioPerfil.ImagenUrlPerfil = usuario.ImagenUrlPerfil;
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

                try
                {
                    if (usuarioVM.ImagenFilePerfil is not null && usuarioVM.ImagenFilePerfil.Length > 0)
                    {
                        var ruta = await _imagenStorage.SaveAsync(usuario.Id, usuarioVM.ImagenFilePerfil);
                        usuario.ImagenUrlPerfil = ruta;
                        usuarioVM.ImagenUrlPerfil = ruta;
                    }
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(usuarioVM);
                }

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

        [Authorize]
        public async Task<IActionResult> Panel()
        {
            var roles = _roleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");

            var usuarios = _userManager.Users.ToList();
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "UserName");

            var panelUsuarios = new List<PanelUsViewModel>();

            foreach (var usuario in usuarios)
            {
                var RolesUsuario = await _userManager.GetRolesAsync(usuario);
                var panelUsuario = new PanelUsViewModel
                {
                    IdUsuario = Guid.Parse(usuario.Id),
                    UserName = usuario.UserName,
                    Email = usuario.Email,
                    Roles = RolesUsuario.ToList()
                };
                panelUsuarios.Add(panelUsuario);
            }

            ViewBag.UsuariosRoles = panelUsuarios;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> CreateRole(string rol)
        {
            if(rol is null) return NotFound();
            rol = rol.Trim();
            rol = rol.ToLower();
            if(await _roleManager.RoleExistsAsync(rol))
            {
                ViewBag.ErrorMsg = "Rol ya existente";
                ViewBag.Rol = rol;
                return View("Panel");
            }
            await _roleManager.CreateAsync(new IdentityRole(rol));
            TempData["SuccessMsg"] = "Rol creado exitosamente";

            return RedirectToAction("Panel");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(PanelUsViewModel PanelUsuario)
        {
            if (PanelUsuario is null) return NotFound();
            var Usuario = await _userManager.FindByIdAsync(PanelUsuario.IdUsuario.ToString());
            await _userManager.AddToRoleAsync(Usuario, PanelUsuario.RolSelec);

            return RedirectToAction("Panel");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(PanelUsViewModel PanelUsuario)
        {
            if (PanelUsuario is null) return NotFound();
            var Usuario = await _userManager.FindByIdAsync(PanelUsuario.IdUsuario.ToString());
            var token = await _userManager.GeneratePasswordResetTokenAsync(Usuario);
            var result = await _userManager.ResetPasswordAsync(Usuario, token, "nuevo");
            TempData["resetPassMsg"] = "No se pudo cambiar la contraseña";
            if (result.Succeeded)
            {
                TempData["resetPassMsg"] = "Contraseña cambiada exitosamente.";
            }
            return RedirectToAction("Panel");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool validarClave(string clave, string confirmar)
        {
            if (string.IsNullOrEmpty(clave)) return false;
            if (clave != confirmar) return false;
            return true;
        }
    }
}
