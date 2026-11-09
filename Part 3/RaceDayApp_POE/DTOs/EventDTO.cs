using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class EventDTO
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Description { get; set; } = "";
        [Required] public DateTime Date { get; set; }
        [Required] public string Location { get; set; } = "";
        [Required] public string Distance { get; set; } = "";
        [Required] public string eventType { get; set; } = "";

    }//end of class

}//end of namespace