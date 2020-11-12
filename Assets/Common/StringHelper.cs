namespace Com.Github.Knose1.Common
{
	public static class StringHelper
	{
		//Upper CamelCase
		public static string ToUpperCamelCase(this string input)
		{
			if (input.Length == 0) return input;
			return input.Remove(0, 1).Insert(0, input.Substring(0, 1).ToUpper()); // set the 1st character to Upper
		}

		//Lower camelCase
		public static string ToLowerCamelCase(this string input)
		{
			if (input.Length == 0) return input;
			return input.Remove(0, 1).Insert(0, input.Substring(0, 1).ToLower()); //set the 1st character to lower
		}
	}
}
