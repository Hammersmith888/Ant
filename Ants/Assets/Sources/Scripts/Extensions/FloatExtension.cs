public static class FloatExtension
{
	public static string ToEnglandString(this float value)
	{
		return value.ToString("F", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
	}
}