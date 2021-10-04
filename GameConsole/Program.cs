using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Models;
using static Common.Library;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace GameConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:7890/api/players/") };

            await RunAsync(client, CancellationToken.None);
        }

        static async Task RunAsync(HttpClient client, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Random rand = new Random();
                cancellationToken.ThrowIfCancellationRequested();

                List<PlayerAPI> players;
                HashSet<PlayerAPI> removedPlayers = new HashSet<PlayerAPI>();

                await MovePlayers(client, cancellationToken);
                HttpResponseMessage response = await GetPlayers(client, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var playersResponseJson = response.Content.ReadAsStringAsync(cancellationToken).Result;
                    players = JsonConvert.DeserializeObject<List<PlayerAPI>>(playersResponseJson);
                    GameConsoleEventSource.Current.Message($"\t{players.Count} active players");

                    foreach (PlayerAPI p1 in players)
                    {
                        if (removedPlayers.Contains(p1))
                            continue;

                        //var actor1 = PlayersController.GetActor(Guid.Parse(p1.Id));

                        if (p1.State == StatusToString(Status.Exploring))
                        {
                            foreach (PlayerAPI p2 in players)
                            {
                                if (removedPlayers.Contains(p2))
                                    continue;

                                if (!p1.Equals(p2))
                                {
                                    double distance = Distance(p1.Coordinates.X, p2.Coordinates.X, p1.Coordinates.Y, p2.Coordinates.Y);
                                    if (distance < 10
                                        && p1.State == StatusToString(Status.Exploring)
                                        && p2.State == StatusToString(Status.Exploring))
                                    {
                                        if (rand.NextDouble() > 0.66)
                                        {
                                            double luck1 = rand.Next(0, 20) * 1e-2;
                                            double luck2 = rand.Next(0, 20) * 1e-2;

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
                                                    GameConsoleEventSource.Current.Message($"[GAME]: {p1.Username} defeated {p2.Username} by pure luck!");
                                                else
                                                    GameConsoleEventSource.Current.Message($"[GAME]: {p1.Username} defeated {p2.Username}!");

                                                p1.HP += rand.Next(1, 20);
                                                p1.AD += rand.Next(1, 10);
                                                p1.NumberOfFights += 1;
                                                removedPlayers.Add(p2);
                                                DeletePlayer(client, Guid.Parse(p2.Id), cancellationToken);
                                                GameConsoleEventSource.Current.Message($"[USER.DELETE]: {p2.Id} logged out.");
                                                UpdatePlayer(
                                                    Guid.Parse(p1.Id),
                                                    p1.HP,
                                                    p1.AD,
                                                    p1.NumberOfFights,
                                                    client,
                                                    cancellationToken);
                                            }
                                            else // player2 wins
                                            {
                                                if (luck2 > luck1)
                                                    GameConsoleEventSource.Current.Message($"[GAME]: {p2.Username} defeated {p1.Username} by pure luck!");
                                                else
                                                    GameConsoleEventSource.Current.Message($"[GAME]: {p2.Username} defeated {p1.Username}!");

                                                p2.HP += rand.Next(1, 20);
                                                p2.AD += rand.Next(1, 10);
                                                p2.NumberOfFights += 1;
                                                removedPlayers.Add(p1);
                                                DeletePlayer(client, Guid.Parse(p1.Id), cancellationToken);
                                                GameConsoleEventSource.Current.Message($"[USER.DELETE]: {p1.Id} logged out.");
                                                UpdatePlayer(
                                                    Guid.Parse(p2.Id),
                                                    p2.HP,
                                                    p2.AD,
                                                    p2.NumberOfFights,
                                                    client,
                                                    cancellationToken);
                                            }
                                        }
                                        else
                                            GameConsoleEventSource.Current.Message($"[GAME]: {p1.Username} and {p2.Username} can fight, but they flee...");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        static async Task<HttpResponseMessage> GetPlayers(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync("", cancellationToken);
        }

        static async Task<HttpResponseMessage> GetPlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"{playerId}", cancellationToken);
        }

        static async Task<HttpResponseMessage> MovePlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"movePlayer/{playerId}", cancellationToken);
        }

        static async Task<HttpResponseMessage> MovePlayers(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.GetAsync($"movePlayers", cancellationToken);
        }

        static async Task<HttpResponseMessage> DeletePlayer(HttpClient client, Guid playerId, CancellationToken cancellationToken = default)
        {
            return await client.DeleteAsync($"deletePlayer/{playerId}", cancellationToken);
        }

        static async Task<HttpResponseMessage> CreatePlayer(HttpClient client, CancellationToken cancellationToken = default)
        {
            return await client.PutAsync($"createPlayer", null, cancellationToken);
        }

        static async Task<HttpResponseMessage> UpdatePlayer(Guid playerId, int hp, int ad, int numberOfFights,
            HttpClient client, CancellationToken cancellationToken = default)
        {
            string request = $"updatePlayer?playerId={playerId}&hp={hp}&ad={ad}&numberOfFights={numberOfFights}";
            return await client.GetAsync(request, cancellationToken);
        }
    }
}
