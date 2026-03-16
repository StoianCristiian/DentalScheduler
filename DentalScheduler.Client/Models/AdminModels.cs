namespace DentalScheduler.Client.Models;

public class AdminDashboardStats
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class UserWithRoleDto
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } // "Admin", "Doctor", "Patient"

    public string FullName => $"{FirstName} {LastName}";
}

public class UpdateRoleRequest
{
    public string NewRole { get; set; }
}

