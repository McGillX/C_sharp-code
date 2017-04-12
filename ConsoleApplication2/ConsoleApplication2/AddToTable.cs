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
            if (input == null)
                return input;
            input = input.Replace("\\", "");
            input = input.Replace("'", "");
            input = input.Replace("\"", "");
            input = input.Replace("%", "");
            input = input.Replace("_", "");

            return input;
        }

        internal static void AddRowToDragAndDropTable(MySqlConnection mcon, DragAndDropItemDropped drop, DragAndDropItemLifted lift, DragAndDropLoad load)
        {
            string tableName = "drag_and_drop";
            //all drag and drop events have the fields below.
            string query = "INSERT INTO " + tableName + "(event_type, user_id, module_id, time_event_emitted, course_id";
            if (lift != null)
            {
                string m_id = getDragAndDropId(lift.context.path);
                query += ", item_id) VALUES('" + lift.event_type + "', " + lift.context.userID + ", '" + m_id + "', '" + lift.time.Split('+')[0] +
                    "', '" + lift.context.courseID + "', '" + lift.liftEvent.id + "');";
            }
            else if (drop != null)
            {
                int cor = drop.dropEvent.correctness ? 1 : 0;
                string m_id = getDragAndDropId(drop.context.path);
        
                query += ", item_id, item_name, correctness, location_name, location_id) VALUES('" + drop.event_type + "', " + drop.context.userID + ", '" + m_id + "', '" + drop.time.Split('+')[0] +
                    "', '" + drop.context.courseID + "', '" + drop.dropEvent.id + "', '" + drop.dropEvent.id_name + "', " + cor +
                    ", '" + drop.dropEvent.loc_name + "', '" + drop.dropEvent.loc_id + "');";
               
            }
            else if (load != null)
            {
                string m_id = getDragAndDropId(load.context.path);
                query += ") VALUES('" + load.event_type + "', " + load.context.userID + ", '" + m_id + "', '" + load.time.Split('+')[0] +
                    "', '" + load.context.courseID + "');";
            }
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToCohortsTables(MySqlConnection mcon, CohortLog cLog)
        {
            string event_type = cLog.event_type;
            string tableName = "cohort_events";
            string query = "INSERT INTO ";
            if (event_type.Equals("edx.cohort.created"))
            {
                tableName = "cohorts";
                query += tableName + "(id, original_name, time_cohort_created, course_id)";
                query += "VALUES( " + cLog.cEvent.cohort_id+ ", '" + cLog.cEvent.cohortName + "', '" + cLog.time.Split('+')[0] + "', '" + cLog.context.courseID + "');";
            }
            else
            {
                query += tableName + "(cohort_id, user_id, event_type, time_event_emitted, course_id)";
                query += "VALUES( " + cLog.cEvent.cohort_id + ", " + cLog.cEvent.uID + ", '" + cLog.event_type + "', '" + cLog.time.Split('+')[0] + "', '" + cLog.context.courseID + "');";
            }
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }


        private static string getDragAndDropId(string path)
        {
            string[] pieces = path.Split('/');
            string code = "drag-and-drop-v2";
            string id = "";
            foreach(string s in pieces)
            {
                if (s.Contains(code))
                {
                    id = s;
                    break;
                }
                    
            }
            return id;
        }

        internal static void AddRowToCourseModuleTable(MySqlConnection mcon, BaseCourseModule log)
        {
            string tableName = "course_module";
            string query = "INSERT INTO " + tableName + "(id, category, display_name, start_date, end_date, due_date, format, graded, parent_course_module_id)";

            string displayName = EscapeAllSpecialChars(log.mData.displayName);
            //condition ? first_expression : second_expression;  
            string start = log.mData.start == null ? "NULL" : "'" + log.mData.start.Split('Z')[0] + "'";
            string end = log.mData.end == null ? "NULL" : "'" + log.mData.end.Split('Z')[0] + "'";
            string due = log.mData.due == null ? "NULL" : "'"+log.mData.due.Split('Z')[0]+"'";
            string graded = log.mData.graded == null ? "NULL" : log.mData.graded.ToString();
            string parent = log.parentID == null ? "NULL" : "'" + log.parentID.ToString() + "'";
            query += "VALUES('" + log.id + "', '" + log.category + "', '" + displayName + "', "+ start +", " + 
                end + ", " + due + ", '" + log.mData.format + "', " + graded + ", " + parent+ ");";
            
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
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
            //q = q.Replace("'", "");
            q = EscapeAllSpecialChars(q);
            log.eventField.corrected_text = EscapeAllSpecialChars(log.eventField.corrected_text);
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
            string query = "INSERT INTO " + tableName + "(edx_id, event_type, anonymous, anonymous_to_peers, body, category_id, category_name, followed, thread_type, title, user_course_role, user_forum_role, response_id, discussion_id, time_event_emitted, user_id, team_id, course_id)";
            int id = 0;
            int anonToPeer = 0;
            int anon = 0;
            if(log.eventField.title!=null)
                log.eventField.title = EscapeAllSpecialChars(log.eventField.title);
            string body = log.eventField.body.Replace("\"", "");
            body = body.Replace("'", "");
            body = EscapeAllSpecialChars(body);
            log.eventField.category_name = EscapeAllSpecialChars(log.eventField.category_name);
            int followed = 0;
            if (log.eventField.anonymous == true)
                anon = 1;
            if (log.eventField.anonymous_to_peers == true)
                anonToPeer = 1;
            if (log.eventField.option.followed == true)
                followed = 1;
            if (log.eventField.user_course_roles == null || log.eventField.user_course_roles.Length == 0)
            {
                log.eventField.user_course_roles = new string[1];
                log.eventField.user_course_roles[0] = "";
            }
            if(log.eventField.user_forum_roles == null || log.eventField.user_forum_roles.Length == 0)
            {
                log.eventField.user_forum_roles = new string[1];
                log.eventField.user_forum_roles[0] = "";
            }
                
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

        internal static void AddRowToPollSubmitTable(MySqlConnection mcon, PollSubmit log)
        {
            string tableName = "poll_submissions";
            string query = "INSERT INTO " + tableName + "(user_id, course_id, choice, display_name, url_name, usage_key, time_event_emitted, path)";
            int id = 0;
            string displayName = "";
            int.TryParse(log.context.userID, out id);
            if (log.context.courseID == null || log.context.courseID.Length < 5 || id == 0 )
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else
            {
                displayName = EscapeAllSpecialChars(log.context.module.displayName); //make sure the name is okay to be inserted.
                query += "VALUES(" + log.context.userID + ", '" + log.context.courseID + "', '" + log.pollEvent.choice + "', '" +  
                    displayName + "', '" + log.pollEvent.urlName + "', '" + log.context.module.key + "', '" + log.time.Split('+')[0] + "', '" + log.context.path + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToBasicTeamTable(MySqlConnection mcon, BasicTeam log)
        {
            string tableName = "basic_team_logs";
            string query = "INSERT INTO " + tableName + "(course_id, user_id, team_id, time_event_emitted, event_type, page_url)";
            int id = 0;
            string displayName = "";
            int.TryParse(log.teamContext.userID, out id);
            if (log.teamContext.courseID == null || log.teamContext.courseID.Length < 5 || id == 0)
            {
                throw new Exception();
                //invalid log entry. (bad course id, or bad user id)
            }
            else
            {
                query += "VALUES('" + log.teamContext.courseID + "', " + log.teamContext.userID + ", '" + log.teamEvent.team_id + "', '" +
                     log.time.Split('+')[0] + "', '" + log.event_type + "', '"+ log.teamContext.path + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToDiscussionThreadTable(MySqlConnection mcon, ThreadEntry log)
        {
            string tableName = "discussion_thread";
            string query = "INSERT INTO " + tableName + "(id, closed, last_activity_at, commentable_id, title, thread_type, course_id)";
            string title =  EscapeAllSpecialChars(log.title); //make sure the title is okay to be inserted.

            query += "VALUES('" + log.id.id + "', " + log.closed + ", '" + log.last_activity_at.date.Split('Z')[0] + "', '" 
                + log.commentable_id + "', '" + title + "', '" + log.thread_type + "', '" + log.courseID + "');";
           
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToDiscussionPostTable(MySqlConnection mcon, DiscussionPostShared log)
        {
            string tableName = "discussion_post";
            string type = "comment";
            string body = EscapeAllSpecialChars(log.body); //make sure the body is okay to be inserted.
            string query = "INSERT INTO " + tableName + "(id, user_id, anonymous, anonymous_to_peers, body, created_at, updated_at, endorsed, parent_id, thread_id, type)";
            string update = "null";
            if (log.updated_at.date != null)
                update = log.updated_at.date.Split('Z')[0];

            if (log is CommentEntry)
            {
                CommentEntry logAsComment = (CommentEntry)log;
                if (logAsComment.comment_parent_id == null || logAsComment.comment_parent_id.id == null || logAsComment.comment_parent_id.id.Length < 1)
                {
                    try
                    {
                        type = "response";
                        query += "VALUES('" + log.id.id + "', " + log.user_id + ", " + log.anonymous + ", " + log.anonymous_to_peers + ", '" + body
                        + "', '" + log.created_at.date.Split('Z')[0] + "', '" + update + "', " + logAsComment.endorsed + ", null , '" + logAsComment.threadID.id + "', '" + type + "');";
                    }catch(NullReferenceException e)
                    {
                        Console.WriteLine(log);
                        Console.ReadLine();
                    }
                    
                }
                else
                {
                    query += "VALUES('" + log.id.id + "', " + log.user_id + ", " + log.anonymous + ", " + log.anonymous_to_peers + ", '" + body
                    + "', '" + log.created_at.date.Split('Z')[0] + "', '" + update + "', " + logAsComment.endorsed + ", '" + logAsComment.comment_parent_id.id + "', '" + logAsComment.threadID.id + "', '" + type + "');";
                }
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, mcon);
                    cmd.ExecuteNonQuery();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(log);
                    Console.ReadLine();
                }
                if (logAsComment.endorsed)
                {
                    //add to endorsement table
                    if(logAsComment.endorsement!= null)
                        AddRowToDiscussionEndorsementTable(mcon, logAsComment);
                    else
                    {
                        Program.WriteLogToFile(999, 0, log.id.id, 0, new NullReferenceException(), "nullEndorsements");
                    }
                }

            }
            else if(log is ThreadEntry)
            {
                type = "thread";
                query += "VALUES('" + log.id.id + "', " + log.user_id + ", " + log.anonymous + ", " + log.anonymous_to_peers + ", '" + body
                    + "', '" + log.created_at.date.Split('Z')[0] + "', '" + update + "', " + false + ", null, '" + log.id.id + "', '" + type + "');";
                MySqlCommand cmd = new MySqlCommand(query, mcon);
                cmd.ExecuteNonQuery();
            }
            if(log.voters!=null && log.voters.upIds!=null && log.voters.upIds.Length>0)
            {
                foreach(int userID in log.voters.upIds)
                {
                    AddRowToDiscussionVoteTable(mcon, log, userID);
                }
            }
           
        }

        internal static void AddRowToDiscussionEndorsementTable(MySqlConnection mcon, CommentEntry log)
        {
            string tableName = "discussion_endorsement";
            string query = "INSERT INTO " + tableName + "(reply_id, endorsed_at, endorsed_by)";
            query += "VALUES('" + log.id.id + "', '" + log.endorsement.endorsed_at.date.Split('Z')[0] + "', " + log.endorsement.user_id + ");";

            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToDiscussionVoteTable(MySqlConnection mcon, DiscussionPostShared log, int voterID)
        {
            string tableName = "discussion_vote";
            string query = "INSERT INTO " + tableName + "(post_id, user_id)";
            query += "VALUES('" + log.id.id + "', " + voterID + ");";

            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }

        internal static void AddRowToTable(MySqlConnection mcon, TrackingLogWithContext log)
        {
            string tableName = "log_dump"; //updated for new version of the all_logs table.
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
                query += "VALUES('" + log.context.courseID + "', " + id + ", '" + log.time.Split('+')[0] + "', '" + log.event_type + "', '" + log.page + "');";
            }
            else if (id != 0)
            {
                query = "INSERT INTO " + tableName + "(course_id, user_id, time_event_emitted, event_type) ";
                query += "VALUES('" + log.context.courseID + "', " + id + ", '" + log.time.Split('+')[0] + "', '" + log.event_type + "');";
            }
            else
            {
                query = "INSERT INTO " + tableName + "(course_id, time_event_emitted, event_type) ";
                query += "VALUES('" + log.context.courseID + "', '" + log.time.Split('+')[0] + "', '" + log.event_type + "');";
            }
            //Console.WriteLine(query);
            //Console.ReadLine();
            MySqlCommand cmd = new MySqlCommand(query, mcon);
            cmd.ExecuteNonQuery();
        }
    }
}
