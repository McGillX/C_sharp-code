using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    [DataContract]
    public abstract class DiscussionPostShared
    {
        //Shared fields first.
        [DataMember(Name = "_id")]
        public TextID id;
        [DataMember(Name = "body")]
        public string body;
        [DataMember(Name = "anonymous")]
        public bool anonymous;
        [DataMember(Name = "anonymous_to_peers")]
        public bool anonymous_to_peers;

        [DataMember(Name = "votes")]
        public VoteInfo voters;
        [DataMember(Name = "updated_at")]
        public Date updated_at;
        [DataMember(Name = "created_at")]
        public Date created_at;
        [DataMember(Name = "course_id")]
        public string courseID;
        [DataMember(Name = "author_id")]
        public int user_id;
    }

    [DataContract]
    public class VoteInfo
    {
        [DataMember(Name = "up")]
        public int[] upIds;

        public override string ToString()
        {
            string s = "";
            foreach (int i in upIds)
                s += i + "";
            return s;
        }
    }

    [DataContract]
    public class TextID
    {
        [DataMember(Name = "$oid")]
        public string id;

        public override string ToString()
        {
            return id;
        }
    }

    [DataContract]
    public class Date
    {
        [DataMember(Name = "$date")]
        public string date;

        public override string ToString()
        {
            return date;
        }
    }

    [DataContract]
    public class Endorsement
    {
        [DataMember(Name = "user_id")]
        public int user_id;
        [DataMember(Name = "time")]
        public Date endorsed_at;

        public override string ToString()
        {
            return user_id + " " + endorsed_at;
        }
    }

    [DataContract]
    public class CommentEntry : DiscussionPostShared
    {
        [DataMember(Name = "endorsed")]
        public bool endorsed;

        [DataMember(Name = "parent_id")]
        public TextID comment_parent_id;
        [DataMember(Name = "comment_thread_id")]
        public TextID threadID;


        [DataMember(Name = "endorsement")]
        public Endorsement endorsement;

        public override string ToString()
        {
            string s = id + ": " + body + "\n";
            s += endorsed + " " + user_id + " " + comment_parent_id.id + " " + threadID.id + "\n";
            s += courseID + " " + updated_at + " " + created_at + "\n";
            s += voters + "\n";
            s += endorsement;
            return s;
        }
    }

    [DataContract]
    public class ThreadEntry : DiscussionPostShared
    {
        [DataMember(Name = "closed")]
        public bool closed;
        [DataMember(Name = "thread_type")]
        public string thread_type;
        [DataMember(Name = "title")]
        public string title;
        [DataMember(Name = "commentable_id")]
        public string commentable_id;
        [DataMember(Name = "last_activity_at")]
        public Date last_activity_at;

        public override string ToString()
        {
            string s = id + ": " + body + "\n";
            s +=  " " + user_id + " " + courseID + " " + updated_at + " " + created_at + "\n";
            s += closed + " " + thread_type + " " + title + " " + commentable_id + "\n";
            s += voters;
            return s;
        }
    }
}
