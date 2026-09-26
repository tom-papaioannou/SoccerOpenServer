using System;
using System.Linq;
using SoccerOpenServer.Models.Contracts;
using SoccerOpenServer.Models.People;
using SoccerOpenServer.Models.Teams;
using SoccerOpenServer.Services;
using Xunit;

namespace SoccerOpenServer.Tests;

public class NationalSquadTests
{
    [Fact]
    public void SelectUnique_returns_thirty_unique_players_from_eligible_pool()
    {
        var eligible = Enumerable.Range(0, 50).Select(_ => Guid.NewGuid()).ToList();

        var selected = NationalSquadSelector.SelectUnique(eligible, 30, new Random(42));

        Assert.Equal(30, selected.Count);
        Assert.Equal(30, selected.Distinct().Count());
        Assert.All(selected, id => Assert.Contains(id, eligible));
    }

    [Fact]
    public void National_contract_coexists_with_club_contract_for_same_player()
    {
        var player = new Person { PersonID = Guid.NewGuid() };
        var club = new Team { TeamID = Guid.NewGuid(), Code = "CLB" };
        var nation = new Team { TeamID = Guid.NewGuid(), Code = "NAT", IsNationalTeam = true };
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var clubContract = new Contract
        {
            ContractID = Guid.NewGuid(), Person = player, Team = club, PersonID = player.PersonID,
            TeamID = club.TeamID, StartDate = today.AddYears(-1), EndDate = today.AddYears(2), Role = Role.Player
        };
        var nationalContract = new Contract
        {
            ContractID = Guid.NewGuid(), Person = player, Team = nation, PersonID = player.PersonID,
            TeamID = nation.TeamID, StartDate = today, EndDate = today.AddYears(1), Role = Role.Player
        };
        player.Contracts.Add(clubContract);
        player.Contracts.Add(nationalContract);

        Assert.Equal(2, player.Contracts.Count);
        Assert.Contains(player.Contracts, contract => contract.TeamID == club.TeamID);
        Assert.Contains(player.Contracts, contract => contract.TeamID == nation.TeamID && contract.EndDate == today.AddYears(1));
    }
}
