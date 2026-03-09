namespace DentalScheduler.Application.Appointments.Queries.GetDoctors;

public interface IDoctorRoleChecker
{
    Task<List<DoctorDto>> GetUsersInRoleAsync(string role);
}

