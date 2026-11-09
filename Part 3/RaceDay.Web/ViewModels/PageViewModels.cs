using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required, StringLength(60)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Account type")]
    public string RoleType { get; set; } = "Participant";
}

public class EventFormViewModel
{
    public string? EventID { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1500)]
    public string Description { get; set; } = string.Empty;

    [Required, Display(Name = "Date and time")]
    public DateTime Date { get; set; } = DateTime.Now.AddDays(14);

    [Required, StringLength(120)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Distance { get; set; } = string.Empty;

    [Required, Display(Name = "Event type")]
    public string EventType { get; set; } = "Road race";

    [Display(Name = "Event banner (JPG, PNG or WEBP; max 3 MB)")]
    public IFormFile? BannerImage { get; set; }

    public string? ExistingBannerUrl { get; set; }
}

public class CategoryFormViewModel
{
    public string EventID { get; set; } = string.Empty;
    public string? CategoryID { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Age group")]
    public string AgeGroup { get; set; } = string.Empty;

    public List<ApiCategory> Categories { get; set; } = new();
    public string EventName { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    [Required, StringLength(60)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Surname { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    [Display(Name = "Profile picture (JPG, PNG or WEBP; max 3 MB)")]
    public IFormFile? ProfilePicture { get; set; }
}

public class OrganiserDashboardViewModel
{
    public List<OrganiserEventCardViewModel> Events { get; set; } = new();
    public int TotalEvents { get; set; }
    public int TotalEnrollments { get; set; }
    public int UpcomingEvents { get; set; }
}

public class OrganiserEventCardViewModel
{
    public ApiEvent Event { get; set; } = new();
    public int EnrollmentCount { get; set; }
    public int ResultCount { get; set; }
    public string? BannerUrl { get; set; }
}

public class ParticipantEventsViewModel
{
    public List<ApiEvent> Events { get; set; } = new();
    public string? Search { get; set; }
    public string? EventType { get; set; }
}

public class EventDetailsViewModel
{
    public ApiEvent Event { get; set; } = new();
    public List<ApiCategory> Categories { get; set; } = new();
    public string? BannerUrl { get; set; }
}

public class ParticipantEnrollmentRow
{
    public ApiParticipantEnrollment Enrollment { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string? ResultSummary { get; set; }
}

public class ParticipantEnrollmentsViewModel
{
    public List<ParticipantEnrollmentRow> Enrollments { get; set; } = new();
}

public class OrganiserEnrollmentsViewModel
{
    public ApiEvent Event { get; set; } = new();
    public List<ApiEventEnrollment> Enrollments { get; set; } = new();
    public HashSet<string> ResultEnrollmentIDs { get; set; } = new();
}

public class OrganiserResultRow
{
    public ApiEventEnrollment Enrollment { get; set; } = new();
    public ApiEventResult? Result { get; set; }
}

public class OrganiserResultsViewModel
{
    public ApiEvent Event { get; set; } = new();
    public List<OrganiserResultRow> Rows { get; set; } = new();
}

public class ResultEntryViewModel
{
    [Required]
    public string EnrollmentID { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Finish time (HH:MM:SS)")]
    public string FinishTime { get; set; } = "00:30:00";

    [Range(1, int.MaxValue)]
    [Display(Name = "Finishing position")]
    public int FinishPosition { get; set; } = 1;
}
