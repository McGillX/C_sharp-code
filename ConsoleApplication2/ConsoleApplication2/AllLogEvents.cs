using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class TrackingLogWithEvent
    {
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
        [DataMember(Name = "page")]
        public string page { get; set; }
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event")]
        public Event eventField { get; set; }

        public string toString()
        {
            return eventField.toString() + " " + time + " " + event_type + " " + page;
        }
    }

    [DataContract]
    public class ContextWithModule
    {
        [DataMember(Name = "course_id")]
        public string courseID { get; set; }
        [DataMember(Name = "user_id")]
        public string userID { get; set; }
        [DataMember(Name = "path")]
        public string path { get; set; }
        [DataMember(Name = "module")]
        public Module module { get; set; }

        public override string ToString()
        {
            return courseID + " " + userID + module.displayName;
        }
    }

    [DataContract]
    public class Module
    {
        [DataMember(Name = "usage_key")]
        public string usageKey;
        [DataMember(Name = "display_name")]
        public string displayName;
    }

    [DataContract]
    public class Context
    {
        [DataMember(Name = "course_id")]
        public string courseID { get; set; }
        [DataMember(Name = "user_id")]
        public string userID { get; set; }
        [DataMember(Name = "path")]
        public string path { get; set; }

        public override string ToString()
        {
            return courseID + " " + userID + " " +path;
        }
    }

    [DataContract]
    public class Event
    {
        [DataMember(Name = "user_id")]
        public string userID { get; set; }
        [DataMember(Name = "course_id")]
        public string courseID { get; set; }

        public string toString()
        {
            return courseID + " " + userID;
        }
    }

    [DataContract]
    public class TrackingLogWithContext
    {
        [DataMember(Name = "context")]
        public Context context { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
        [DataMember(Name = "page")]
        public string page { get; set; }
        [DataMember(Name = "time")]
        public string time { get; set; }

        public string toString()
        {
            try
            {
                return context.ToString() + " " + time + " " + event_type + " " + page;
            }
            catch (Exception e)
            {
                return time + " " + event_type + " " + page;
            }
        }
    }
}