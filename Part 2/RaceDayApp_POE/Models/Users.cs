namespace RaceDayApp_POE.Models
{
    public class Users
    {
        public string userID {  get; set; }
        public int roleID { get; set; }
        public string PasswordHash { get; set; } = "";
        public string fisrtName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

        public UserRoles userRole { get; set; }

        public ICollection<Events> Events { get; set; }
        public ICollection<Enrollments> Enrollement {  get; set; }
    }
}
