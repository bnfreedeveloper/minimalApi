using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace minimalApi.Models
{
    public class Command
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(200)")]
        public string? HowTo { get; set; }
        [Required]
        [MaxLength(5)]

        public string? Platform { get; set; }
        [Required]
        public string? CommandLine { get; set; }
    }
}