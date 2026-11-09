using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class CreateCategoryDTO
    {
        [Required] public string categoryName { get; set; } = "";
        [Required] public string ageGroup { get; set; } = "";

    }//end of class

}//end of namespace
