namespace RaceDay.Web.ViewModels;

public class ApiLoginResponse
{
    public string UserID { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ApiEvent
{
    public string EventID { get; set; } = string.Empty;
    public string OrganiserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}

public class ApiEventSaveResponse
{
    public string EventID { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ApiCategory
{
    public string CategoryID { get; set; } = string.Empty;
    public string EventID { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
}

public class ApiProfile
{
    public string UserID { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ApiParticipantEnrollment
{
    public string EnrollmentID { get; set; } = string.Empty;
    public string EventID { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventLocation { get; set; } = string.Empty;
    public string CategoryID { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
}

public class ApiEventEnrollment
{
    public string EnrollmentID { get; set; } = string.Empty;
    public string ParticipantID { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string CategoryID { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
}

public class ApiMyResult
{
    public string ResultID { get; set; } = string.Empty;
    public string EnrollmentID { get; set; } = string.Empty;
    public string EventID { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
}

public class ApiEventResult
{
    public string ResultID { get; set; } = string.Empty;
    public string EnrollmentID { get; set; } = string.Empty;
    public string ParticipantID { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
}

public class ApiMessage
{
    public string Message { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EventRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
}

public class CategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
}

public class EnrollmentRequest
{
    public string CategoryID { get; set; } = string.Empty;
}

public class ResultRequest
{
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
}
