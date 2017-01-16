using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public class SearchEvent
    {
        [DataMember(Name = "corrected_text")]
        public string corrected_text { get; set; }
        [DataMember(Name = "query")]
        public string query { get; set; }
        [DataMember(Name = "total_results")]
        public int total_results { get; set; }

        public override string ToString()
        {
            return corrected_text + " " + query + " " + total_results;
        }
    }


    [DataContract]
    public class TextEvent
    {
        //only present for comment
        [DataMember(Name = "response")]
        public ResponseID response_id = null; //only one attribute - bad design
        //only present for response
        [DataMember(Name = "discussion")]
        public DiscussionID discussion_id = null; //only one attribute - bad design

        //only present for thread
        [DataMember(Name = "anonymous")]
        public bool? anonymous = null;
        [DataMember(Name = "anonymous_to_peers")]
        public bool? anonymous_to_peers = null;
        [DataMember(Name = "team_id")]
        public string team_id;
        [DataMember(Name = "thread_type")]
        public string thread_type; //basically an enum. discussion or question
        [DataMember(Name = "title")]
        public string title;

        //present for all three
        [DataMember(Name = "body")]
        public string body;
        [DataMember(Name = "category_id")]
        public string category_id;
        [DataMember(Name = "category_name")]
        public string category_name;
        [DataMember(Name = "id")]
        public string id; 
        [DataMember(Name = "options")]
        public DiscussionOptions option; //only contrains the followed attribute. bad design
        [DataMember(Name = "user_course_roles")]
        public string[] user_course_roles;
        [DataMember(Name = "user_forums_roles")]
        public string[] user_forum_roles;

        public override string ToString()
        {
            return body + " " + category_id + " " + category_name + ", " + id + " " + option.followed + ", " + title + " " + discussion_id + " " + response_id;
        }
    }

    [DataContract]
    public class VoteEvent
    {
        [DataMember(Name = "category_id")]
        public string category_id;
        [DataMember(Name = "category_name")]
        public string category_name;
        [DataMember(Name = "undo_vote")]
        public bool undo_vote;

        public override string ToString()
        {
            return category_id + " " + category_name + " " + undo_vote;
        }
    }

    [DataContract]
    public class DiscussionOptions
    {
        [DataMember(Name = "followed")]
        public bool followed;
    }

    [DataContract]
    public class DiscussionID
    {
        [DataMember(Name = "id")]
        public string id = null;
    }

    [DataContract]
    public class ResponseID
    {
        [DataMember(Name = "id")]
        public string id = null;
    }

    //note: corrected text was added *after* 5 Jun 2014
    //event itself was added 16 May 2014
    //Maybe only look at post Jun 2014
    [DataContract]
    public class DiscussionSearch
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }
        //event has search specific fields. 
        [DataMember(Name = "event")]
        public SearchEvent eventField { get; set; }
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        public override string ToString()
        {
            return context.ToString() + eventField.ToString() + " " + event_type + " " + time;
        }
    }

    [DataContract]
    public class DiscussionVote
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }

        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        [DataMember(Name = "event")]
        public VoteEvent vote_event { get; set; }

        public override string ToString()
        {
            return context.ToString() + " " + vote_event.ToString() + " " + event_type + " " + time;
        }
    }

    [DataContract]
    public class DiscussionText
    {
        //context has userid and courseid
        [DataMember(Name = "context")]
        public Context context { get; set; }

        //event has event specific fields. 
        [DataMember(Name = "event")]
        public TextEvent eventField { get; set; }

        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }

        public override string ToString()
        {
            return context.ToString() + " " + eventField.ToString() + " " + event_type + " " + time;
        }
    }
}