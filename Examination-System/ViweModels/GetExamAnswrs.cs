namespace Examination_System.ViweModels
{
    public class GetExamAnswrs
    {
        public int exam_id { get; set; }
        public int question_id { get; set; } 
        public string question_text { get; set; }= null!;

        public int selected_option { get; set; }

        public string selected_option_text{ get; set; }= null!;

    }
}
