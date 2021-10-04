using Common.API;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using PlayerCollection.Model;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserActor.Interfaces;
using UserOrchestrator.Interfaces;

namespace UserOrchestrator
{
    public static class Constants
    {
        public const int MinInstanceCount = 1;
        public const int MaxInstanceCount = 5;
        public const int UpperLoadTreshold = 100;
        public const int LowerLoadTreshold = 50;
        public const string ServiceLoadMetricName = "ActivePlayers";
        public const int ScaleIncrement = 1;
        public static readonly TimeSpan ScaleInterval = TimeSpan.FromSeconds(60);
        public static readonly TimeSpan ReportingInterval = TimeSpan.FromSeconds(30);
    }

    public sealed class UserOrchestrator : StatelessService, IUserOrchestrator
    {
        public static IPlayerCollectionService _service;

        public UserOrchestrator(StatelessServiceContext context)
            : base(context)
        {
            var proxyFactory = new ServiceProxyFactory(
                c => new FabricTransportServiceRemotingClientFactory());

            _service = proxyFactory.CreateServiceProxy<IPlayerCollectionService>(
                new Uri("fabric:/OnboardingApplication/PlayerCollection"),
                new ServicePartitionKey(0L));
        }

        public async Task<IEnumerable<PlayerAPI>> GetPlayersAsync(CancellationToken cancellationToken = default)
        {
            List<Player> allPlayers = new List<Player>();
            List<Int64> partitionsLowKey = await _service.GetPartitionsLowKey(cancellationToken);

            foreach (Int64 key in partitionsLowKey)
            {
                var proxy = GetPlayerServiceProxy(key);
                var players = await proxy.GetAllPlayersAsync();
                allPlayers.AddRange(players);
            }

            return allPlayers.Select(p => new PlayerAPI()
            {
                Id = p.Id.ToString(),
                Username = p.Username,
                HP = p.HP,
                AD = p.AD,
                State = Common.Library.StatusToString(p.State),
                NumberOfFights = p.NumberOfFights,
                Coordinates = new CoordAPI() { X = p.Coordinates.X, Y = p.Coordinates.Y }
            });
        }

        public async Task<PlayerAPI> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IUserActor actor = GetActor(playerId);
            var p = await actor.GetPlayerAsync(cancellationToken);

            if (p == null) return null;

            var playersApi = new PlayerAPI()
            {
                Id = p.Id.ToString(),
                Username = p.Username,
                HP = p.HP,
                AD = p.AD,
                State = Common.Library.StatusToString(p.State),
                NumberOfFights = p.NumberOfFights,
                Coordinates = new CoordAPI() { X = p.Coordinates.X, Y = p.Coordinates.Y }
            };

            return playersApi;
        }

        public async Task MovePlayersAsync(CancellationToken cancellationToken = default)
        {
            var allPlayers = await GetPlayersAsync(cancellationToken);

            Parallel.ForEach(allPlayers, async p =>
            {
                IUserActor actor = GetActor(Guid.Parse(p.Id));
                await actor.MovePlayerAsync();
            });
        }

        public async Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IUserActor actor = GetActor(playerId);

            await actor.MovePlayerAsync(cancellationToken);
        }

        public async Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IUserActor actor = GetActor(playerId);

            IActorService userActorServiceProxy = ActorServiceProxy.Create(
                new Uri("fabric:/OnboardingApplication/UserActorService"),
                actor.GetActorId());

            await userActorServiceProxy.DeleteActorAsync(actor.GetActorId(), CancellationToken.None);
        }

        public async Task CreatePlayerAsync(CancellationToken cancellationToken = default)
        {
            Player player = new Player().Init();
            IUserActor actor = GetActor(player.Id);

            await actor.AddPlayerAsync(player, cancellationToken);
        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task CreatePlayersAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    CreatePlayerAsync(cancellationToken);
                }
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        public async Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            IUserActor actor = GetActor(player.Id);
            await actor.UpdatePlayerAsync(player, cancellationToken);
        }

        public async Task<Int64> GetActivePlayersCountAsync(CancellationToken cancellationToken = default)
        {
            var players = await GetPlayersAsync(cancellationToken);
            return players.LongCount();
        }

        public static IPlayerCollectionService GetPlayerServiceProxy(long partitionKey)
        {
            var proxyFactory = new ServiceProxyFactory(
                c => new FabricTransportServiceRemotingClientFactory());

            return proxyFactory.CreateServiceProxy<IPlayerCollectionService>(
                new Uri("fabric:/OnboardingApplication/PlayerCollection"),
                new ServicePartitionKey(partitionKey));
        }

        public static IUserActor GetActor(Guid userId)
        {
            return ActorProxy.Create<IUserActor>(
                new ActorId(userId),
                new Uri("fabric:/OnboardingApplication/UserActorService"));
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await DefineMetricsAndPoliciesAsync();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Int64 count = await GetActivePlayersCountAsync(cancellationToken);

                ServiceEventSource.Current.ServiceMessage(Context, $"[ActivePlayersCount]: {count}, reported at {DateTime.Now}");
                Partition.ReportLoad(new List<LoadMetric> { new LoadMetric(Constants.ServiceLoadMetricName, (int)count) });

                await Task.Delay(Constants.ReportingInterval, cancellationToken);
            }
        }

        private async Task DefineMetricsAndPoliciesAsync()
        {
            var fabricClient = new FabricClient();
            var appName = new Uri($"{Context.CodePackageActivationContext.ApplicationName}/UserOrchestrator");

            var serviceUpdateDescription = new StatelessServiceUpdateDescription();

            var serviceLoadMetricDescription = new StatelessServiceLoadMetricDescription
            {
                Name = Constants.ServiceLoadMetricName,
                DefaultLoad = 0,
                Weight = ServiceLoadMetricWeight.High
            };

            if (serviceUpdateDescription.Metrics == null)
                serviceUpdateDescription.Metrics = new ServiceMetric();
            serviceUpdateDescription.Metrics.Add(serviceLoadMetricDescription);

            var scaleMechanism = new PartitionInstanceCountScaleMechanism
            {
                MinInstanceCount = Constants.MinInstanceCount,
                MaxInstanceCount = Constants.MaxInstanceCount,
                ScaleIncrement = Constants.ScaleIncrement
            };

            var scalingTrigger = new AveragePartitionLoadScalingTrigger
            {
                MetricName = Constants.ServiceLoadMetricName,
                LowerLoadThreshold = Constants.LowerLoadTreshold,
                UpperLoadThreshold = Constants.UpperLoadTreshold,
                ScaleInterval = Constants.ScaleInterval
            };

            var scalingPolicy = new ScalingPolicyDescription(scaleMechanism, scalingTrigger);

            if (serviceUpdateDescription.ScalingPolicies == null)
                serviceUpdateDescription.ScalingPolicies = new List<ScalingPolicyDescription>();
            serviceUpdateDescription.ScalingPolicies.Add(scalingPolicy);

            await fabricClient.ServiceManager.UpdateServiceAsync(appName, serviceUpdateDescription);
        }
    }
}
