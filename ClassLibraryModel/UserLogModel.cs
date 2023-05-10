using System.ComponentModel.DataAnnotations;

namespace FCApi.Models
{
    public class UserLogModel
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
