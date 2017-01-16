using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ConsoleApplication2
{

    class Program
    {
        /*options:
        None - all_logs
        Discussion - forum_searched, forum_text_created, forum_text_voted
        Video - all video logs (cc, transcript, seek, play, etc). ->fixed for 2013, 2014...
        Problem_Submissions - for now, just problem_check
        Problem_Definitions - do this before submissions :)
        Polls - for polls and surveys submission. - done
        Team - for team events. 
        All - populate all tables at once (not implemented)
        */
        private static string EVENT_TYPE = "Problem_Submissions";
        internal static HashSet<ProblemDefinition> allProblems = new HashSet<ProblemDefinition>();
        internal static HashSet<QuestionDefinition> allQuestions = new HashSet<QuestionDefinition>();

        public static void Main(string[] args)
        {

            //MySqlConnection cnn = Connect();
            //SetUpTrackingLogs();
            //ReadDiscussionDump("C:\\Users\\mlyman2\\Documents\\Edx\\Research\\DiscussionLogDumps");
            ReadCourseStructures("C:\\Users\\mlyman2\\Documents\\Edx\\Research\\CourseStructureDumps");
            //String courseName = "Body101x";
            //createTrackingLogTableForCourse(cnn, "courseName");
            //populateTrackingLogTableForCourse(cnn, "courseName");
        }

        //This reads in the *course_struction JSON files.
        private static void ReadCourseStructures(string pathLocation)
        {
            MySqlConnection cnn = Connect();

            string[] fnames = System.IO.Directory.GetFiles(pathLocation);
            Array.Sort(fnames);
            foreach (string fName in fnames)
            {
                Console.WriteLine(fName);
                Dictionary<string, BaseCourseModule> courseModules = BuildingObjects.BuildCourseModuleObjects(fName);
                BaseCourseModule root = null;
                //add the root
                foreach(KeyValuePair<string, BaseCourseModule> entry in courseModules)
                {
                    if (entry.Value.category.Equals("course"))
                    {
                        root = entry.Value;
                        break;
                    }          
                }
                //do bfs to add parents before children. 
                Queue<BaseCourseModule> q = new Queue<BaseCourseModule>();
                q.Enqueue(root);
                while (q.Count != 0)
                {
                    BaseCourseModule next = q.Dequeue();
                    foreach(string id in next.children)
                    {
                        if (courseModules.ContainsKey(id))
                        {
                            BaseCourseModule node = courseModules[id];
                            q.Enqueue(node);
                        }
                    }
                    AddToTable.AddRowToCourseModuleTable(cnn, next);
                }
            }
        }

        //This reads in the *prod.mongo discussion files.
        private static void ReadDiscussionDump(string pathLocation)
        {
            MySqlConnection cnn = Connect();

            int fileNum = 0;

            string[] fnames = System.IO.Directory.GetFiles(pathLocation);
            Array.Sort(fnames);
            foreach (string fName in fnames)
            {
                if (!fName.Contains("McGillX-CHEM181x_3-3T2016"))
                    continue;
                int commentCount = 0, commentThreadCount = 0;
                Console.WriteLine("file number: " + fileNum);
                fileNum++;
                string[] a = System.IO.File.ReadAllLines(fName);
                //the lines are stored in *reverse* chronological order.
                //reverse the array in order to avoid broken foreign key relationship
                //(make sure the threads are inserted before the replies to the threads)
                Array.Reverse(a); 
                int lineNum = a.Length;

                foreach (string theline in a)
                {
                    byte[] bytes = Encoding.Default.GetBytes(theline);
                    string line = Encoding.UTF8.GetString(bytes);
                    DiscussionPostShared entry = null;                  
                    lineNum--;
                    if (line.Contains("\"_type\":\"Comment\""))
                    {
                        commentCount++;
                        //build comment object
                        entry = BuildingObjects.BuildMongoObjectComment(line);
                        /*Console.WriteLine("comment");
                        Console.WriteLine(entry);
                        Console.ReadLine();*/
                        
                    }   
                    else if (line.Contains("\"_type\":\"CommentThread\""))
                    {
                        commentThreadCount++;
                        //build thread object
                        entry = BuildingObjects.BuildMongoObjectThread(line);
                        AddToTable.AddRowToDiscussionThreadTable(cnn, (ThreadEntry)entry);
                        /*Console.WriteLine("thread");
                        Console.WriteLine(entry);
                        Console.ReadLine();*/
                    }
                    else
                    {
                        throw new FormatException("Invalid type. Required: Comment or CommentThread on line "+ lineNum);
                    }
                    AddToTable.AddRowToDiscussionPostTable(cnn, entry);     
                }
                Console.WriteLine(fName);
                //No Other count. Good. 
                Console.WriteLine("Comment count " + commentCount + " thread count " + commentThreadCount);
            }
            Console.ReadLine();
        }

        private static MySqlConnection Connect()
        {
            //update the connection string to contain the connection information for your database.
            string connectionString = "Server=**********; Database=******; Uid=****; Pwd=****************;";
            MySqlConnection cnn;
            cnn = new MySqlConnection(connectionString);
            cnn.Open();
            return cnn;
        }

        //this method will initialize the tracking logs for years 2013-2016 in the all_logs table.
        //attributes pulled from the JSON are those listed in the TrackingLog class. 
        //end May 25 2016.
        private static void SetUpTrackingLogs()
        {
            int[] years = { 2016, 2017 }; 
            int year;
            MySqlConnection cnn = null;
            try
            {
                cnn = Connect();
                Console.WriteLine("connection success!");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }
            foreach (int y in years)
            {
                year = y;
                Console.WriteLine("Year " + year);
                //Console.ReadLine();
                //2013, 2014 are stored differently. 
                if (year > 2014)
                {
                    Post2014Logs(year, cnn);
                }
                else
                {
                    Pre2015Logs(year, cnn);
                }
            }
            //If we were building the definitions, then add them to the tables. 
            if (EVENT_TYPE.Equals("Problem_Definitions"))
            {
                AddToTable.AddValuesToProblemDefnTable(cnn);
                AddToTable.AddValuesToQuestionDefnTable(cnn);
            }         
        }

        //Reads in a JSON file. 
        //Will call methods that create sql objects and add rows to tables
        //switch is on event_type - so wording must be exact match in final attribute
        private static void ReadFile(string fileName, int year, MySqlConnection cnn, int fileNum)
        {
            var a = System.IO.File.ReadLines(fileName); //its a big file, so dont load it all at once.
            int lineNum = 0;

            foreach (string theline in a)
            {
                byte[] bytes = Encoding.Default.GetBytes(theline);
                string line = Encoding.UTF8.GetString(bytes);
                switch (EVENT_TYPE) {
                    case "None":
                        AllLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                    case "Discussion":
                        DiscussionLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                    case "Video":
                        VideoLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                    case "Problem_Definitions":
                    case "Problem_Submissions":
                        ProblemLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                    case "Polls":
                        PollLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                    case "Team":
                        TeamLogs(line, fileName, lineNum, cnn, fileNum, year);
                        break;
                }
                lineNum++;
            }
        }

        private static void Pre2015Logs(int year, MySqlConnection cnn)
        {
            int fileNum = 0;
            string pathLocation = "C:\\Users\\mlyman2\\Documents\\Edx\\Research\\decrypted-tracking-logs\\decrypted-tracking-logs\\allLogs" + year;

            string[] fnames = System.IO.Directory.GetFiles(pathLocation);
            Array.Sort(fnames);
            
            foreach (string fileName in fnames)
            {
                Console.WriteLine("file number: " + fileNum);
                Console.WriteLine("file name: " + fileName);
                fileNum++;
                
                ReadFile(fileName, year, cnn, fileNum);
            }
        }

        private static void Post2014Logs(int year, MySqlConnection cnn)
        {
            int fileNum = 0;
            string pathLocation = "C:\\Users\\mlyman2\\Documents\\Edx\\Research\\decrypted-tracking-logs\\decrypted-tracking-logs\\" + year;
                 string[] subdirectoryEntries = System.IO.Directory.GetDirectories(pathLocation);
            Array.Sort(subdirectoryEntries);
            
            foreach (string dirName in subdirectoryEntries)
            {
                string[] fnames = System.IO.Directory.GetFiles(dirName);
                Console.WriteLine("file number: " + fileNum);
                Console.WriteLine("file name: " + fnames[0]);
             
                fileNum++;
                if (EVENT_TYPE.Equals("Team") && fileNum < 257 && year<=2015) //there are no team events before this point
                  continue;

                ReadFile(fnames[0], year, cnn, fileNum);
            }
        }

        private static void TeamLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            //if the line doesn't describe a standard team event.
            string[] tags = { "edx.team.activity_updated", "edx.team.created", "edx.team.deleted" };
            //If the source is browser, the log will be all messed up and not usable. 
            if (!(line.Contains(tags[0]) || line.Contains(tags[1]) || line.Contains(tags[2])))
                return;
            try
            {

                BasicTeam teamLog = BuildingObjects.BuildTrackingObjectTeamEvent(line);
                if (teamLog.teamEvent.team_id.Length < 4)
                {
                    Console.WriteLine(teamLog);
                    Console.WriteLine(line);
                    Console.ReadLine();
                }
                AddToTable.AddRowToBasicTeamTable(cnn, teamLog);
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                Console.WriteLine(ex.Message);
                Console.WriteLine(line);
                Console.ReadLine();
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    DeadConnectionWrite(fileNum, fileName, line, lineNum);
                }
                WriteLogToFile(year, fileNum, line, lineNum, ex, "PollLogs");
            }
            catch (Exception ex)
            {
                WriteLogToFile(year, fileNum, line, lineNum, ex, "PollLogs");
            }
        }

        private static void PollLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            //if the line doesn't describe a problem event.
            string[] tags = {"\"xblock.poll.submitted\""};
            //If the source is browser, the log will be all messed up and not usable. 
            if (!(line.Contains(tags[0])))
                return;

            try
            {

                PollSubmit poll = BuildingObjects.BuildTrackingObjectPollEvent(line);
                
                AddToTable.AddRowToPollSubmitTable(cnn, poll);
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                Console.WriteLine(ex.Message);
                Console.WriteLine(line);
                Console.ReadLine();
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    DeadConnectionWrite(fileNum, fileName, line, lineNum);
                }
                WriteLogToFile(year, fileNum, line, lineNum, ex, "PollLogs");
            }
            catch (Exception ex)
            {
                WriteLogToFile(year, fileNum, line, lineNum, ex, "PollLogs");
            }
        }

        private static void VideoLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            //if the line doesn't describe a video event -or if its an 'implicit' event (starts with /
            if (!(line.Contains("video") || line.Contains("caption")) || line.Contains("\"event_type\": \"/")) 
                return;
            VideoSpeed vSpeed = null;
            VideoSeek vSeek = null;
            VideoOther vOther = null;
            VideoLoad vLoad = null;
            string loadTag = "\"event_type\": \"load_video\"";
            string speedTag = "\"event_type\": \"speed_change_video\"";
            string seekTag =  "\"event_type\": \"seek_video\"" ;
            string[] otherTags = { "\"event_type\": \"hide_transcript\"", "\"event_type\": \"show_transcript\"", "\"event_type\": \"edx.video.closed_captions",
                                "\"event_type\": \"load_video\"", "\"event_type\": \"pause_video\"", "\"event_type\": \"play_video\"", "\"event_type\": \"stop_video\"",
                                "\"event_type\": \"video_hide_cc_menu\"", "\"event_type\": \"video_show_cc_menu\""};
            try
            {
                if (line.Contains(speedTag))
                {
                    //video speed change
                    line = FixLine(line);
                    vSpeed = BuildingObjects.BuildTrackingObjectVideoSpeed(line);
                    AddToTable.AddRowToVideoTable(cnn, vOther, vSeek, vSpeed, vLoad);
                }
                else if (line.Contains(seekTag))
                {
                    //video seek
                    line = FixLine(line);
                    vSeek = BuildingObjects.BuildTrackingObjectVideoSeek(line);
                    AddToTable.AddRowToVideoTable(cnn, vOther, vSeek, vSpeed, vLoad);

                }
                else if( line.Contains(loadTag))
                {
                    //load video
                    line = FixLine(line);
                    vLoad = BuildingObjects.BuildTrackingObjectVideoLoad(line);
                    AddToTable.AddRowToVideoTable(cnn, vOther, vSeek, vSpeed, vLoad);
                }
                else if (containsElem(line, otherTags))
                {
                    //other video
                    line = FixLine(line);
                    vOther = BuildingObjects.BuildTrackingObjectVideoOther(line);
                    AddToTable.AddRowToVideoTable(cnn, vOther, vSeek, vSpeed, vLoad);
                }
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    DeadConnectionWrite(fileNum, fileName, line, lineNum);
                }
                WriteLogToFile(year, fileNum, line, ex, "videoLogsUpdate");
            }
            catch (Exception ex)
            {
                WriteLogToFile(year, fileNum, line, ex, "videoLogsUpdate");
            }
        }

        //small helper method to fix the stupid escape bug.
        //bad: "event": "{\"code\": \"mobile\", \"id\": \"c6e429afbdde4514a089b1d0ec10ee39\"}",
        //good: "event": {"code": "mobile", "id": "c6e429afbdde4514a089b1d0ec10ee39"},
        private static string FixLine(string input)
        {
            input = input.Replace("\\\"", "\"");
            input = input.Replace("}\"", "}");
            input = input.Replace("\"{", "{");
            return input;
        }

        private static string FixLine2(string input)
        {
            input = input.Replace("\\\"", "\"");
            return input;
        }

        private static void DeadConnectionWrite(int fileNum, string fileName, string line, int lineNum)
        {
            string logFile1 = @"C:\Users\mlyman2\Documents\Edx\Research\ErrorLogs\DeadConnectionDetails.txt";
            using (StreamWriter wr = File.AppendText(logFile1))
            {
                wr.WriteLine("**********************");
                wr.WriteLine(DateTime.Now.ToString());
                wr.WriteLine("File number " + fileNum);
                wr.WriteLine("File name " + fileName);
                wr.WriteLine("Line number " + lineNum);
                wr.WriteLine("Line object contents \n" + line);
                wr.WriteLine("----------------");
            }
            Environment.Exit(0);
        }

        private static void CreateProblemDefinitions(ProblemCheck problem)
        {
            ProblemDefinition pd = new ProblemDefinition(problem.probEvent.problemID, problem.probContext.path, problem.probContext.courseID, problem.probEvent.grade, problem.probContext.module.usageKey, problem.probContext.module.displayName);
            if (allProblems.Contains(pd))
            {
                //if pd is complete, add it (the old one may not have been). 
                if (pd.isComplete)
                    allProblems.Add(pd);
            }
            else
                allProblems.Add(pd);
        }

        private static void CreateQuestionDefinitions(ProblemCheck problem)
        {
            foreach(string qID in problem.probEvent.qSubmissions.Keys)
            {
                //if the hashset already contains a quesiton with that id, then skip it
                if (allQuestions.Contains(new QuestionDefinition(qID)))
                    continue;
                QuestionDefinition q = new QuestionDefinition(qID);
                ProblemSubmissionElement pSubElem = problem.probEvent.qSubmissions[qID];
                q.input_type = pSubElem.inputType;
                q.question_text = pSubElem.questionText;
                q.response_type = pSubElem.responseType;
                q.problem_id = problem.probEvent.problemID;
                allQuestions.Add(q);
            }
        }

        private static void ProblemLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            //if the line doesn't describe a problem event.
            string tag = "\"event_type\": \"problem_check\"";
            //If the source is browser, the log will be all messed up and not usable. 
            string badSource = "\"event_source\": \"browser\"";
            if (!line.Contains(tag) || line.Contains(badSource))
                return;

            try
            {
                ProblemCheck problem = BuildingObjects.BuildTrackingObjectProblem(line);

                if (EVENT_TYPE.Equals("Problem_Submissions"))
                {
                    //AddToTable.AddRowToProblemSubmissionTable(cnn, problem);
                    AddToTable.AddRowToQuestionSubmissionTable(cnn, problem);
                }
                else if (EVENT_TYPE.Equals("Problem_Definitions"))
                {
                    CreateProblemDefinitions(problem);
                    CreateQuestionDefinitions(problem);
                }
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                //Console.WriteLine(ex.Message);
                //Console.ReadLine();
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    DeadConnectionWrite(fileNum,fileName, line, lineNum);
                }
                WriteLogToFile(year, fileNum, line, lineNum, ex, "ProblemLogsv5_");
            }
            catch (Exception ex)
            {
                WriteLogToFile(year, fileNum, line, lineNum, ex, "ProblemLogsv5_");
            }
        }

        private static Boolean containsElem(string inpt, string[] elems)
        {
            foreach(string x in elems)
            {
                if (inpt.Contains(x))
                    return true;
            }
            return false;
        }

        private static void DiscussionLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            //if the line doesn't describe a discussion event.
            string tag = "\"event_type\": \"edx.forum";
            if (!line.Contains(tag))
                return;
            string[] createTags = { "\"event_type\": \"edx.forum.thread.created\"", "\"event_type\": \"edx.forum.comment.created\"", "\"event_type\": \"edx.forum.response.created\"" };
            string[] voteTags = { "\"event_type\": \"edx.forum.response.voted\"", "\"event_type\": \"edx.forum.thread.voted\"" };
            string[] searchTag = { "\"event_type\": \"edx.forum.searched\"" };
            try
            {
                if (line.Contains(searchTag[0]))
                {
                    //forum_searched table
                    DiscussionSearch dsEvent = BuildingObjects.BuildTrackingObjectDiscussionSearch(line);
                    //Console.WriteLine(line);
                    AddToTable.AddRowToForumSearchedTable(cnn, dsEvent);
                }
                else if (line.Contains(voteTags[0]) || line.Contains(voteTags[1]))
                {
                    //forum_text_voted
                    DiscussionVote dvEvent = BuildingObjects.BuildTrackingObjectDiscussionVote(line);
                    //Console.WriteLine(line);
                    AddToTable.AddRowToForumVoteTable(cnn, dvEvent);

                }
                else if (line.Contains(createTags[0]) || line.Contains(createTags[1]) || line.Contains(createTags[2]))
                {
                    //forum_text_created
                    DiscussionText dtEvent = BuildingObjects.BuildTrackingObjectDiscussiontext(line);
                    //Console.WriteLine(line);
                    AddToTable.AddRowToForumCreatedTable(cnn, dtEvent);
                }
                else
                {
                    //this is weird. 
                    Console.WriteLine(line);
                    Console.ReadLine();
                }
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    DeadConnectionWrite(fileNum, fileName, line, lineNum);
                }
                WriteLogToFile(year, fileNum, line, ex, "discussionLogs");
            }
            catch (Exception ex)
            {
                Console.WriteLine(line);
                WriteLogToFile(year, fileNum, line, ex, "discussionLogs");
            }
        }

        private static void AllLogs(string line, string fileName, int lineNum, MySqlConnection cnn, int fileNum, int year)
        {
            TrackingLogWithContext tl = null;
            TrackingLogWithEvent tEvent = null;
            try
            {
                tl = BuildingObjects.BuildTrackingObjectContext(line);
                if (tl.context.userID == null || tl.context.userID.Length < 1)
                {
                    //see if we can get the user id from the event field instead. Tt might not be there....
                    try
                    {
                        tEvent = BuildingObjects.BuildTrackingObjectEvent(line);
                        if (!(tl.context.courseID.Equals(tEvent.eventField.courseID)))
                        {
                            //if they can't agree on the course id, use the one that isn't null
                            if (tEvent.eventField.courseID != null)
                                tl.context.courseID = tEvent.eventField.courseID;
                        }
                        tl.context.userID = tEvent.eventField.userID;
                    }
                    catch (Exception ex)
                    { }
                }
                if (tl.page == null)
                    tl.page = tl.context.path; //if there is no page, try using the 'path' from context. 
                AddToTable.AddRowToTable(cnn, tl);
            }
            catch (MySqlException ex)
            {
                int result = ex.Number;
                //these should be all the numbers that can result for a connection error. 
                //not sure wtf 0 is, but its what happens when I unplug the ethernet cable.
                if (result == 53 || result == -2 || result == 2 || result == -1 || result == 0)
                {
                    //log where the break is and then exit. 
                    string logFile1 = @"C:\Users\mlyman2\Documents\Edx\Research\DeadConnectionDetails.txt";
                    using (StreamWriter wr = File.AppendText(logFile1))
                    {
                        wr.WriteLine("File number " + fileNum);
                        wr.WriteLine("File name " + fileName);
                        wr.WriteLine("Line number " + lineNum);
                        wr.WriteLine("Log object contents " + tl.toString());
                    }
                    Environment.Exit(0);
                }
                writeToLogFile(year, fileNum, tl, ex);
            }
            catch (Exception ex)
            {
                writeToLogFile(year, fileNum, tl, ex);
            }
        }

        internal static void WriteLogToFile(int year, int fileNum, string line, int lineNum, Exception ex, string filename)
        {
            string logFile = @"C:\Users\mlyman2\Documents\Edx\Research\ErrorLogs\" + filename + year + ".txt";
            using (StreamWriter wr = File.AppendText(logFile))
            {
                wr.WriteLine("File number " + fileNum);
                wr.WriteLine("Line number " + lineNum);
                wr.WriteLine(line);
                wr.WriteLine(ex.Message);
            }
        }

        private static void WriteLogToFile(int year, int fileNum, string line, Exception ex, string filename)
        {
            string logFile = @"C:\Users\mlyman2\Documents\Edx\Research\ErrorLogs\"+filename + year + ".txt";
            using (StreamWriter wr = File.AppendText(logFile))
            {
                wr.WriteLine("File number " + fileNum);
                wr.WriteLine(line);
                wr.WriteLine(ex.Message);
            } 
        }

        private static void writeToLogFile(int year, int fileNum, TrackingLogWithContext tl, Exception ex)
        {
            string logFile = @"C:\Users\mlyman2\Documents\Edx\Research\ErrorLogs\badLogs" + year + ".txt";
            using (StreamWriter wr = File.AppendText(logFile))
            {
                wr.WriteLine("File number " + fileNum);
                wr.WriteLine(tl.toString());
            }
            Console.WriteLine(ex.Message);
            Console.WriteLine(tl.toString());
            //Console.ReadLine();
        }

    }
}
