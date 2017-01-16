using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class PollEvents
    {
        [DataMember(Name ="url_name")]
        public string urlName;
        [DataMember(Name ="choice")]
        public string choice;

        public override string ToString()
        {
            return urlName + " " + choice + "\n";
        }

    }

    [DataContract]
    public class PollContext
    {
        [DataMember(Name = "course_id")]
        public string courseID { get; set; }
        [DataMember(Name = "user_id")]
        public string userID { get; set; }
        [DataMember(Name = "path")]
        public string path { get; set; }
        [DataMember(Name = "module")]
        public PollModule module;

        public override string ToString()
        {
            return courseID + " " + userID;
        }
    }

    [DataContract]
    public class PollModule
    {
        [DataMember(Name = "display_name")]
        public string displayName;
        [DataMember(Name = "usage_key")]
        public string key;
    }



    [DataContract]
    public class PollSubmit
    {
        [DataMember(Name = "event")]
        public PollEvents pollEvent;
        [DataMember(Name = "context")]
        public PollContext context;
        [DataMember(Name = "time")]
        public string time { get; set; }

        public override string ToString()
        {
            return pollEvent.ToString() + " " + context.ToString() + "\n";
        }


    }
}
