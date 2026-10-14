using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class CreateEnrollmentDTO
    {
        [Required] public string categoryID { get; set; } = "";

    }//end of class

}//end of namespace