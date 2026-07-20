namespace Library.Core.Helpers;

public static class Warmup
{
    public static bool IsPowerOfN(int bookId, int n)
    {
        if (bookId < 1 || n < 2)
            return false;

        while (bookId > 1)
        {
            if (bookId % n != 0)
                return false;

            bookId /= n;
        }

        return true;
    }

    public static string ReverseTitle(string bookTitle)
    {
        if (string.IsNullOrEmpty(bookTitle))
            return string.Empty;

        char[] chars = bookTitle.ToCharArray();
        Array.Reverse(chars);

        return new string(chars);
    }
    
    public static string RepeatTitle(string bookTitle, int repeatCount)
    {
        if (string.IsNullOrEmpty(bookTitle) || repeatCount <= 0)
            return string.Empty;

        return string.Concat(Enumerable.Repeat(bookTitle, repeatCount));
    }
    
    public static IReadOnlyList<int> GetOddNumberedIds(int from, int to)
    {
        List<int> bookIds = [];
        if (from > to)
        {
            (from, to) = (to, from);
        }

        if (from % 2 == 0)
        {
            from++;
        }

        for (int i = from; i <= to; i += 2)
        {
            bookIds.Add(i);
        }
        
        return bookIds;
    }
}