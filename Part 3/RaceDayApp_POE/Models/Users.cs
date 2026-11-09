using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class Users
    {
        [Key]
        public string userID {  get; set; }
        public int roleID { get; set; }
        public string PasswordHash { get; set; } = "";
        public string firstName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public UserRoles userRole { get; set; }

        public ICollection<Events> Event { get; set; }
        public ICollection<Enrollments> Enrollment {  get; set; }

    }//end of class: Users

}//end of namespace