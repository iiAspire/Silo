public class WorldClock
{
    public int Day = 0;
    public int MinuteOfDay = 480; // 08:00 start

    public void AdvanceMinutes(int minutes)
    {
        MinuteOfDay += minutes;

        while (MinuteOfDay >= 1440)
        {
            MinuteOfDay -= 1440;
            Day++;
        }
    }
}