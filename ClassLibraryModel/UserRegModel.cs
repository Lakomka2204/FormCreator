using System.ComponentModel.DataAnnotations;

namespace ClassLibraryModel
{
    public class UserRegModel
    {
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Compare(nameof(ConfirmPassword))]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
