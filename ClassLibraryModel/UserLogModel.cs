using System.ComponentModel.DataAnnotations;

namespace ClassLibraryModel
{
    public class UserLogModel
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
