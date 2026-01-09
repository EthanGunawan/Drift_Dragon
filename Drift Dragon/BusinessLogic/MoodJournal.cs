namespace Drift_Dragon.BusinessLogic;

public class MoodJournal
{
    public int MoodJournalID { get; set; }
    public DateTime Date { get; set; }
    public Mood Mood { get; set; }
    public string Reflection { get; set; }
}