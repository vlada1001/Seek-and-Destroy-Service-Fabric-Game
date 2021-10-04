using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCollection.Model
{
    public interface IPlayerRepository
    {
        Task<IEnumerable<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default);
        Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default);
        Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task MovePlayersAsync(CancellationToken cancellationToken = default);
        Task<Player> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task UpdatePlayerAsync(Player updatedPlayer, CancellationToken cancellationToken = default);
        Task<int> GetActivePlayerCount(CancellationToken cancellationToken = default);
    }
}
