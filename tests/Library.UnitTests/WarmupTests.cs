using Library.Core.Entities;
using Library.Core.Helpers;
using Library.Core.Repositories;
using Library.Core.Services;
using Xunit;

namespace Library.UnitTests;

public sealed class WarmupTests
{
    [Theory]
    [InlineData(2,2,true)]
    [InlineData(5,2,false)]
    [InlineData(4,2,true)]
    [InlineData(0,2,false)]
    public void IsPowerOfN_ReturnsTrueOrFalse(int bookId, int n, bool expected)
    {
        var result = Warmup.IsPowerOfN(bookId, n);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Moby Dick", "kciD yboM")]
    [InlineData("Rebtel", "letbeR")]
    public void ReverseTitle_ReturnsReversedTitle(string bookTitle, string expected)
    {
        var result = Warmup.ReverseTitle(bookTitle);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Read", 3, "ReadReadRead")]
    [InlineData("Rebtel", 2,  "RebtelRebtel")]
    public void RepeatTitle_ReturnsRepeatedTitle(string bookTitle, int repeatCount, string expected)
    {
        var result = Warmup.RepeatTitle(bookTitle, repeatCount);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void GetOddNumberedIds_ReturnsOddIds()
    {
        const int from = 0;
        const int to = 10;
        
        IReadOnlyList<int> expected = [1, 3, 5, 7, 9];
        
        var result = Warmup.GetOddNumberedIds(from, to);
        Assert.Equal(expected, result);
    }
}
