using Microsoft.EntityFrameworkCore;
using WebSocketServer.Core.context;
using WebSocketServer.Server.Repositorys;

namespace WebSocketServerTests;

public class RepositoryTests
{
    private  DuckingContext _duckingContext;
    private IGameHistoryRepository _gameHistoryRepository;
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DuckingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _duckingContext = new DuckingContext(options);
        _gameHistoryRepository = new GameHistoryRepository(_duckingContext);
        //Seed DB 
        var user1 = new DuckingUser
        {
            Id = 1,
            UserName = "alice",
            Password = "password1"
        };

        var user2 = new DuckingUser
        {
            Id = 2,
            UserName = "bob",
            Password = "password2"
        };
        _duckingContext.Users.AddRange(user1, user2);
        _duckingContext.Logins.AddRange(
            new DuckingLogins
            {
                Id = 1,
                UserId = 1,
                LoginTime = new DateTime(2026, 3, 1, 10, 0, 0)
            },
            new DuckingLogins
            {
                Id = 2,
                UserId = 2,
                LoginTime = new DateTime(2026, 3, 2, 11, 30, 0)
            }
        );

        _duckingContext.Games.AddRange(
            new DuckingGame
            {
                Id = 1,
                Type = LobbyType.PlanningPoker, 
                Players = new() { user1 }
            },
            new DuckingGame
            {
                Id = 2,
                Type = LobbyType.DuckRace, 
                Players = new() { user2 }
            },
            new DuckingGame
            {
                Id = 3,
                Type = LobbyType.DuckRace, 
                Players = new() { user1, user2 }
            }
        );

        _duckingContext.SaveChanges();
    }
    [TearDown]
    public void TearDown()
    {
        _duckingContext.Dispose();
    }

    [Test]
    public async Task GetDuckingGameHistoryFromUserId_ShouldReturnGamesForUser2()
    {
        var result = await _gameHistoryRepository.GetDuckingGameHistoryFromUserId(2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(2));
        Assert.That(result.DuckingGames.Count, Is.EqualTo(2));

        Assert.That(result.DuckingGames.Any(g => g.Id == 2), Is.True);
        Assert.That(result.DuckingGames.Any(g => g.Id == 3), Is.True);
    }

    [Test]
    public async Task GetDuckingGameHistoryFromUserId_ShouldReturnGamesForUser1()
    {
        var result = await _gameHistoryRepository.GetDuckingGameHistoryFromUserId(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.DuckingGames, Is.Not.Null);
        Assert.That(result.DuckingGames.Count, Is.EqualTo(2));

        Assert.That(result.DuckingGames.Any(g => g.Id == 1), Is.True);
        Assert.That(result.DuckingGames.Any(g => g.Id == 3), Is.True);
    }
}
