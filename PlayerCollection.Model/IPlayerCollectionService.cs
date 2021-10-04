using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCollection.Model
{
    public interface IPlayerCollectionService : IService
    {
        Task<Player[]> GetAllPlayersAsync(CancellationToken cancellationToken = default);
        Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default);
        Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task MovePlayersAsync(CancellationToken cancellationToken = default);
        Task<Player> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default);

        Task<List<Int64>> GetPartitionsLowKey(CancellationToken cancellationToken = default);

        Task<int> GetPartitionCount(CancellationToken cancellationToken = default);
        Task<int> GetActivePlayerCount(CancellationToken cancellationToken = default);
    }
}
