using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Vintaloo.WebApp.Controllers
{
    public class AuthController : Controller
    {

        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
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
                Secure = Request.IsHttps,
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

        // GET /Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordDTO());
        }

        // POST /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var token = _authService.GenerarTokenRecuperacion(dto.Correo);
            if (token != null)
            {
                var enlace = Url.Action(nameof(ResetPassword), "Auth", new { token }, Request.Scheme);
                if (!string.IsNullOrWhiteSpace(enlace))
                    await _emailService.EnviarRecuperacionAsync(dto.Correo, enlace);
            }

            TempData["Mensaje"] = "Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.";
            return RedirectToAction(nameof(Login));
        }

        // GET /Auth/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "El enlace de recuperación no es válido.";
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordDTO { Token = token });
        }

        // POST /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (!_authService.RestablecerPassword(dto.Token, dto.Password))
            {
                ModelState.AddModelError(string.Empty, "El enlace no es válido o ya venció. Solicita uno nuevo.");
                return View(dto);
            }

            TempData["Mensaje"] = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";
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
