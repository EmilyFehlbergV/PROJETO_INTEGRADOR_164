using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistoPorMim.Models
{
    [Table("local")]
    public class Local
    {
        [Key]
        [Column("id_local")]
        public int IdLocal { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("categoria")]
        public string Categoria { get; set; } = "Outro";

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("logradouro")]
        public string? Logradouro { get; set; }

        [Column("numero")]
        public string? Numero { get; set; }

        [Column("bairro")]
        public string? Bairro { get; set; }

        [Column("cidade")]
        public string? Cidade { get; set; }

        [Column("estado")]
        public string? Estado { get; set; }

        [Column("cep")]
        public string? Cep { get; set; }

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("data_cadastro")]
        public DateTime DataCadastro { get; set; }
    }
}