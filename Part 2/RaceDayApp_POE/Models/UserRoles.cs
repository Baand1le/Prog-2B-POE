using System.ComponentModel.DataAnnotations;

namespace RaceDayApp_POE.Models
{
    public class UserRoles
    {
        [Key]
        public int roleID { get; set; }
        public string roleType { get; set; }

        public ICollection<Users> User {  get; set; }

    }//end of class: UserRoles

}//end of namespace