using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VistoPorMim.Models
{
    [Table("avaliacao")]
    public class Avaliacao
    {
        [Key]
        [Column("id_avaliacao")]
        public int IdAvaliacao { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_local")]
        public int IdLocal { get; set; }

        [Range(1, 5)]
        [Column("nota")]
        public int Nota { get; set; }

        [Column("comentario")]
        public string? Comentario { get; set; }

        [Column("data_avaliacao")]
        public DateTime DataAvaliacao { get; set; }
    }
}