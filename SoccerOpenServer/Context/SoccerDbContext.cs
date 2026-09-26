// Copyright (c) 2026 Tom Papaioannou. All rights reserved.
// Licensed under the MIT License

using SoccerOpenServer.Models.Competitions;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Servers;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Models.Training;
using SoccerOpenServer.Models.Users;
using SoccerOpenServer.Models.World;
using Microsoft.EntityFrameworkCore;

public class SoccerDbContext : DbContext
{
    public DbSet<Person> People { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Tactic> Tactics { get; set; }
    public DbSet<PlayerTactic> PlayerTactics { get; set; }
    public DbSet<PlayerTrainedPosition> PlayerTrainedPositions { get; set; }
    public DbSet<PlayerTrainedRole> PlayerTrainedRoles { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AppUserClaim> AppUserClaims { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
    public DbSet<PlayerStats> PlayerStats { get; set; }
    public DbSet<CoachStats> CoachStats { get; set; }
    public DbSet<MedicStats> MedicStats { get; set; }
    public DbSet<Nation> Nations { get; set; }
    public DbSet<Continent> Continents { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<Kit> Kits { get; set; }
    public DbSet<CompetitionTable> CompetitionTables { get; set; }
    public DbSet<PersonHealthAndFitness> PersonHealthAndFitnesses { get; set; }
    public DbSet<CupRound> CupRounds { get; set; }
    public DbSet<CupTie> CupTies { get; set; }
    public DbSet<ManagerGameStats> ManagerGameStatsTable { get; set; }
    public DbSet<ManagerFormationPicked> ManagerFormationPickedTable { get; set; }
    public DbSet<PlayerUnavailability> PlayerUnavailabilities { get; set; }
    public DbSet<PlayerCompetitionDiscipline> PlayerCompetitionDisciplines { get; set; }
    public DbSet<TeamTacticPriority> TeamTacticPriorities { get; set; }
    public DbSet<TrainingSchedule> TrainingSchedules { get; set; }
    public SoccerDbContext(DbContextOptions<SoccerDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.Claims)
            .WithOne(c => c.AppUser)
            .HasForeignKey(c => c.AppUserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Email)
            .HasMaxLength(256);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<AppUserClaim>()
            .Property(c => c.Type)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<AppUserClaim>()
            .Property(c => c.Value)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.AppUser)
            .WithMany()
            .HasForeignKey(rt => rt.AppUserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Person)
            .WithOne(p => p.AppUser)
            .HasForeignKey<AppUser>(u => u.PersonID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PlayerTactic>()
            .HasOne(pt => pt.Person)
            .WithMany(p => p.PlayerTactics)
            .HasForeignKey(pt => pt.PersonID);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Person)
            .WithMany(p => p.Contracts)
            .HasForeignKey(c => c.PersonID);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Team)
            .WithMany(t => t.Contracts)
            .HasForeignKey(c => c.TeamID);

        modelBuilder.Entity<PlayerStats>()
            .HasIndex(ps => ps.PersonID)
            .IsUnique();

        modelBuilder.Entity<PlayerStats>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_PlayerStats_LegRatings",
                "(([RightLegRating] >= 85 AND [LeftLegRating] >= 25 AND [LeftLegRating] < [RightLegRating]) OR ([LeftLegRating] >= 85 AND [RightLegRating] >= 25 AND [RightLegRating] < [LeftLegRating]))"));

