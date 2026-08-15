using UserApi.Common;
using UserApi.DTOs.Responses;

namespace UserApi.Services.Interfaces;

public interface IDashboardService
{
    Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync(int currentUserId, string currentUserRole);
}