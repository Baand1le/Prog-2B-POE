using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class LoginDTO
    {
        [Required][EmailAddress] public string email { get; set; } = "";
        [Required] public string password { get; set; } = "";

    }//end of class

}//end of namespace