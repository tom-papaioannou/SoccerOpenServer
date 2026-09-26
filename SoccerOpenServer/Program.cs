// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Servers;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Models.Users;
using SoccerOpenServer.Models.World;
using SoccerOpenServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SoccerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.UseParameterizedCollectionMode(ParameterTranslationMode.Parameter)));


builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<ITeamGenerationService, TeamGenerationService>();
builder.Services.AddScoped<NationalTeamGenerationService>();
builder.Services.AddScoped<ITeamAccessService, TeamAccessService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SoccerDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var teamGenerationService = scope.ServiceProvider.GetRequiredService<ITeamGenerationService>();
    var nationalTeamGenerationService = scope.ServiceProvider.GetRequiredService<NationalTeamGenerationService>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    // optional: ensure DB exists / migrations applied
    // await db.Database.MigrateAsync();

    var adminUsername = config["SeedAdmin:Username"] ?? "admin";
    var adminPassword = config["SeedAdmin:Password"] ?? "ChangeMe123!";
    var hostPassword = config["SeedHost:Password"] ?? "ChangeMe123!";
    var userPassword = config["SeedUser:Password"] ?? "ChangeMe123!";

    var adminExists = await db.AppUsers.AnyAsync(u => u.Username == adminUsername);

    if (!adminExists)
    {
        hasher.CreateHash(adminPassword, out var hash, out var salt);

        var admin = new AppUser
        {
            Username = adminUsername,
            Email = "admin@socceros.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            Claims = new List<AppUserClaim>
            {
                new AppUserClaim { Type = ClaimTypes.Role, Value = "Admin" },
                new AppUserClaim { Type = ClaimTypes.NameIdentifier, Value = adminUsername }
            }
        };

        db.AppUsers.Add(admin);
        await db.SaveChangesAsync();
    }

    // Create Europe
    Continent? europe = await db.Continents.FirstOrDefaultAsync(c => c.Name == "Europe");
    if (europe == null)
    {
        europe = new Continent { ContinentID = Guid.NewGuid(), Name = "Europe", Code = "EUR" };
        db.Continents.Add(europe);
        await db.SaveChangesAsync();
    }

    var europeanNations = new (string Name, string ISO2, string ISO3)[]
    {
        ("Greece",  "GR", "GRE"),
        ("England", "GB", "ENG"),
        ("Italy",   "IT", "ITA"),
        ("France",  "FR", "FRA"),
        ("Germany", "DE", "DEU"),
        ("Netherlands", "NL", "NLD"),
        ("Spain",   "ES", "ESP"),
        ("Portugal", "PT", "PRT")
    };

    var existingNationNames = await db.Nations
        .Where(n => europeanNations.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in europeanNations)
    {
        if (!existingNationNames.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = europe.ContinentID
            });
        }
    }

    // Create North America
    Continent? northAmerica = await db.Continents.FirstOrDefaultAsync(c => c.Name == "North America");
    if (northAmerica == null)
    {
        northAmerica = new Continent { ContinentID = Guid.NewGuid(), Name = "North America", Code = "NAM" };
        db.Continents.Add(northAmerica);
        await db.SaveChangesAsync();
    }

    var nationSeedsNorthAmerica = new (string Name, string ISO2, string ISO3)[]
    {
        ("United States", "US", "USA"),
        ("Canada", "CA", "CAN"),
        ("Mexico", "MX", "MEX")
    };

    var existingNationNamesNorthAmerica = await db.Nations
        .Where(n => nationSeedsNorthAmerica.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in nationSeedsNorthAmerica)
    {
        if (!existingNationNamesNorthAmerica.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = northAmerica.ContinentID
            });
        }
    }

    // Create South America
    Continent? southAmerica = await db.Continents.FirstOrDefaultAsync(c => c.Name == "South America");
    if (southAmerica == null)
    {
        southAmerica = new Continent { ContinentID = Guid.NewGuid(), Name = "South America", Code = "SAM" };
        db.Continents.Add(southAmerica);
        await db.SaveChangesAsync();
    }

    var nationSeedsSouthAmerica = new (string Name, string ISO2, string ISO3)[]
    {
        ("Argentina", "AR", "ARG"),
        ("Brazil", "BR", "BRA")
    };

    var existingNationNamesSouthAmerica = await db.Nations
        .Where(n => nationSeedsSouthAmerica.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in nationSeedsSouthAmerica)
    {
        if (!existingNationNamesSouthAmerica.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = southAmerica.ContinentID
            });
        }
    }

    // Create Oceania
    Continent? oceania = await db.Continents.FirstOrDefaultAsync(c => c.Name == "Oceania");
    if (oceania == null)
    {
        oceania = new Continent { ContinentID = Guid.NewGuid(), Name = "Oceania", Code = "OCE" };
        db.Continents.Add(oceania);
        await db.SaveChangesAsync();
    }

    var nationSeedsOceania = new (string Name, string ISO2, string ISO3)[]
    {
        ("Australia", "AU", "AUS")
    };

    var existingNationNamesOceania = await db.Nations
        .Where(n => nationSeedsOceania.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in nationSeedsOceania)
    {
        if (!existingNationNamesOceania.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = oceania.ContinentID
            });
        }
    }

    // Create Asia
    Continent? asia = await db.Continents.FirstOrDefaultAsync(c => c.Name == "Asia");
    if (asia == null)
    {
        asia = new Continent { ContinentID = Guid.NewGuid(), Name = "Asia", Code = "ASI" };
        db.Continents.Add(asia);
        await db.SaveChangesAsync();
    }

    var nationSeedsAsia = new (string Name, string ISO2, string ISO3)[]
    {
        ("Japan", "JP", "JPN")
    };

    var existingNationNamesAsia = await db.Nations
        .Where(n => nationSeedsAsia.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in nationSeedsAsia)
    {
        if (!existingNationNamesAsia.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = asia.ContinentID
            });
        }
    }

    // Create Africa
    Continent? africa = await db.Continents.FirstOrDefaultAsync(c => c.Name == "Africa");
    if (africa == null)
    {
        africa = new Continent { ContinentID = Guid.NewGuid(), Name = "Africa", Code = "AFR" };
        db.Continents.Add(africa);
        await db.SaveChangesAsync();
    }

    var nationSeedsAfrica = new (string Name, string ISO2, string ISO3)[]
    {
        ("Morocco", "MA", "MAR")
    };

    var existingNationNamesAfrica = await db.Nations
        .Where(n => nationSeedsAfrica.Select(s => s.Name).Contains(n.Name))
        .Select(n => n.Name)
        .ToListAsync();

    foreach (var (name, iso2, iso3) in nationSeedsAfrica)
    {
        if (!existingNationNamesAfrica.Contains(name))
        {
            db.Nations.Add(new Nation
            {
                NationID = Guid.NewGuid(),
                Name = name,
                ISO2 = iso2,
                ISO3 = iso3,
                ContinentID = africa.ContinentID
            });
        }
    }

    await db.SaveChangesAsync();

    // Create default server if none exist
    var serverExists = await db.Servers.AnyAsync();
    if (!serverExists)
    {
        db.Servers.Add(new Server { ServerID = Guid.NewGuid(), Name = "Main" });
        await db.SaveChangesAsync();
    }

    var mainServer = await db.Servers.FirstOrDefaultAsync(s => s.Name == "Main");

    // Create default host user if none exist
    var hostExists = await db.AppUsers.AnyAsync(u => u.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Host"));
    if (!hostExists && mainServer != null)
    {
        hasher.CreateHash(hostPassword, out var hostHash, out var hostSalt);
        Guid hostId = Guid.NewGuid();

        var hostUser = new AppUser
        {
            Id = hostId,
            Username = "host",
            Email = "host@socceros.local",
            PasswordHash = hostHash,
            PasswordSalt = hostSalt,
            Claims = new List<AppUserClaim>
            {
                new AppUserClaim { Type = ClaimTypes.Role, Value = "Host" },
                new AppUserClaim { Type = ClaimTypes.NameIdentifier, Value = hostId.ToString() }
            },
            Person = new Person
            {
                Name = "John",
                Surname = "Doe",
                DateOfBirth = new DateOnly(1970, 1, 1),
                PlaceOfBirth = "Athens",
                ServerID = mainServer.ServerID
            }
        };

        db.AppUsers.Add(hostUser);
        await db.SaveChangesAsync();
    }

    // Ensure default competitions exist
    if (mainServer != null)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var nations = await db.Nations
            .OrderBy(n => n.Name)
            .ToListAsync();
        var generatedTeamIDs = new List<Guid>();
        var leagueSeeds = new (int Priority, string Suffix)[]
        {
            (1, "A"),
            (2, "B")
        };

        foreach (var nation in nations)
        {
            var nationLeagueTeams = new List<Team>();

            foreach (var (priority, suffix) in leagueSeeds)
            {
                var leagueName = $"{nation.Name} League {suffix}";
                var league = await db.Competitions
                    .Include(c => c.Teams)
                    .FirstOrDefaultAsync(c =>
                        c.NationID == nation.NationID &&
                        c.CompetitionType == CompetitionType.League &&
                        c.Priority == priority);

                if (league == null)
                {
                    var generatedTeams = await teamGenerationService.GenerateTeamsForCompetition(mainServer.ServerID, nation.NationID, 16, priority);
                    generatedTeamIDs.AddRange(generatedTeams.Select(t => t.TeamID));

                    Guid competitionID = Guid.NewGuid();

                    league = new Competition
                    {
                        CompetitionID = competitionID,
                        CompetitionName = leagueName,
                        NationID = nation.NationID,
                        CompetitionTeamsType = CompetitionTeamsType.Club,
                        Priority = priority,
                        CompetitionType = CompetitionType.League,
                        Teams = generatedTeams,
                        ServerID = mainServer.ServerID
                    };

                    db.Competitions.Add(league);

                    foreach (var team in generatedTeams)
                    {
                        db.CompetitionTables.Add(new CompetitionTable
                        {
                            CompetitionTableID = Guid.NewGuid(),
                            CompetitionID = competitionID,
                            TeamID = team.TeamID,
                            MatchesPlayed = 0,
                            Wins = 0,
                            Draws = 0,
                            Losses = 0,
                            GoalsFor = 0,
                            GoalsAgainst = 0,
                            YellowCards = 0,
                            RedCards = 0,
                            Points = 0
                        });
                    }
                }

                nationLeagueTeams.AddRange(league.Teams ?? Array.Empty<Team>());
            }

            var cupName = GetDefaultCupName(nation);
            var cupExists = await db.Competitions
                .AnyAsync(c =>
                    c.NationID == nation.NationID &&
                    c.CompetitionType == CompetitionType.Knockout &&
                    c.CompetitionName == cupName);

            if (!cupExists)
            {
                var participatingTeams = nationLeagueTeams
                    .GroupBy(t => t.TeamID)
                    .Select(g => g.First())
                    .OrderBy(t => t.Name)
                    .ThenBy(t => t.TeamID)
                    .ToList();

                CreateDefaultCup(db, mainServer.ServerID, nation, participatingTeams);
            }
        }

        await db.SaveChangesAsync();
        await teamGenerationService.AssignPlayersToGeneratedTeams(generatedTeamIDs);
        await db.SaveChangesAsync();
        await nationalTeamGenerationService.EnsureNationalSquadsAsync();
        await transaction.CommitAsync();
    }

    // Create default regular user if none exist
    var userExists = await db.AppUsers.AnyAsync(u => u.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "User"));
    if (!userExists && mainServer != null)
    {
        hasher.CreateHash(userPassword, out var userHash, out var userSalt);
        Guid userId = Guid.NewGuid();

        var regularUser = new AppUser
        {
            Id = userId,
            Username = "user",
            Email = "user@socceros.local",
            PasswordHash = userHash,
            PasswordSalt = userSalt,
            Claims = new List<AppUserClaim>
            {
                new AppUserClaim { Type = ClaimTypes.Role, Value = "User" },
                new AppUserClaim { Type = ClaimTypes.NameIdentifier, Value = userId.ToString() }
            }
        };

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var availableTeams = await db.Teams
            .Where(t => t.AppUserID == null)
            .ToListAsync();

        if (availableTeams.Any())
        {
            var randomTeam = availableTeams[Random.Shared.Next(availableTeams.Count)];
            var existingManager = await db.People
                .FirstOrDefaultAsync(p =>
                    p.StaffRole == StaffRole.Manager &&
                    p.Contracts.Any(c =>
                        c.TeamID == randomTeam.TeamID &&
                        c.Role == Role.Staff &&
                        (c.EndDate == null || c.EndDate > today)));

            if (existingManager != null)
            {
                regularUser.Person = existingManager;
            }
            else
            {
                var person = new Person
                {
                    Name = "John",
                    Surname = "Doe",
                    DateOfBirth = new DateOnly(1970, 1, 1),
                    PlaceOfBirth = "Athens",
                    ServerID = mainServer.ServerID,
                    StaffRole = StaffRole.Manager
                };

                regularUser.Person = person;

                var contract = new Contract
                {
                    Person = person,
                    Team = randomTeam,
                    StartDate = today,
                    EndDate = DateOnly.FromDateTime(now.AddYears(1)),
                    Role = Role.Staff
                };

                db.Contracts.Add(contract);
            }

            randomTeam.AppUserID = userId;
            db.Teams.Update(randomTeam);
        }

        db.AppUsers.Add(regularUser);
        await db.SaveChangesAsync();
    }
}

