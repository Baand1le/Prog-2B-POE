namespace RaceDayApp_POE.Models
{
    public class Events
    {
        public string eventID { get; set; }
        public string orgaizerID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date {  get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
        public string eventType { get; set; }
        

        public Users Organizer { get; set; }

        public ICollection<Categories> Category { get; set; }
        public ICollection<Enrollments> Enrollment { get; set; }
    }
}
