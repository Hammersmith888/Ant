using System;

public static class DateTimeExtensions
{

	public static DateTime ChangeTime(this DateTime dateTime, int hours, int minute)
	{
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hours, minute, dateTime.Second);
	}

	public static DateTime ChangeMonth(this DateTime dateTime, int month)
	{
		return new DateTime(dateTime.Year, month, dateTime.Day);
	}

	public static DateTime ChangeDay(this DateTime dateTime, int day)
	{
		return new DateTime(dateTime.Year, dateTime.Month, day);
	}

	public static DateTime ChangeYear(this DateTime dateTime, int year)
	{
		return new DateTime(year, dateTime.Month, dateTime.Day);
	}

}