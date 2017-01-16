using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ConsoleApplication2
{
    public static class BuildingObjects
    {

        internal static Dictionary<string, BaseCourseModule> BuildCourseModuleObjects(string path)
        {
            //string path = "C:\\Users\\mlyman2\\Documents\\Edx\\Research\\DecryptedEdxDownloads\\McGillX-ATOC185x-1T2016-course_structure-prod-analytics.json";
            string json = System.IO.File.ReadAllText(path);
            Dictionary<string, BaseCourseModule> values = JsonConvert.DeserializeObject<Dictionary<string, BaseCourseModule>>(json);
            foreach (KeyValuePair<string, BaseCourseModule> entry in values)
            {
                string s = entry.Key;
                entry.Value.id = s;
                //ungraded sequentials are actually not graded (not null graded)
                if (entry.Value.category.Equals("sequential") && entry.Value.mData.graded == null)
                    entry.Value.mData.graded = false;

                string[] childIDs = entry.Value.children;
                foreach(string c in childIDs)
                {
                    //shouldn't be needed, but there are some children that don't point to anything, so here it is!
                    if(values.ContainsKey(c))
                        values[c].parentID = s; //sets the parent of each child to be this id.
                }
            }

            return values;
        }
        internal static ProblemCheck BuildTrackingObjectProblem(string line)
        {
            ProblemCheck results = null;
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(line)))
            {
                DataContractJsonSerializerSettings settings =
                        new DataContractJsonSerializerSettings();
                settings.UseSimpleDictionaryFormat = true;

                DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(ProblemCheck), settings);

                results = (ProblemCheck)serializer.ReadObject(ms);
                //could return results here. Some List<string> stuff would be unusable though.

                //First, fix finalAnswerList
                Dictionary<string, object> answers = results.probEvent.answerList;
                results.probEvent.finalAnswerList = new Dictionary<string, List<string>>();

                foreach (string k in answers.Keys)
                {
                    results.probEvent.finalAnswerList.Add(k, new List<string>());
                    object elem = answers[k];
                    Type anstype = elem.GetType();
                    if (anstype.IsArray ||
                        (anstype.IsGenericType &&
                        (anstype.GetGenericTypeDefinition() == typeof(ICollection<>) || anstype.GetGenericTypeDefinition() == typeof(IEnumerable<>) || anstype.GetGenericTypeDefinition() == typeof(List<>))))
                    {
                        foreach (object o in (ICollection<object>)elem)
                        {
                            results.probEvent.finalAnswerList[k].Add((string)o);
                        }
                    }
                    else
                    {
                        results.probEvent.finalAnswerList[k].Add((string)elem);
                    }
                }

                //Now, fix the answers in submission.
                Dictionary<string, ProblemSubmissionElement> d = results.probEvent.qSubmissions;
                foreach (string k in d.Keys)
                {
                    results.probEvent.qSubmissions[k].finalAnswers = new List<string>();
                    object elem = results.probEvent.qSubmissions[k].answer;
                    Type anstype = elem.GetType();
                    if (anstype.IsArray ||
                        (anstype.IsGenericType &&
                        (anstype.GetGenericTypeDefinition() == typeof(ICollection<>) || anstype.GetGenericTypeDefinition() == typeof(IEnumerable<>) || anstype.GetGenericTypeDefinition() == typeof(List<>))))
                    {
                        foreach (object o in (ICollection<object>)elem)
                        {
                            results.probEvent.qSubmissions[k].finalAnswers.Add((string)o);
                        }
                    }
                    else
                    {
                        results.probEvent.qSubmissions[k].finalAnswers.Add((string)elem);
                    }
                    string s = results.probEvent.qSubmissions[k].iscorrect;
                    if (s.Equals("true") || s.Equals("True"))
                        results.probEvent.qSubmissions[k].correct = true;
                    else if (s.Equals("false") || s.Equals("False"))
                        results.probEvent.qSubmissions[k].correct = false;
                    else
                        results.probEvent.qSubmissions[k].correct = null;

                }
                //now, compress the 'agent' field
                if (results.agent.Contains("iPad"))
                    results.sourceSummary = "mobile: iPad";
                else if (results.agent.Contains("iPhone"))
                    results.sourceSummary = "mobile: iPhone";
                else if (results.agent.Contains("android"))
                    results.sourceSummary = "mobile: android";
                else if (results.agent.Contains("mobile"))
                    results.sourceSummary = "mobile: other";
                else if (results.agent.Contains("Windows"))
                    results.sourceSummary = "pc: Windows";
                else if (results.agent.Contains("Macintosh"))
                    results.sourceSummary = "pc: Mac";
                else if (results.agent.Contains("Linux"))
                    results.sourceSummary = "pc: Linux";
                else
                    results.sourceSummary = "other";

            }
            return results;
        }


        internal static DiscussionVote BuildTrackingObjectDiscussionVote(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DiscussionVote));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            DiscussionVote dataObject = serializer.ReadObject(ms) as DiscussionVote;
            return dataObject;
        }

        internal static DiscussionText BuildTrackingObjectDiscussiontext(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DiscussionText));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            DiscussionText dataObject = serializer.ReadObject(ms) as DiscussionText;
            return dataObject;
        }

        internal static DiscussionSearch BuildTrackingObjectDiscussionSearch(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(DiscussionSearch));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            DiscussionSearch dataObject = serializer.ReadObject(ms) as DiscussionSearch;
            return dataObject;
        }

        internal static VideoSeek BuildTrackingObjectVideoSeek(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VideoSeek));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            VideoSeek dataObject = serializer.ReadObject(ms) as VideoSeek;
            return dataObject;
        }

        internal static VideoSpeed BuildTrackingObjectVideoSpeed(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VideoSpeed));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            VideoSpeed dataObject = serializer.ReadObject(ms) as VideoSpeed;
            return dataObject;
        }

        internal static VideoOther BuildTrackingObjectVideoOther(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VideoOther));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            VideoOther dataObject = serializer.ReadObject(ms) as VideoOther;
            return dataObject;
        }

        internal static VideoLoad BuildTrackingObjectVideoLoad(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VideoLoad));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            VideoLoad dataObject = serializer.ReadObject(ms) as VideoLoad;
            return dataObject;
        }

        internal static PollSubmit BuildTrackingObjectPollEvent(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PollSubmit));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            PollSubmit dataObject = serializer.ReadObject(ms) as PollSubmit;
            return dataObject;
        }

        internal static BasicTeam BuildTrackingObjectTeamEvent(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BasicTeam));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            BasicTeam dataObject = serializer.ReadObject(ms) as BasicTeam;
            return dataObject;
        }

        internal static CommentEntry BuildMongoObjectComment(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CommentEntry));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            CommentEntry dataObject = serializer.ReadObject(ms) as CommentEntry;
            return dataObject;
        }

        internal static ThreadEntry BuildMongoObjectThread(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ThreadEntry));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            ThreadEntry dataObject = serializer.ReadObject(ms) as ThreadEntry;
            return dataObject;
        }

        internal static TrackingLogWithEvent BuildTrackingObjectEvent(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TrackingLogWithEvent));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            TrackingLogWithEvent dataObject = serializer.ReadObject(ms) as TrackingLogWithEvent;
            return dataObject;
        }

        internal static TrackingLogWithContext BuildTrackingObjectContext(string line)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TrackingLogWithContext));

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(line));
            TrackingLogWithContext dataObject = serializer.ReadObject(ms) as TrackingLogWithContext;
            return dataObject;
        }
    }
}
