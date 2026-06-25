namespace task01;
public static class StringExtensions
{
    public static bool IsPalindrome(this string input)
    {
        string low = input.ToLower();
        char[] ch = new char[low.Length];
        int count = 0;
        foreach (char c in low)
        {
            if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            {
                ch[count] = c;
                count++;
            }
        }
        if (count == 0)
        {
            return false;
        }
        string s = new string(ch, 0, count);
        char[] reverse = new char[count];
        for (int i = 0; i < count; i++)
        {
            reverse[i] = ch[count - 1 - i];
        }
        string new_s = new string(reverse);
        return s == new_s;
    }
}