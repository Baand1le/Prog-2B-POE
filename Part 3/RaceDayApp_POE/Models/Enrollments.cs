using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class Enrollments
    {
        [Key]
        public string enrollmentID { get; set; }
        public string participantID { get; set; }
        public string eventID { get; set; }
        public string categoryID { get; set; }
        public DateTime enrollmentDate { get; set; }

        public Users Participant {  get; set; }
        public Events Event { get; set; }
        public Categories Category { get; set; }
        public Results Result { get; set; }
    }//end of class: Enrollements

}//end of namespace
