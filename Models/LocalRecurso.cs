using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistoPorMim.Models
{
    [Table("local_recurso")]
    public class LocalRecurso
    {
        [Key]
        [Column("id_local")]
        public int IdLocal { get; set; }

        [Column("id_recurso")]
        public int IdRecurso { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Não informado";

        [Column("observacao")]
        public string? Observacao { get; set; }
    }
}