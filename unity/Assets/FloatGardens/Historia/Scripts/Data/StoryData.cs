// PLACEHOLDER.
using System.Collections.Generic;
namespace FloatGardens.Historia
{
    public static class StoryData
    {
        public static readonly List<Chapter> Chapters = new List<Chapter>();
        public static readonly List<Mission> Missions = new List<Mission>();
        public static readonly List<Competition> Competitions = new List<Competition>();
        public static readonly List<Rival> Rivals = new List<Rival>();
        public static readonly List<MailItem> Mail = new List<MailItem>();
        public static readonly Dictionary<string, DialogLine[]> Dialogs = new Dictionary<string, DialogLine[]>();
        /// <summary>Preguntas por tema ("historia", "plagas", "catacion", "preparacion"...) o por id de región.</summary>
        public static readonly Dictionary<string, QuizQuestion[]> Quizzes = new Dictionary<string, QuizQuestion[]>();
    }
}
