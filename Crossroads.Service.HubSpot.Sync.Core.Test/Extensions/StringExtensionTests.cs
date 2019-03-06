using System;
using FluentAssertions;
using Xunit;

namespace Crossroads.Service.HubSpot.Sync.Core.Test.Extensions
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        [InlineData("\r\n\t")]
        public void IsNullOrWhitespace_WhenNullEmptyOrWhiteSpace_ShouldReturnTrue(string valueToCheck) =>
            valueToCheck.IsNullOrWhiteSpace().Should().BeTrue();

        [Theory]
        [InlineData("Title Case Gets Split Apart", "TitleCaseGetsSplitApart")]
        [InlineData("ABCDEFGis The Beginning Of", "ABCDEFGisTheBeginningOf")]
        [InlineData("ABCDEFG is The Beginning Of", "ABCDEFG isTheBeginningOf")]
        public void SpaceDelimitTitleCaseText_WhenTitleCase_ShouldGetSpacedAsExpected(string expected, string titleCaseText) =>
            titleCaseText.SpaceDelimitTitleCaseText().Should().Be(expected);
    }
}