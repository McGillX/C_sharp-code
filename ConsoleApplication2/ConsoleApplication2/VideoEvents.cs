using System.Runtime.Serialization;
namespace ConsoleApplication2
{
    [DataContract]
    public class VideoSeekEvent
    {
        [DataMember(Name = "new_time")]
        public float newTime;
        [DataMember(Name = "old_time")]
        public float oldTime;
        [DataMember(Name = "code")]
        public string code;
        [DataMember(Name = "id")]
        public string id;
    }

    [DataContract]
    public class VideoSeek
    {
        [DataMember(Name = "context")]
        public Context vidContext;
        [DataMember(Name = "event")]
        public VideoSeekEvent vidSeekEvent;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
    }

    [DataContract]
    public class VideoSpeedEvent
    {
        [DataMember(Name = "code")]
        public string code;
        [DataMember(Name = "id")]
        public string id;
        [DataMember(Name = "current_time")]
        public float curTime;
        [DataMember(Name = "old_speed")]
        public string oldSpeed;
        [DataMember(Name = "new_speed")]
        public string newSpeed;
    }

    [DataContract]
    public class VideoSpeed
    {
        [DataMember(Name = "context")]
        public Context vidContext;
        [DataMember(Name = "event")]
        public VideoSpeedEvent vidSpeedEvent;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
    }

    [DataContract]
    public class VideoLoadEvent
    {
        [DataMember(Name = "code")]
        public string code;
        [DataMember(Name = "id")]
        public string id;
    }

    [DataContract]
    public class VideoLoad
    {
        [DataMember(Name = "context")]
        public Context vidContext;
        [DataMember(Name = "event")]
        public VideoLoadEvent vidLoadEvent;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
    }

    [DataContract]
    public class VideoOtherEvent
    {
        [DataMember(Name = "code")]
        public string code;
        [DataMember(Name = "id")]
        public string id;
        [DataMember(Name = "currentTime")]
        public double curTime;
    }

    [DataContract]
    public class VideoOther
    {
        [DataMember(Name = "context")]
        public Context vidContext;
        [DataMember(Name = "event")]
        public VideoOtherEvent vidOtherEvent;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
    }
}