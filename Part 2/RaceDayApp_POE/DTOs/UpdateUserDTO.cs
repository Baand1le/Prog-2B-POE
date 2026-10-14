using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class UpdateUserDTO
    {
        [Required] public string firstName { get; set; } = "";
        [Required] public string surname { get; set; } = "";
        [Required, EmailAddress] public string email { get; set; } = "";

    }//end of class

}//end of namespace