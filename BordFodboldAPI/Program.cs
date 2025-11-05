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

        Dictionary<int, string> userTokens = new Dictionary<int, string>();

        // Tables might not exist, so we ensure they are created
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BordFodboldDbContext>();
            db.Database.EnsureCreated();
        }

        // Create new player and save it to database
        // Take a JSON body with Name, Initials and optionally Handicap
        app.MapPost("/CreatePlayer", async (BordFodboldDbContext db, CreatePlayerDto dto) =>
        {
            var player = new Player(dto.Name, dto.Initials, dto.Handicap);
            db.Players.Add(player);
            await db.SaveChangesAsync();
            return Results.Ok(player);
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

        // Create a new match by specifying player IDs for both teams
        app.MapPost("/CreateMatch", async (BordFodboldDbContext db, CreateMatchDto dto) =>
        {
            var team1Players = await db.Players.Where(p => dto.Team1PlayerIds.Contains(p.Id)).ToListAsync();
            var team2Players = await db.Players.Where(p => dto.Team2PlayerIds.Contains(p.Id)).ToListAsync();
            try
            {
                var match = Match.CreateMatch(team1Players, team2Players);
                db.Matches.Add(match);
                await db.SaveChangesAsync();
                return Results.Ok(match);
            }
            catch (InvalidTeamSizeException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (PlayerOnBothTeamsException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        // Add match result by specifying match ID and winning team ID (1 or 2)
        // This could also be a PUT request
        app.MapPost("/AddMatchResult/{matchId}/{winningTeamId}", async (BordFodboldDbContext db, int matchId, int winningTeamId) =>
        {
            var match = await db.Matches.FindAsync(matchId);
            if (match is null)
            {
                return Results.NotFound();
            }
            // Could probably be done in a single query
            var team1Players = await db.Players.Where(p => p.Id == match.Team1Player1Id || p.Id == match.Team1Player2Id).ToListAsync();
            var team2Players = await db.Players.Where(p => p.Id == match.Team2Player1Id || p.Id == match.Team2Player2Id).ToListAsync();
            switch (winningTeamId)
            {
                case 1:
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
            return Results.Ok(team1Players.Concat(team2Players));
        });

        app.MapPost("/Register", async (BordFodboldDbContext db, CreateUserDto dto) =>
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (existingUser != null)
            {
                return Results.BadRequest("User already exists");
            }
            var user = User.CreateUser(dto.UserName, dto.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok(user.UserName);
        });

        app.MapPost("/Login", async (BordFodboldDbContext db, CreateUserDto dto) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null || !User.VerifyPassword(dto.Password, user.Password))
            {
                return Results.Unauthorized();
            }
            var token = Guid.NewGuid().ToString();
            userTokens[user.Id] = token;
            return Results.Ok(new { Token = token });
        });

        app.Run();
    }
}