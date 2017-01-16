using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class BasicTeamEvent
    {
        [DataMember(Name = "team_id")]
        public string team_id;
    }

    [DataContract]
    public class BasicTeam
    {
        [DataMember(Name = "context")]
        public Context teamContext;
        [DataMember(Name = "event")]
        public BasicTeamEvent teamEvent;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        public override string ToString()
        {
            return event_type + " " + teamEvent.team_id + " " + teamContext.courseID;
        }
    }
}
