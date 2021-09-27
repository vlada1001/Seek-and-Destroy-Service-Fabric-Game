using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers;
using WebAPI.Models;
using static Common.Library;

namespace Game
{
    public sealed class Game : StatelessService
    {
        public static HttpClient client = new HttpClient();

        public Game(StatelessServiceContext context)
            : base(context)
        {
            client.BaseAddress = new Uri("http://localhost:7890/api/players/");
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return Array.Empty<ServiceInstanceListener>();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            Random rand = new Random();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<PlayerAPI> players;
                HashSet<PlayerAPI> removedPlayers = new HashSet<PlayerAPI>();

                await MovePlayers(client, cancellationToken);
                HttpResponseMessage response = await GetPlayers(client, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var playersResponseJson = response.Content.ReadAsStringAsync(cancellationToken).Result;
                    players = JsonConvert.DeserializeObject<List<PlayerAPI>>(playersResponseJson);

                    foreach (PlayerAPI p1 in players)
                    {
                        if (removedPlayers.Contains(p1))
                            continue;

                        var actor1 = PlayersController.GetActor(Guid.Parse(p1.Id));

                        // if player1 is exploring the map, look for fight
                        // if it's in fight, decide the winner
                        if (p1.State == StatusToString(Status.Exploring))
                        {
                            foreach (PlayerAPI p2 in players)
                            {
                                if (removedPlayers.Contains(p2))
                                    continue;

                                if (!p1.Equals(p2))
                                {
                                    double distance = Math.Sqrt(p1.Coordinates.X * p2.Coordinates.X + p1.Coordinates.Y * p2.Coordinates.Y);
                                    if (p1.State == StatusToString(Status.Exploring)
                                        && p2.State == StatusToString(Status.Exploring)
                                        && distance < 10)
                                    {
                                        if (rand.NextDouble() > 0.66)
                                        {
                                            double luck1 = rand.Next(0, 30) * 1e-2;
                                            double luck2 = rand.Next(0, 30) * 1e-2;

                                            double probToWinPlayer1 = 0.02 * (p1.NumberOfFights + 1)
                                                + 0.01 * p1.AD + 0.001 * p1.HP;
                                            double probToWinPlayer2 = 0.02 * (p2.NumberOfFights + 1)
                                                + 0.01 * p2.AD + 0.001 * p2.HP;

                                            if (rand.NextDouble() > 0.7)
                                                probToWinPlayer1 += luck1;
                                            if (rand.NextDouble() > 0.7)
                                                probToWinPlayer2 += luck2;

                                            if (probToWinPlayer1 > probToWinPlayer2) // player1 wins
                                            {
                                                if (luck1 > luck2)
                                                    ServiceEventSource.Current.ServiceMessage(Context,
                                                        $"[GAME]: {p1.Username} defeated {p2.Username} by accident!");
                                                else
                                                    ServiceEventSource.Current.ServiceMessage(Context,
                                                        $"[GAME]: {p1.Username} defeated {p2.Username}!");

                                                removedPlayers.Add(p2);
                                                await DeletePlayer(client, Guid.Parse(p2.Id), cancellationToken);
                                            }
                                            else // player2 wins
                                            {
                                                if (luck2 > luck1)
                                                    ServiceEventSource.Current.ServiceMessage(Context,
                                                        $"[GAME]: {p2.Username} defeated {p1.Username} by accident!");
                                                else
                                                    ServiceEventSource.Current.ServiceMessage(Context,
                                                        $"[GAME]: {p2.Username} defeated {p1.Username}!");

                                                removedPlayers.Add(p1);
                                                await DeletePlayer(client, Guid.Parse(p1.Id), cancellationToken);
                                            }
                                        }
                                        else
                                            ServiceEventSource.Current.ServiceMessage(Context,
                                                $"[GAME]: {p1.Username} and {p2.Username} can fight, but they flee...");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        protected async Task<HttpResponseMessage> GetPlayers(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync("", cancellationToken);
        }

        protected async Task<HttpResponseMessage> GetPlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"{playerId}", cancellationToken);
        }

        protected async Task<HttpResponseMessage> MovePlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"movePlayer/{playerId}", cancellationToken);
        }

        protected async Task<HttpResponseMessage> MovePlayers(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"movePlayers", cancellationToken);
        }

        protected async Task<HttpResponseMessage> DeletePlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.DeleteAsync($"deletePlayer/{playerId}", cancellationToken);
        }

        protected async Task<HttpResponseMessage> CreatePlayer(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.PutAsync($"createPlayer", null, cancellationToken);
        }
    }
}
