using CalendarControl;

namespace CalendarExample.Models;

public class EventModel : IDatable
{
    public int Id { get; set; }
    public DateTime DateOf { get; set; } = DateTime.Now;
    public TimeSpan StartTime { get; set; } = DateTime.Now.TimeOfDay;
    public TimeSpan EndTime { get; set; } = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(30));
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public EventModel()
    {

    }

    public EventModel(int id, string title, DateTime dateOf, TimeSpan start, TimeSpan end, string location)
    {
        Id = id;
        Title = title;
        DateOf = dateOf;
        StartTime = start;
        EndTime = end;
        Location = location;
    }

    public bool AllowUpdate() 
    {
        if (string.IsNullOrEmpty(Title))
            return false;

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EventModel model &&
               Id == model.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public override string ToString()
    {
        return $"{StartTime:hh\\:mm} - {Title}";
    }
}