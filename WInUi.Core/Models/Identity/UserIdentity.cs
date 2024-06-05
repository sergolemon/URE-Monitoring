
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URE.Core.Models.Identity
{
    public class UserIdentity
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        public string UserName { get; set; }
        [NotMapped]
        public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
        [NotMapped]
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
