using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class Categories
    {
        [Key]
        public string categoryID { get; set;  }
        public string eventID { get; set; }
        public string categoryName { get; set; }
        public string ageGroup { get; set; }

        public Events  Event { get; set; }

        public ICollection<Enrollments> Enrollment { get; set; }
    }//end of class: Categories

}//end of namespace
