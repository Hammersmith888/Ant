public static class StringExtensions
{
	public static string CapitalizeFirstLetter(this string str)
	{
		if (string.IsNullOrEmpty(str)) return "";
		var firstChar = str[0];
		firstChar = char.ToUpper(firstChar);
		return firstChar + str.Substring(1);
	}
}