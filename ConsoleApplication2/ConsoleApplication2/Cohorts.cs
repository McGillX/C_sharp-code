using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class CohortEvent
    {
        [DataMember(Name = "cohort_id")]
        public int cohort_id { get; set; }
        [DataMember(Name = "cohort_name")]
        public string cohortName { get; set; }

        [DataMember(Name = "user_id")]
        public string uID { get; set; }

        public override string ToString()
        {
            return cohortName + " " + cohort_id;
        }
    }
    [DataContract]
    public class CohortLog
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }
        
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        [DataMember(Name ="event")]
        public CohortEvent cEvent { get; set; }


        public override string ToString()
        {
            return context.ToString() + " " + event_type + " " + time + " " + cEvent;
        }
    }

   
}
