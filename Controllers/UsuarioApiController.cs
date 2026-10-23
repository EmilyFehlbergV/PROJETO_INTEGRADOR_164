
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using VistoPorMim.Data;

namespace VistoPorMim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuarioApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UsuarioApi
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _context.Usuarios
                .Select(usuario => new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Email,
                    usuario.TipoUsuario
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/UsuarioApi/perfil
        [HttpGet("perfil")]
        public async Task<IActionResult> MeuPerfil()
        {
            var idUsuario = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuario, out int id))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .Where(usuario => usuario.IdUsuario == id)
                .Select(usuario => new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Email,
                    usuario.Telefone,
                    usuario.FotoPerfil,
                    usuario.Biografia,
                    usuario.TipoUsuario,
                    usuario.DataCadastro
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            return Ok(usuario);
        }

        // PUT: api/UsuarioApi/perfil
        [HttpPut("perfil")]
        public async Task<IActionResult> AtualizarPerfil(
            [FromBody] AtualizarPerfilRequest dados)
        {
            var idUsuario = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(idUsuario, out int id))
            {
                return Unauthorized();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(
                    usuario => usuario.IdUsuario == id
                );

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });
            }

            if (string.IsNullOrWhiteSpace(dados.Nome))
            {
                return BadRequest(new
                {
                    mensagem = "O nome é obrigatório."
                });
            }

            usuario.Nome = dados.Nome.Trim();
            usuario.Telefone = dados.Telefone;
            usuario.FotoPerfil = dados.FotoPerfil;
            usuario.Biografia = dados.Biografia;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Perfil atualizado com sucesso!",
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Email,
                    usuario.Telefone,
                    usuario.FotoPerfil,
                    usuario.Biografia
                }
            });
        }
    }

    public class AtualizarPerfilRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        public string? FotoPerfil { get; set; }

        [StringLength(500)]
        public string? Biografia { get; set; }
    }
}
