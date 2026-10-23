using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistoPorMim.Models
{
    [Table("recurso")]
    public class Recurso
    {
        [Key]
        [Column("id_recurso")]
        public int IdRecurso { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("descricao")]
        public string? Descricao { get; set; }
    }
}