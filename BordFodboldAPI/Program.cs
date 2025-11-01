using BordFodboldAPI;
using BordFodboldAPI.Exceptions;
using BordFodboldAPI.Objects;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<BordFodboldDbContext>(options =>
            options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));
        var app = builder.Build();

        // Create new player and save it to database
        // Take a JSON body with Name, Initials and optionally Handicap
        app.MapPost("/CreatePlayer", async (BordFodboldDbContext db, CreatePlayerDto dto) =>
        {
            var player = new Player(dto.Name, dto.Initials, dto.Handicap);
            db.Players.Add(player);
            await db.SaveChangesAsync();
            return player;
        });


        // Edit existing player but do not change handicap
        // Could also have been a PUT request
        app.MapPost("/EditPlayer/{id}", async (BordFodboldDbContext db, int id, CreatePlayerDto dto) =>
        {
            var player = await db.Players.FindAsync(id);
            if (player is null)
            {
                return Results.NotFound();
            }
            player.Name = dto.Name;
            player.Initials = dto.Initials;
            await db.SaveChangesAsync();
            return Results.Ok(player);
        });

        // Delete player by ID
        app.MapDelete("/DeletePlayer/{id}", async (BordFodboldDbContext db, int id) =>
        {
            var player = await db.Players.FindAsync(id);
            if (player is null)
            {
                return Results.NotFound();
            }
            db.Players.Remove(player);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        // Get all players sorted by handicap descending and then by name ascending in case of same handicap value
        app.MapGet("/GetPlayers", async (BordFodboldDbContext db) =>
        {
            var players = await db.Players.ToListAsync();
            var sorted = players.OrderByDescending(p => p.Handicap).ThenBy(p => p.Name).ToList();
            return Results.Ok(sorted);
        });

        // Search players by name or initials containing the search term
        // Use LINQ to perform the search
        app.MapGet("/SearchPlayers/{searchTerm}", async (BordFodboldDbContext db, string searchTerm) =>
        {
            var players = await db.Players
                .Where(p => p.Name.Contains(searchTerm) || p.Initials.Contains(searchTerm))
                .ToListAsync();
            var sorted = players.OrderByDescending(p => p.Handicap).ThenBy(p => p.Name).ToList();
            return Results.Ok(sorted);
        });

        // Create a new match by specifying player IDs for both teams and optionally initial scores
        app.MapPost("/CreateMatch", async (BordFodboldDbContext db, CreateMatchDto dto) =>
        {
            var team1Players = await db.Players.Where(p => dto.Team1PlayerIds.Contains(p.Id)).ToListAsync();
            var team2Players = await db.Players.Where(p => dto.Team2PlayerIds.Contains(p.Id)).ToListAsync();
            try
            {
                var match = new Match(team1Players, team2Players, dto.Team1Score, dto.Team2Score);
                db.Matches.Add(match);
                await db.SaveChangesAsync();
                return Results.Ok(match);
            }
            catch (TooManyPlayersException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (PlayerOnBothTeamsException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // Add match result by specifying match ID and winning team ID (1 or 2)
        app.MapPost("/AddMatchResult/{matchId}/{winningTeamId}", async (BordFodboldDbContext db, int matchId, int winningTeamId) =>
        {
            var match = await db.Matches.FindAsync(matchId);
            if (match is null)
            {
                return Results.NotFound();
            }
            // Could probably be done in a single query
            var team1Players = await db.Players.Where(p => match.Team1.Select(tp => tp.Id).Contains(p.Id)).ToListAsync();
            var team2Players = await db.Players.Where(p => match.Team2.Select(tp => tp.Id).Contains(p.Id)).ToListAsync();
            switch (winningTeamId)
            {
                case 1:
                    match.Team1Score += 1;
                    foreach (var player in team1Players)
                    {
                        player.Handicap += 1;
                    }
                    foreach (var player in team2Players)
                    {
                        player.Handicap -= 1;
                    }
                    break;
                case 2:
                    match.Team2Score += 1;
                    foreach (var player in team2Players)
                    {
                        player.Handicap += 1;
                    }
                    foreach (var player in team1Players)
                    {
                        player.Handicap -= 1;
                    }
                    break;
                default:
                    return Results.BadRequest("Invalid team ID. Must be 1 or 2.");
            }
            await db.SaveChangesAsync();
            return Results.Ok(match);
        });

        app.Run();
    }
}