        modelBuilder.Entity<CoachStats>()
            .HasOne(cs => cs.Person)
            .WithOne(p => p.CoachStats)
            .HasForeignKey<CoachStats>(cs => cs.PersonID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CoachStats>()
            .HasIndex(cs => cs.PersonID)
            .IsUnique();

        modelBuilder.Entity<CoachStats>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_CoachStats_Attack", "[Attack] >= 1 AND [Attack] <= 100");
                table.HasCheckConstraint("CK_CoachStats_Defend", "[Defend] >= 1 AND [Defend] <= 100");
                table.HasCheckConstraint("CK_CoachStats_Control", "[Control] >= 1 AND [Control] <= 100");
                table.HasCheckConstraint("CK_CoachStats_Goalkeeper", "[Goalkeeper] >= 1 AND [Goalkeeper] <= 100");
                table.HasCheckConstraint("CK_CoachStats_Tactic", "[Tactic] >= 1 AND [Tactic] <= 100");
                table.HasCheckConstraint("CK_CoachStats_Fitness", "[Fitness] >= 1 AND [Fitness] <= 100");
            });

        modelBuilder.Entity<MedicStats>()
            .HasOne(ms => ms.Person)
            .WithOne(p => p.MedicStats)
            .HasForeignKey<MedicStats>(ms => ms.PersonID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedicStats>()
            .HasIndex(ms => ms.PersonID)
            .IsUnique();

        modelBuilder.Entity<MedicStats>()
            .ToTable(table =>
            {
                table.HasCheckConstraint("CK_MedicStats_Diagnosis", "[Diagnosis] >= 1 AND [Diagnosis] <= 100");
                table.HasCheckConstraint("CK_MedicStats_Treatment", "[Treatment] >= 1 AND [Treatment] <= 100");
                table.HasCheckConstraint("CK_MedicStats_Rehabilitation", "[Rehabilitation] >= 1 AND [Rehabilitation] <= 100");
                table.HasCheckConstraint("CK_MedicStats_Prevention", "[Prevention] >= 1 AND [Prevention] <= 100");
            });

        modelBuilder.Entity<Person>()
            .HasOne(p => p.Nation)
            .WithMany()
            .HasForeignKey(p => p.NationID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Continent>()
            .HasMany(c => c.Nations)
            .WithOne(n => n.Continent)
            .HasForeignKey(n => n.ContinentID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Server>()
            .HasMany(s => s.Persons)
            .WithOne(p => p.Server)
            .HasForeignKey(s => s.ServerID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Server>()
            .HasMany(s => s.Competitions)
            .WithOne(p => p.Server)
            .HasForeignKey(s => s.ServerID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.Server)
            .WithMany(s => s.Persons)
            .HasForeignKey(p => p.ServerID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Competition>()
            .HasOne(c => c.Server)
            .WithMany(s => s.Competitions)
            .HasForeignKey(c => c.ServerID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.AppUser)
            .WithMany()
            .HasForeignKey(t => t.AppUserID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.AppUserID)
            .IsUnique()
            .HasFilter("[AppUserID] IS NOT NULL");

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Stadium)
            .WithOne(s => s.Team)
            .HasForeignKey<Team>(t => t.StadiumID);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Kit)
            .WithOne(k => k.Team)
            .HasForeignKey<Team>(t => t.KitID);

        modelBuilder.Entity<Person>()
            .HasOne(p => p.HealthAndFitness)
            .WithOne(h => h.Person)
            .HasForeignKey<PersonHealthAndFitness>(h => h.PersonID)
            .IsRequired();

        modelBuilder.Entity<CupRound>()
            .HasKey(x => x.CupRoundID);

        modelBuilder.Entity<CupRound>()
            .HasOne(x => x.Competition)
            .WithMany()
            .HasForeignKey(x => x.CompetitionID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CupTie>()
            .HasKey(x => x.CupTieID);

        modelBuilder.Entity<CupTie>()
            .HasOne(x => x.CupRound)
            .WithMany(x => x.Ties)
            .HasForeignKey(x => x.CupRoundID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CupTie>()
            .HasOne<CupTie>()
            .WithMany()
            .HasForeignKey(x => x.NextCupTieID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CupRound>()
            .HasIndex(x => new { x.CompetitionID, x.RoundNumber })
            .IsUnique();

        modelBuilder.Entity<CupTie>()
            .HasIndex(x => new { x.CupRoundID, x.TieNumber })
            .IsUnique();

        modelBuilder.Entity<ManagerGameStats>(entity =>
        {
            entity.HasKey(x => x.PersonID);

            entity.HasOne(x => x.Person)
                .WithOne(x => x.ManagerGameStats)
                .HasForeignKey<ManagerGameStats>(x => x.PersonID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManagerFormationPicked>(entity =>
        {
            entity.HasKey(x => x.ManagerFormationPickedID);

            entity.HasOne(x => x.Person)
                .WithMany(x => x.ManagerFormationsPicked)
                .HasForeignKey(x => x.PersonID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new
            {
                x.PersonID,
                x.Formation
            })
            .IsUnique();
        });

        modelBuilder.Entity<PlayerCompetitionDiscipline>()
            .HasKey(x => new
            {
                x.PersonID,
                x.CompetitionID
            });

        modelBuilder.Entity<PlayerUnavailability>()
            .ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerUnavailability_MatchesRemaining",
                    "[MatchesRemaining] > 0");
            });

        modelBuilder.Entity<PlayerCompetitionDiscipline>()
            .ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_PlayerCompetitionDiscipline_YellowCards",
                    "[YellowCards] >= 0 AND [YellowCards] < 3");
            });

        modelBuilder.Entity<TeamTacticPriority>()
            .HasKey(x => x.TeamTacticPriorityID);

        modelBuilder.Entity<TeamTacticPriority>()
            .HasOne(x => x.Team)
            .WithMany(x => x.TacticPriorities)
            .HasForeignKey(x => x.TeamID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeamTacticPriority>()
            .HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeamTacticPriority>()
            .HasIndex(x => new
            {
                x.TeamID,
                x.Type,
                x.PersonID
            })
            .IsUnique();

        modelBuilder.Entity<TeamTacticPriority>()
            .HasIndex(x => new
            {
                x.TeamID,
                x.Type,
                x.Priority
            })
            .IsUnique();

        modelBuilder.Entity<TeamTacticPriority>().ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_TeamTacticPriority_Priority",
                    "[Priority] >= 1");
            });

        modelBuilder.Entity<Person>()
            .HasOne(p => p.TrainingSchedule)
            .WithMany(t => t.Persons)
            .HasForeignKey(p => p.TrainingScheduleID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
