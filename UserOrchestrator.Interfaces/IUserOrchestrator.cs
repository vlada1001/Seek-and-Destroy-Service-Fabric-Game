using Common.API;
using Microsoft.ServiceFabric.Services.Remoting;
using PlayerCollection.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UserOrchestrator.Interfaces
{
    public interface IUserOrchestrator : IService
    {
        Task<IEnumerable<PlayerAPI>> GetPlayersAsync(CancellationToken cancellationToken = default);
        Task<PlayerAPI> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default);
        Task MovePlayersAsync(CancellationToken cancellationToken = default);
        Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
        Task CreatePlayerAsync(CancellationToken cancellationToken = default);
        Task CreatePlayersAsync(int count, CancellationToken cancellationToken = default);
        Task<Int64> GetActivePlayersCountAsync(CancellationToken cancellationToken = default);
    }
}