app.Run();

static string GetDefaultCupName(Nation nation)
{
    return $"{nation.Name} Cup";
}

static void CreateDefaultCup(SoccerDbContext db, Guid serverID, Nation nation, List<Team> participatingTeams)
{
    ValidateCupTeamCount(nation, participatingTeams.Count);

    Guid cupID = Guid.NewGuid();
    var cup = new Competition
    {
        CompetitionID = cupID,
        CompetitionName = GetDefaultCupName(nation),
        NationID = nation.NationID,
        CompetitionTeamsType = CompetitionTeamsType.Club,
        Priority = 1,
        CompetitionType = CompetitionType.Knockout,
        Teams = participatingTeams,
        ServerID = serverID
    };

    var rounds = new List<CupRound>();
    var tiesByRound = new List<List<CupTie>>();

    int teamCount = participatingTeams.Count;
    int roundNumber = 1;

    while (teamCount >= 2)
    {
        var round = new CupRound
        {
            CupRoundID = Guid.NewGuid(),
            CompetitionID = cupID,
            RoundNumber = roundNumber,
            TeamCount = teamCount,
            RoundType = GetCupRoundType(teamCount),
            IsCompleted = false
        };

        var ties = new List<CupTie>();
        int tieCount = teamCount / 2;

        for (int tieNumber = 1; tieNumber <= tieCount; tieNumber++)
        {
            var tie = new CupTie
            {
                CupTieID = Guid.NewGuid(),
                CupRoundID = round.CupRoundID,
                TieNumber = tieNumber,
                WinnerTeamID = null,
                IsCompleted = false
            };

            if (roundNumber == 1)
            {
                int teamIndex = (tieNumber - 1) * 2;
                tie.HomeTeamID = participatingTeams[teamIndex].TeamID;
                tie.AwayTeamID = participatingTeams[teamIndex + 1].TeamID;
            }

            ties.Add(tie);
            round.Ties.Add(tie);
        }

        rounds.Add(round);
        tiesByRound.Add(ties);

        teamCount /= 2;
        roundNumber++;
    }

    for (int roundIndex = 0; roundIndex < tiesByRound.Count - 1; roundIndex++)
    {
        var currentRoundTies = tiesByRound[roundIndex];
        var nextRoundTies = tiesByRound[roundIndex + 1];

        for (int tieIndex = 0; tieIndex < currentRoundTies.Count; tieIndex++)
        {
            var currentTie = currentRoundTies[tieIndex];
            currentTie.NextCupTieID = nextRoundTies[tieIndex / 2].CupTieID;
            currentTie.AdvancesAsHomeTeam = tieIndex % 2 == 0;
        }
    }

    db.Competitions.Add(cup);
    db.CupRounds.AddRange(rounds);
    db.CupTies.AddRange(tiesByRound.SelectMany(ties => ties));
}

static void ValidateCupTeamCount(Nation nation, int teamCount)
{
    if (teamCount < 2)
    {
        throw new InvalidOperationException($"Cannot create the default cup for {nation.Name}: at least 2 league teams are required, but {teamCount} were found.");
    }

    if (!IsPowerOfTwo(teamCount))
    {
        throw new InvalidOperationException($"Cannot create the default cup for {nation.Name}: the league team count must be a power of two, but {teamCount} teams were found.");
    }
}

static bool IsPowerOfTwo(int value)
{
    return (value & (value - 1)) == 0;
}

static CupRoundType GetCupRoundType(int teamCount)
{
    return teamCount switch
    {
        64 => CupRoundType.RoundOf64,
        32 => CupRoundType.RoundOf32,
        16 => CupRoundType.RoundOf16,
        8 => CupRoundType.QuarterFinal,
        4 => CupRoundType.SemiFinal,
        2 => CupRoundType.Final,
        _ => throw new InvalidOperationException($"Cup round with {teamCount} teams is not supported. Add a CupRoundType value before seeding this bracket size.")
    };
}
