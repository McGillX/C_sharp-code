namespace ConsoleApplication2
{
    public class Course
    {
        internal string name;
        internal string term;
        internal int year;
        internal string startDate;
        internal string endDate;
        internal string[] gradedCompIds; //array of string of 'ids' of graded components (tests, assignments, etc)

        internal Course(string name, string term, int year, string startDate, string endDate)
        {
            this.name = name;
            this.term = term;
            this.year = year;
            this.startDate = startDate;
            this.endDate = endDate;
        }

        internal Course(string name, string term, int year, string[] gradedIds)
        {
            this.name = name;
            this.term = term;
            this.year = year;
            this.gradedCompIds = gradedIds;
        }
    }
}