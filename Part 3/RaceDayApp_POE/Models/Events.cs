using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class Events
    {
        [Key]
        public string eventID { get; set; }
        public string organiserID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
        public string eventType { get; set; }
        

        public Users Organiser { get; set; }

        public ICollection<Categories> Category { get; set; }
        public ICollection<Enrollments> Enrollment { get; set; }

    }//end of class: Events

}//end of namespace