using Microsoft.ServiceFabric.Actors;
using PlayerCollection.Model;
using System.Threading;
using System.Threading.Tasks;

namespace UserActor.Interfaces
{
    public interface IUserActor : IActor
    {
        Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default);
        Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default);
        Task MovePlayerAsync(CancellationToken cancellationToken = default);
        Task DeletePlayerAsync(CancellationToken cancellationToken = default);
        Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default);
    }
}
