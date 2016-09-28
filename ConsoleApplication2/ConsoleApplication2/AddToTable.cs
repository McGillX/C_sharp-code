using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ConsoleApplication2
{
    public static class AddToTable
    {
        //will set up an empty 'base' tracking log table for (all iterations of) a course
        //has fields: id, course_id, user_id, time_event_emitted, event_type, page_url
        //table name is courseName_all_logs
        internal static void createTrackingLogTableForCourse(MySqlConnection mcon, string courseName)
        {
            string query = "CREATE TABLE " + courseName + "_all_logs(id BIGINT UNSIGNED NOT NULL, course_id varchar(255), user_id int(11), time_event_emitted datetime, event_type varchar(255), page_url text, PRIMARY KEY(id), FOREIGN KEY(course_id) REFERENCES courses(id));";
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        //find all entries from all_logs that contain the coursename
        internal static void populateTrackingLogTableForCourse(MySqlConnection mcon, string courseName)
        {
            string query = "INSERT INTO " + courseName + "_all_logs (id, course_id, user_id, time_event_emitted, event_type, page_url) SELECT * FROM all_logs WHERE (course_id='McGillX/Body101x/1T2015' or course_id = 'course-v1:McGillX+Body101x+1T2016');";
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            try //if the primary key is already present (ie, the entry is already in the body table), an exception will be thrown. 
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            { }
        }

        private static string EscapeAllSpecialChars(string input)
        {
            /*\0   An ASCII NUL (0x00) character.
            \'   A single quote (“'”) character.
            \"   A double quote (“"”) character.
            \b   A backspace character.
            \n   A newline (linefeed) character.
            \r   A carriage return character.
            \t   A tab character.
            \Z   ASCII 26 (Control-Z). See note following the table.
            \\   A backslash (“\”) character.
            \%   A “%” character. See note following the table.
            \_   A “_” character. See note following the table.*/
            input = input.Replace("\\", "\\\\");
            input = input.Replace("'", "\\'");
            input = input.Replace("\"", "\\\"");
            input = input.Replace("%", "\\%");
            input = input.Replace("_", "\\_");

            return input;
        }


        internal static List<Course> SetUpCourses()
        {
            List<Course> allCourses = new List<Course>();

            //allCourses.Add(new Course("CHEM181x", "1T2014", 2014));
            //allCourses.Add(new Course("CHEM181x", "3T2014", 2014));

            /* allCourses.Add(new Course("GROOCx", "T3_2015", 2015));

             allCourses.Add(new Course("CHEM181x", "3T2015", 2015));
             allCourses.Add(new Course("CHEM181x", "3T2016", 2016));*/ //<- Course hasn't happened yet!

            // allCourses.Add(new Course("ATOC185x", "2T2014", 2014));
            // allCourses.Add(new Course("ATOC185x", "1T2015", 2015));
            //allCourses.Add(new Course("ATOC185x", "1T2016", 2016, "2016-01-13")); //Start date: Weds, Jan 13, 2016 2016-01-13

            //allCourses.Add(new Course("Body101x", "1T2015", 2015, "2015-02-25", "2015-05-06")); //also ends may 6
            allCourses.Add(new Course("Body101x", "1T2016", 2016, "2016-02-10", "2016-05-06")); //ends may 6

            return allCourses;
        }

        internal static void AddRowToQuestionSubmissionTable(MySqlConnection mcon, ProblemCheck log)
        {
            string tableName = "question_submission";

            //first, get the problem id
            //all of the times are unique, since we now have microsecond precision.
            MySqlCommand cmd = mcon.CreateCommand();
            cmd.CommandText = "SELECT id from problem_submission WHERE time_event_emitted = '" + log.time.Split('+')[0] + "';";
            //Console.WriteLine("SELECT id from problem_submission WHERE time_event_emitted = '" + log.time.Split('+')[0] + "';");
            //Console.ReadLine();
            int problem_id = 0;
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                problem_id = reader.GetInt32(0);
            }
            reader.Close();
            if (problem_id != 0)
            {
                foreach (string qID in log.probEvent.finalAnswerList.Keys)
                {
                    string query = "INSERT INTO " + tableName + "(question_id, answers, submissions, correctness, problem_submission_id)";
                    QuestionSubmission qs = new QuestionSubmission();
                    qs.question_id = qID;
                    qs.pSubmissionId = problem_id;
                    qs.answers = arrayToString2(log.probEvent.finalAnswerList[qID].ToArray());
                    qs.correctness = log.probEvent.correctMap[qID].correctness;
                    qs.submissions = arrayToString2(log.probEvent.qSubmissions[qID].finalAnswers.ToArray());

                    string ans = EscapeAllSpecialChars(qs.answers);
                    string sub = EscapeAllSpecialChars(qs.submissions);

                    query += "VALUES('" + qs.question_id + "', '" + ans + "', '" + sub + "', '" + qs.correctness + "', " + qs.pSubmissionId + ");";
                    //Console.WriteLine(query);
                    //Console.ReadLine();
                    cmd = new MySqlCommand(query, mcon);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        internal static void AddRowToProblemSubmissionTable(MySqlConnection mcon, ProblemCheck log)
        {
            string tableName = "problem_submission";
            string query = "INSERT INTO " + tableName + "(problem_id, attempt_number, user_id, grade, time_event_emitted, machine_type)";
            int id = 0;

            int.TryParse(log.probContext.userID, out id);
            if (log.probContext.courseID == null || log.probContext.courseID.Length < 5 || id == 0 || log.probEvent == null)
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else
            {
                query += "VALUES('" + log.probEvent.problemID + "', " + log.probEvent.attempts + ", " + id + ", " + log.probEvent.grade + ", '" + log.time.Split('+')[0] + "', '" + log.sourceSummary + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddValuesToQuestionDefnTable(MySqlConnection mcon)
        {
            string tableName = "question_definition";
            foreach (QuestionDefinition qd in Program.allQuestions)
            {
                string query = "INSERT INTO " + tableName + "(id, input_type, response_type, question_text, problem_id)";
                //get rid of quotes! They could break the db... 
                //string body = qd.question_text.Replace("\"", "");
                string body = qd.question_text.Replace("'", "''");

                query += "VALUES('" + qd.id + "', '" + qd.input_type + "', '" + qd.response_type + "', '" + body + "', '" + qd.problem_id + "');";
                //Console.WriteLine(query);
                //Console.ReadLine();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, mcon);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) { };
            }
        }

        internal static void AddValuesToProblemDefnTable(MySqlConnection mcon)
        {
            string tableName = "problem_definition";
            foreach (ProblemDefinition pd in Program.allProblems)
            {
                string query = "INSERT INTO " + tableName + "(id, path, course_id, max_grade, module_id, display_name)";


                query += "VALUES('" + pd.id + "', '" + pd.path + "', '" + pd.course_id + "', " + pd.maxGrade +
                           ", '" + pd.module_id + "', '" + pd.display_name + "');";
                //Console.WriteLine(query);
                //Console.ReadLine();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, mcon);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) { };
            }
        }

        internal static void AddRowToForumSearchedTable(MySqlConnection mcon, DiscussionSearch log)
        {
            string tableName = "forum_searched";
            string query = "INSERT INTO " + tableName + "(event_type, time_event_emitted, query, total_results, corrected_text, user_id, course_id)";
            int id = 0;
            string q = log.eventField.query.Replace("\"", "");
            q = q.Replace("'", "");
            int.TryParse(log.context.userID, out id);
            if (log.context.courseID == null || log.context.courseID.Length < 5 || id == 0 || log.eventField == null || log.eventField.query == null || !(log.event_type.Equals("edx.forum.searched")))
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else
            {
                query += "VALUES('" + log.event_type + "', '" + log.time.Split('+')[0] + "', '" + q + "', " + log.eventField.total_results + ", '" + log.eventField.corrected_text + "', " + id + ", '" + log.context.courseID + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToForumCreatedTable(MySqlConnection mcon, DiscussionText log)
        {
            string tableName = "forum_text_created";
            string query = "INSERT INTO " + tableName + "(id, event_type, anonymous, anonymous_to_peers, body, category_id, category_name, followed, thread_type, title, user_course_role, user_forum_role, response_id, discussion_id, time_event_emitted, user_id, team_id, course_id)";
            int id = 0;
            int anonToPeer = 0;
            int anon = 0;
            string body = log.eventField.body.Replace("\"", "");
            body = body.Replace("'", "");
            int followed = 0;
            if (log.eventField.anonymous == true)
                anon = 1;
            if (log.eventField.anonymous_to_peers == true)
                anonToPeer = 1;
            if (log.eventField.option.followed == true)
                followed = 1;
            int.TryParse(log.context.userID, out id);
            if (log.context.courseID == null || log.context.courseID.Length < 5 || id == 0 || log.eventField == null || log.eventField.body == null || !(log.event_type.Equals("edx.forum.response.created") || log.event_type.Equals("edx.forum.thread.created") || log.event_type.Equals("edx.forum.comment.created")))
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else if (log.eventField.response_id == null && log.eventField.discussion_id == null)
            {
                query += "VALUES('" + log.eventField.id + "', '" + log.event_type + "', " + anon + ", " + anonToPeer + ", '" + body + "', '" + log.eventField.category_id + "', '" + log.eventField.category_name + "', " + followed + ", '" + log.eventField.thread_type + "', '" +
                    log.eventField.title + "', " + arrayToString(log.eventField.user_course_roles) + ", " + arrayToString(log.eventField.user_forum_roles) + ", null, null, '" + log.time.Split('+')[0] + "', " + id + ", '" + log.eventField.team_id + "', '" + log.context.courseID + "');";
            }
            else if (log.eventField.response_id == null)
            {
                query += "VALUES('" + log.eventField.id + "', '" + log.event_type + "', " + anon + ", " + anonToPeer + ", '" + body + "', '" + log.eventField.category_id + "', '" + log.eventField.category_name + "', " + followed + ", '" + log.eventField.thread_type + "', '" +
                   log.eventField.title + "', " + arrayToString(log.eventField.user_course_roles) + ", " + arrayToString(log.eventField.user_forum_roles) + ", null, '" + log.eventField.discussion_id.id + "', '" + log.time.Split('+')[0] + "', " + id + ", '" + log.eventField.team_id + "', '" + log.context.courseID + "');";
            }
            else if (log.eventField.discussion_id == null)
            {
                query += "VALUES('" + log.eventField.id + "', '" + log.event_type + "', " + anon + ", " + anonToPeer + ", '" + body + "', '" + log.eventField.category_id + "', '" + log.eventField.category_name + "', " + followed + ", '" + log.eventField.thread_type + "', '" +
                   log.eventField.title + "', " + arrayToString(log.eventField.user_course_roles) + ", " + arrayToString(log.eventField.user_forum_roles) + ", '" + log.eventField.response_id.id + "', null, '" + log.time.Split('+')[0] + "', " + id + ", '" + log.eventField.team_id + "', '" + log.context.courseID + "');";
            }
            else
            {
                query += "VALUES('" + log.eventField.id + "', '" + log.event_type + "', " + anon + ", " + anonToPeer + ", '" + body + "', '" + log.eventField.category_id + "', '" + log.eventField.category_name + "', " + followed + ", '" + log.eventField.thread_type + "', '" +
                   log.eventField.title + "', " + arrayToString(log.eventField.user_course_roles) + ", " + arrayToString(log.eventField.user_forum_roles) + ", '" + log.eventField.response_id.id + "', '" + log.eventField.discussion_id.id + "', '" + log.time.Split('+')[0] + "', " + id + ", '" + log.eventField.team_id + "', '" + log.context.courseID + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        private static string arrayToString(string[] arr)
        {
            if (arr == null)
                return "";

            string values = "'";

            for (int i = 0; i < arr.Length; i++)
            {
                if (i < arr.Length - 1)
                    values += arr[i] + ",";
                else
                    values += arr[i] + "'";
            }
            return values;
        }

        //doesn't have the quotes around the string.
        private static string arrayToString2(string[] arr)
        {
            if (arr == null)
                return "";

            string values = "";

            for (int i = 0; i < arr.Length; i++)
            {
                if (i < arr.Length - 1)
                    values += arr[i] + ",";
                else
                    values += arr[i];
            }
            return values;
        }

        internal static void AddRowToForumVoteTable(MySqlConnection mcon, DiscussionVote log)
        {
            string tableName = "forum_text_voted";
            string query = "INSERT INTO " + tableName + "(event_type, category_id, category_name, undo_vote, time_event_emitted, user_id, course_id)";
            int id = 0;
            int undo = 0;
            int.TryParse(log.context.userID, out id);
            if (log.context.courseID == null || log.context.courseID.Length < 5 || id == 0 || log.vote_event == null || !(log.event_type.Equals("edx.forum.response.voted") || log.event_type.Equals("edx.forum.thread.voted")))
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else
            {
                if (log.vote_event.undo_vote)
                    undo = 1;
                query += "VALUES('" + log.event_type + "', '" + log.vote_event.category_id + "', '" + log.vote_event.category_name + "', " + undo + ", '" + log.time.Split('+')[0] + "', " + id + ", '" + log.context.courseID + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        //two of the three vid objs should be null
        internal static void AddRowToVideoTable(MySqlConnection mcon, VideoOther vOther, VideoSeek vSeek, VideoSpeed vSpeed, VideoLoad vLoad)
        {
            string tableName = "video_events";
            //all video events have the fields below.
            string query = "INSERT INTO " + tableName + "(event_type, path, user_id, code, module_id, time_event_emitted, course_id";
            int id = 0;
            if (vOther != null)
            {
                int.TryParse(vOther.vidContext.userID, out id);
                query += ", currentTime) VALUES('" + vOther.event_type + "', '" + vOther.vidContext.path + "', " + id + ", '" + vOther.vidOtherEvent.code +
                    "', '" + vOther.vidOtherEvent.id + "', '" + vOther.time.Split('+')[0] + "', '" + vOther.vidContext.courseID + "', " + vOther.vidOtherEvent.curTime + ")";
            }
            else if (vLoad != null)
            {
                int.TryParse(vLoad.vidContext.userID, out id);
                query += ") VALUES('" + vLoad.event_type + "', '" + vLoad.vidContext.path + "', " + id + ", '" + vLoad.vidLoadEvent.code +
                    "', '" + vLoad.vidLoadEvent.id + "', '" + vLoad.time.Split('+')[0] + "', '" + vLoad.vidContext.courseID + "')";
            }
            else if (vSeek != null)
            {
                int.TryParse(vSeek.vidContext.userID, out id);
                query += ", new_time, old_time) VALUES('" + vSeek.event_type + "', '" + vSeek.vidContext.path + "', " + id + ", '" + vSeek.vidSeekEvent.code +
                    "', '" + vSeek.vidSeekEvent.id + "', '" + vSeek.time.Split('+')[0] + "', '" + vSeek.vidContext.courseID + "', '" + vSeek.vidSeekEvent.newTime + "', '" + vSeek.vidSeekEvent.oldTime + "')";
            }
            else if (vSpeed != null)
            {
                int.TryParse(vSpeed.vidContext.userID, out id);
                query += ", currentTime, new_speed, old_speed) VALUES('" + vSpeed.event_type + "', '" + vSpeed.vidContext.path + "', " + id + ", '" + vSpeed.vidSpeedEvent.code +
                    "', '" + vSpeed.vidSpeedEvent.id + "', '" + vSpeed.time.Split('+')[0] + "', '" + vSpeed.vidContext.courseID + "', " + vSpeed.vidSpeedEvent.curTime + ", '" +
                    vSpeed.vidSpeedEvent.newSpeed + "', '" + vSpeed.vidSpeedEvent.oldSpeed + "')";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToTable(MySqlConnection mcon, TrackingLogWithContext log)
        {
            string tableName = "all_logs";
            string query = "";
            int id = 0;
            int.TryParse(log.context.userID, out id);
            //if there is no course id, then we will have a problem with foreign keys
            //also, wtf event doens't have a course id with it?
            if (log.context.courseID == null || log.context.courseID.Length < 5)
            {
                throw new Exception();
            }
            else if (log.page != null && id != 0)
            {
                query = "INSERT INTO " + tableName + "(course_id, user_id, time_event_emitted, event_type, page_url) ";
                query += "VALUES('" + log.context.courseID + "', " + id + ", '" + log.time + "', '" + log.event_type + "', '" + log.page + "');";
            }
            else if (id != 0)
            {
                query = "INSERT INTO " + tableName + "(course_id, user_id, time_event_emitted, event_type) ";
                query += "VALUES('" + log.context.courseID + "', " + id + ", '" + log.time + "', '" + log.event_type + "');";
            }
            else
            {
                query = "INSERT INTO " + tableName + "(course_id, time_event_emitted, event_type) ";
                query += "VALUES('" + log.context.courseID + "', '" + log.time + "', '" + log.event_type + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }
    }
}
