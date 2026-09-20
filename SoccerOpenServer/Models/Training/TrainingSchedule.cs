using SoccerOpenServer.Models.People;
using System.ComponentModel.DataAnnotations;

namespace SoccerOpenServer.Models.Training
{
    public class TrainingSchedule
    {
        public Guid TrainingScheduleID { get; set; }
        public Guid TeamID { get; set; }
        public TrainingScheduleLevel TrainingScheduleLevel { get; set; }
        public string ScheduleName { get; set; } = "Training Schedule 1";
        [Range(0, 100)]
        public byte AttackPoints { get; set; }
        [Range(0, 100)]
        public byte DefendPoints { get; set; }
        [Range(0, 100)]
        public byte ControlPoints { get; set; }
        [Range(0, 100)]
        public byte GoalkeeperPoints { get; set; }
        [Range(0, 100)]
        public byte TacticPoints { get; set; }
        [Range(0, 100)]
        public byte FitnessPoints { get; set; }
        public Guid? CoachID { get; set; }
        public ICollection<Person> Persons { get; set; } = new List<Person>();
    }
}
