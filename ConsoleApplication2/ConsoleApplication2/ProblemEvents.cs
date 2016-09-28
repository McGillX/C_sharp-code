using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ConsoleApplication2
{
    public class ProblemDefinition : System.Object
    {
        public string id;
        public string path;
        public string course_id;
        public double maxGrade;
        public string module_id;
        public string display_name;
        public bool isComplete = false;

        public ProblemDefinition(string id, string path, string courseID, double grade, string mID, string name)
        {
            this.id = id;
            this.path = path;
            this.course_id = courseID;
            this.maxGrade = grade;
            this.module_id = mID;
            this.display_name = name;
            if (id != null && id.Length > 1 && path != null && path.Length > 1 && course_id != null && course_id.Length > 1 && module_id != null && module_id.Length > 1)
                isComplete = true;
        }

        public override bool Equals(Object p)
        {
            if (p == null)
                return false;
            ProblemDefinition pd = p as ProblemDefinition;
            if (this.id.Equals(pd.id))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return display_name + " " + course_id + " " + id;
        }
    }

    [DataContract]
    public class ProblemCorrectMapItem
    {
        [DataMember(Name = "correctness")]
        public string correctness;

        public override string ToString()
        {
            return correctness;
        }
    }

    [DataContract]
    public class ProblemSubmissionElement
    {
        [DataMember(Name = "input_type")]
        public string inputType;
        [DataMember(Name = "question")]
        public string questionText;
        [DataMember(Name = "response_type")]
        public string responseType;
        [DataMember(Name = "answer")]
        public object answer;

        //convert answer from object to a list of strings (answers)
        public List<string> finalAnswers;

        [DataMember(Name = "correct")]
        public string iscorrect;

        public bool? correct;

        public List<String> answerList;

        public override string ToString()
        {
            string s = inputType + " " + responseType + " " + correct + " Answer: ";
            foreach (string a in finalAnswers)
            {
                s += a + " ";
            }
            return s;
        }
    }

    [DataContract]
    public class ProblemCheckEvent
    {
        [DataMember(Name = "problem_id")]
        public string problemID;
        [DataMember(Name = "attempts")]
        public int attempts;
        [DataMember(Name = "max_grade")]
        public double maxGrade;
        [DataMember(Name = "grade")]
        public double grade;

        // Each key is a problem id. Each value is the answer identifier (can have multiple identifiers for eg select all that apply problems).
        [DataMember(Name = "answers")]
        public Dictionary<string, object> answerList;
        //private JsonDictionaryAttribute answer { get; set; }
        //Then, later convert the objects to string list. 

        public Dictionary<string, List<string>> finalAnswerList;

        //each 'parent' key is a problem Id. 
        //the value is *just* the correctness value (the others are all seemingly irrelevant)
        [DataMember(Name = "correct_map")]
        public Dictionary<string, ProblemCorrectMapItem> correctMap;

        //each key is a problem id. 
        //The values are submission objects with lots of useful info. 
        [DataMember(Name = "submission")]
        public Dictionary<string, ProblemSubmissionElement> qSubmissions;

        public override string ToString()
        {
            string s = this.problemID + " Grade: " + grade + " on " + maxGrade + "\n";

            s += "Answers " + answerList.Count;
            foreach (string k in answerList.Keys)
            {
                s += k + " : ";
                foreach (string listElem in finalAnswerList[k])
                {
                    s += listElem + ", ";
                }
            }
            s += "\n";
            s += "correct map " + correctMap.Count;
            foreach (string k in correctMap.Keys)
            {
                s += k + " : " + correctMap[k] + " ";
            }
            s += "\n";
            s += "submissions " + qSubmissions.Count;
            foreach (string k in qSubmissions.Keys)
            {
                s += k + " : " + qSubmissions[k] + " ";
            }
            s += "\n";
            return s;
        }
    }

    [DataContract]
    public class ProblemCheck
    {
        [DataMember(Name = "event")]
        public ProblemCheckEvent probEvent;
        [DataMember(Name = "context")]
        public ContextWithModule probContext;
        [DataMember(Name = "time")]
        public string time { get; set; }
        [DataMember(Name = "event_type")]
        public string event_type { get; set; }
        [DataMember(Name = "agent")]
        public string agent;

        public string sourceSummary;

        public override string ToString()
        {
            return event_type + " " + agent + " " + sourceSummary + "\n" + probEvent;
        }
    }

    public class QuestionSubmission
    {
        public string question_id;
        public string answers;
        public string submissions;
        public string correctness;
        public int pSubmissionId;
    }

    public class QuestionDefinition : System.Object
    {
        public string id;
        public string input_type;
        public string response_type;
        public string question_text;
        public string problem_id;

        public QuestionDefinition(string id)
        {
            this.id = id;
        }

        public override bool Equals(Object p)
        {
            if (p == null)
                return false;
            QuestionDefinition qd = p as QuestionDefinition;
            if (this.id.Equals(qd.id))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return question_text + " " + input_type;
        }
    }
}