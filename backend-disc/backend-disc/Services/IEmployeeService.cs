using backend_disc.Dtos.Employees;
using backend_disc.Models;

namespace backend_disc.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto?> CreateEmployee(CreateNewEmployee dto);
        Task<int?> DeleteAsync(int id);
        Task<PaginatedList<ReadEmployee>> GetAll(int? departmentId, int? discProfileId, int? positionId, string? search, int pageIndex, int pageSize);
        Task<EmployeeOwnProfileDto?> GetByIdAsync(int id);
        Task<int?> UpdatePrivateDataAsync(int id, UpdatePrivateDataDto updateDto);
    }
}