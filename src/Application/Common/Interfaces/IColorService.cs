namespace SwimmingClub.Application.Common.Interfaces;

public interface IColorService
{
    string GenerateBookingColor(Guid? customerId = null, Guid? bookingTypeId = null);
    string GenerateCustomerColor(Guid customerId);
    string GetUniqueColor(IEnumerable<string> usedColors);
}
