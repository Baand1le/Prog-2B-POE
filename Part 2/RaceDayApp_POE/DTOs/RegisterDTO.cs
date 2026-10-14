using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class RegisterDTO
    {
        [Required] public string firstName { get; set; } = "";
        [Required] public string surname { get; set; } = "";
        [Required, EmailAddress] public string email { get; set; } = "";
        [Required, MinLength(8)] public string password { get; set; } = "";
        [Required] public int roleID { get; set; }


    }//end of class

}//end of namespace
