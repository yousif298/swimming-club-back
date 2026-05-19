using Microsoft.AspNetCore.SignalR;

namespace SwimmingClub.WebAPI.Hubs;

public class BookingHub : Hub
{
    public async Task JoinPoolGroup(string poolId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"pool_{poolId}");

    public async Task LeavePoolGroup(string poolId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pool_{poolId}");

    public async Task NotifyBookingChanged(string poolId, object data)
        => await Clients.Group($"pool_{poolId}").SendAsync("BookingChanged", data);
}
