using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class AuthController : Controller
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está logueado, redirige al Dashboard
            if (Request.Cookies["jwt_token"] != null)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginDTO());
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var token = _authService.Login(dto);

            if (token == null)
            {
                TempData["Error"] = "Correo o contraseña incorrectos.";
                return View(dto);
            }

            // Guardar el token en una cookie HttpOnly
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // cambiar a true en producción con HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            // Guardar nombre en sesión para mostrarlo en la UI
            HttpContext.Session.SetString("UsuarioNombre", dto.Correo);

            TempData["Mensaje"] = "¡Bienvenido de vuelta!";
            return RedirectToAction("Index", "Dashboard");
        }

        // GET /Auth/Registrar
        [HttpGet]
        public IActionResult Registrar()
        {
            return View(new RegisterDTO());
        }

        // POST /Auth/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var exitoso = _authService.Registrar(dto);

            if (!exitoso)
            {
                TempData["Error"] = "El correo ya está registrado. Prueba con otro.";
                return View(dto);
            }

            TempData["Mensaje"] = "Cuenta creada correctamente. Inicia sesión.";
            return RedirectToAction(nameof(Login));
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Eliminar la cookie del token
            Response.Cookies.Delete("jwt_token");

            // Limpiar la sesión
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Login));
        }
    }
}
