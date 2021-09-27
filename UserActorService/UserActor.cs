using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PlayerCollection.Model;
using System.Threading;
using System.Threading.Tasks;
using UserActor.Interfaces;
using WebAPI.Controllers;
using static Common.Library;

namespace UserActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class UserActor : Actor, IUserActor
    {
        public UserActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            await StateManager.TryAddStateAsync("player", player, cancellationToken);
            PlayersController.CreateProxy();
            await PlayersController._service.AddPlayerAsync(player, cancellationToken);
        }

        public async Task DeletePlayerAsync(CancellationToken cancellationToken = default)
        {
            await StateManager.TryRemoveStateAsync("player", cancellationToken);
            PlayersController.CreateProxy();
            await PlayersController._service.DeletePlayerAsync(Id.GetGuidId(), cancellationToken);
        }

        public async Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default)
        {
            PlayersController.CreateProxy();
            return await PlayersController._service.GetPlayerAsync(Id.GetGuidId(), cancellationToken);
        }

        public async Task MovePlayerAsync(CancellationToken cancellationToken = default)
        {
            var player = await GetPlayerAsync(cancellationToken);

            if (player != null && player.State.Equals(Status.Exploring))
            {
                player = await StateManager.AddOrUpdateStateAsync<Player>("player", null, (key, value) => value.Move(), cancellationToken);
                PlayersController.CreateProxy();
                await PlayersController._service.UpdatePlayerAsync(player, cancellationToken);
                ActorEventSource.Current.ActorMessage(this, $"[USER.MOVE]: {player.Username} moved to {player.Coordinates}.");
            }
        }
        public Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"[USER.CREATE]: {Id} logged in.");

            return Task.CompletedTask;
        }

        protected override Task OnDeactivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"[USER.DELETE]: {Id} logged out.");
            return base.OnDeactivateAsync();
        }
    }
}
