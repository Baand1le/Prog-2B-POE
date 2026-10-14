using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.DTOs
{
    public class CreateResultDTO
    {
        [Required] public TimeSpan finishTime { get; set; }
        [Range(1, int.MaxValue)]
        public int finishPosition { get; set; }

    }//end of class

}//end of namespace
