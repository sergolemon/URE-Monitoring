using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URE.Core.Models.Identity
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [MaxLength(8)]
        public string CarNumber { get; set; }
        [Required]
        [MaxLength(60)]
        public string Name { get; set; }
        [Required]
        [MaxLength(60)]
        public string Surname { get; set; }
        [Required]
        [MaxLength(60)]
        public string Patronymic { get; set; }
        [NotMapped]
        public string PersonInfo => string.Join(" ", new object[] { Surname, !string.IsNullOrEmpty(Name) ? Name.First() + "." : null, !string.IsNullOrEmpty(Patronymic) ? Patronymic.First() + "." : null, !string.IsNullOrEmpty(CarNumber) ? $"({CarNumber})" : null }.Where(x => !string.IsNullOrEmpty(x?.ToString())));

    }
}
