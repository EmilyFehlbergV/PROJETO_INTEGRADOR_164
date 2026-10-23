using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authentication;

namespace VistoPorMim.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("senha")]
        public string Senha { get; set; } = string.Empty;

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [Column("biografia")]
        public string? Biografia { get; set; }

        [Required]
        [Column("tipo_usuario")]
        public string TipoUsuario { get; set; } = "Usuário";

        [Column("data_cadastro")]
        public DateTime DataCadastro { get; set; }
        
        
    }
}