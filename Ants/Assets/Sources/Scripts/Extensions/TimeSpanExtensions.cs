using System;
using UnityEngine;

public static class TimeSpanExtensions
{

	public static string ToFastingTimerString(this TimeSpan timeSpan)
	{
		return $"{(int)timeSpan.TotalHours}:{timeSpan.ToString(@"mm\:ss")}";
	}
	public static TimeSpan ChangeMinutes(this TimeSpan timeSpan, int minutes)
	{
		return new TimeSpan(timeSpan.Days, timeSpan.Hours, minutes, timeSpan.Seconds);
	}

	public static TimeSpan ChangeHours(this TimeSpan timeSpan, int hours)
	{
		Debug.Log(hours);
		return new TimeSpan(timeSpan.Days, hours, timeSpan.Minutes, timeSpan.Seconds);
	}

	public static TimeSpan ChangeDay(this TimeSpan timeSpan, int day)
	{
		return new TimeSpan(day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
	}

}