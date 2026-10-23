
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IO;
using VistoPorMim.Data;
using VistoPorMim.Models;

namespace VistoPorMim.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;
        private readonly IWebHostEnvironment _environment;

        private const long TamanhoMaximoFoto = 2 * 1024 * 1024;

        public UsuariosController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            usuario.Nome = usuario.Nome.Trim();
            usuario.Email = usuario.Email.Trim();

            bool emailExiste = await _context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email);

            if (emailExiste)
            {
                ModelState.AddModelError(
                    nameof(usuario.Email),
                    "Este e-mail já está cadastrado."
                );

                return View(usuario);
            }

            usuario.TipoUsuario = "Usuário";
            usuario.DataCadastro = DateTime.Now;

            usuario.Senha = _passwordHasher.HashPassword(
                usuario,
                usuario.Senha
            );

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Informe o e-mail e a senha."
                );

                return View();
            }

            email = email.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha incorretos."
                );

                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Senha,
                senha
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha incorretos."
                );

                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario)
            };

            var identidade = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identidade);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Usuarios");
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var idUsuario = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuario, out int id))
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var idUsuario = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuario, out int id))
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(
            string nome,
            string? telefone,
            string? biografia,
            IFormFile? fotoArquivo)
        {
            var idUsuario = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuario, out int id))
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                ViewData["Erro"] = "O nome é obrigatório.";
                return View(usuario);
            }

            if (nome.Trim().Length > 100)
            {
                ViewData["Erro"] =
                    "O nome deve ter no máximo 100 caracteres.";

                return View(usuario);
            }

            if (biografia?.Length > 500)
            {
                ViewData["Erro"] =
                    "A biografia deve ter no máximo 500 caracteres.";

                return View(usuario);
            }

            usuario.Nome = nome.Trim();
            usuario.Telefone = telefone?.Trim();
            usuario.Biografia = biografia?.Trim();

            string? caminhoFotoAntiga = usuario.FotoPerfil;
            string? novoCaminhoFoto = null;

            if (fotoArquivo != null && fotoArquivo.Length > 0)
            {
                if (fotoArquivo.Length > TamanhoMaximoFoto)
                {
                    ViewData["Erro"] =
                        "A foto deve ter no máximo 2 MB.";

                    return View(usuario);
                }

                string extensao = Path.GetExtension(
                    fotoArquivo.FileName
                ).ToLowerInvariant();

                string[] extensoesPermitidas =
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!extensoesPermitidas.Contains(extensao))
                {
                    ViewData["Erro"] =
                        "Escolha uma imagem JPG, PNG ou WebP.";

                    return View(usuario);
                }

                if (!await AssinaturaDaImagemValida(fotoArquivo, extensao))
                {
                    ViewData["Erro"] =
                        "O arquivo selecionado não é uma imagem válida.";

                    return View(usuario);
                }

                string pastaFotos = Path.Combine(
                    _environment.WebRootPath
                        ?? Path.Combine(
                            _environment.ContentRootPath,
                            "wwwroot"
                        ),
                    "uploads",
                    "perfis"
                );

                Directory.CreateDirectory(pastaFotos);

                string nomeArquivo =
                    Guid.NewGuid().ToString("N") + extensao;

                string caminhoCompleto = Path.Combine(
                    pastaFotos,
                    nomeArquivo
                );

                await using (var stream = new FileStream(
                    caminhoCompleto,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    await fotoArquivo.CopyToAsync(stream);
                }

                novoCaminhoFoto = "/uploads/perfis/" + nomeArquivo;
                usuario.FotoPerfil = novoCaminhoFoto;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (novoCaminhoFoto != null)
                {
                    string pastaFotos = Path.Combine(
                        _environment.WebRootPath
                            ?? Path.Combine(
                                _environment.ContentRootPath,
                                "wwwroot"
                            ),
                        "uploads",
                        "perfis"
                    );

                    string caminhoNovo = Path.Combine(
                        pastaFotos,
                        Path.GetFileName(novoCaminhoFoto)
                    );

                    if (System.IO.File.Exists(caminhoNovo))
                    {
                        System.IO.File.Delete(caminhoNovo);
                    }
                }

                throw;
            }

            // Apagar a foto anterior somente se ela pertencer
            // à pasta de fotos de perfil deste aplicativo.
            if (!string.IsNullOrWhiteSpace(caminhoFotoAntiga) &&
                novoCaminhoFoto != null &&
                caminhoFotoAntiga.StartsWith(
                    "/uploads/perfis/",
                    StringComparison.OrdinalIgnoreCase))
            {
                string pastaFotos = Path.GetFullPath(
                    Path.Combine(
                        _environment.WebRootPath
                            ?? Path.Combine(
                                _environment.ContentRootPath,
                                "wwwroot"
                            ),
                        "uploads",
                        "perfis"
                    )
                );

                string caminhoAntigo = Path.GetFullPath(
                    Path.Combine(
                        pastaFotos,
                        Path.GetFileName(caminhoFotoAntiga)
                    )
                );

                if (caminhoAntigo.StartsWith(
                        pastaFotos + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase) &&
                    System.IO.File.Exists(caminhoAntigo))
                {
                    System.IO.File.Delete(caminhoAntigo);
                }
            }

            // Atualizar o nome armazenado na autenticação.
            var identidade = User.Identity as ClaimsIdentity;
            var claimNome = identidade?.FindFirst(ClaimTypes.Name);

            if (identidade != null && claimNome != null)
            {
                identidade.RemoveClaim(claimNome);

                identidade.AddClaim(
                    new Claim(ClaimTypes.Name, usuario.Nome)
                );

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identidade)
                );
            }

            TempData["Sucesso"] = "Perfil atualizado com sucesso!";

            return RedirectToAction(nameof(Perfil));
        }

        private static async Task<bool> AssinaturaDaImagemValida(
            IFormFile arquivo,
            string extensao)
        {
            byte[] cabecalho = new byte[12];

            await using var stream = arquivo.OpenReadStream();

            int lidos = await stream.ReadAsync(
                cabecalho.AsMemory(0, cabecalho.Length)
            );

            if (extensao == ".jpg" || extensao == ".jpeg")
            {
                return lidos >= 3 &&
                       cabecalho[0] == 0xFF &&
                       cabecalho[1] == 0xD8 &&
                       cabecalho[2] == 0xFF;
            }

            if (extensao == ".png")
            {
                byte[] assinaturaPng =
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                };

                return lidos >= 8 &&
                       cabecalho.Take(8).SequenceEqual(assinaturaPng);
            }

            if (extensao == ".webp")
            {
                return lidos >= 12 &&
                       cabecalho[0] == 'R' &&
                       cabecalho[1] == 'I' &&
                       cabecalho[2] == 'F' &&
                       cabecalho[3] == 'F' &&
                       cabecalho[8] == 'W' &&
                       cabecalho[9] == 'E' &&
                       cabecalho[10] == 'B' &&
                       cabecalho[11] == 'P';
            }

            return false;
        }
    }
}
