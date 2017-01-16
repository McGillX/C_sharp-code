using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ConsoleApplication2
{

    [DataContract]
    public class BaseCourseModule
    {
        public string id;
        public string parentID;
        [DataMember(Name = "category")]
        public string category;
        [DataMember(Name = "children")]
        public string[] children;
        [DataMember(Name = "metadata")]
        public Metadata mData;

        public override string ToString()
        {
            string s = id + " " + category + " Number of children: " + children.Count();
            s += mData.ToString();
            return s;
        }
    }

    [DataContract]
    public class Metadata
    {
        //for most.
        [DataMember(Name = "display_name")]
        public string displayName;
        [DataMember(Name = "start")]
        public string start;
        [DataMember(Name = "end")]
        public string end;

        //for sequential
        [DataMember(Name = "due")]
        public string due;
        [DataMember(Name = "format")]
        public string format;
        [DataMember(Name = "graded")]
        public bool? graded=null;

        public override string ToString()
        {
            return displayName + " " + start + " " + end + "\n" + due + " " + format + " " + graded;
        }

        //Below are only for 'course' modules.
        /*[DataMember(Name = "advanced_modules")]
        public string[] advancedModules;
        [DataMember(Name = "tabs")]
        public Tab[] tabs;
        [DataMember(Name = "discussion_topics")]
        public Dictionary<string, string> topics; //key is display name, value is id.
        [DataMember(Name = "display_coursenumber")]
        public string coursecode;*/

        //for discussion
        /*[DataMember(Name = "discussion_category")]
        public string discussionCategory;
        [DataMember(Name = "discussion_id")]
        public string discussionID;
        [DataMember(Name = "discussion_target")]
        public string discussionTarget;
        *//*
        //for openassessment
        [DataMember(Name = "submission_due")]
        public string submitDue;
        [DataMember(Name = "submission_start")]
        public string submitStart;*/

    }

    [DataContract]
    public class Tab
    {
        [DataMember(Name = "name")]
        public string name;
        [DataMember(Name = "type")]
        public string type;
    }

}