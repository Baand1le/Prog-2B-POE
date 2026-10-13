using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class Results
    {
        [Key]
        public string resultID { get; set; }
        public string enrollmentID { get; set; }
        public TimeSpan finishTime { get; set; }
        public int finishPosition { get; set; }

        public Enrollments Enrollment { get; set; }

    }//end of class: Results

}//end of namespace
