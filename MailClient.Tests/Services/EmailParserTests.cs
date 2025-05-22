using FluentAssertions;
using MailClient.App.Models;
using MailClient.App.Services;

namespace MailClient.Tests.Services
{
    public class EmailMessageParserTests
    {
        private readonly EmailMessageParser _parser = new EmailMessageParser();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Parse_InvalidRawEmail_ThrowsArgumentException(string raw)
        {
            Action act = () => _parser.Parse(raw);
            act.Should().Throw<ArgumentException>().WithMessage("Raw email message cannot be empty.*");
        }

        [Fact]
        public void Parse_MissingFromHeader_ThrowsFormatException()
        {
            string raw =
            "To: to@example.com\r\n" +
            "Subject: Test\r\n" +
            "\r\n" +
            "Body";
            Action act = () => _parser.Parse(raw);
            act.Should().Throw<FormatException>().WithMessage("Missing or empty 'From' header.");
        }

        [Fact]
        public void Parse_MissingToHeader_ThrowsFormatException()
        {
            string raw =
            "From: from@example.com\r\n" +
            "Subject: Test\r\n" +
            "\r\n" +
            "Body";
            Action act = () => _parser.Parse(raw);
            act.Should().Throw<FormatException>().WithMessage("Missing or empty 'To' header.");
        }

        [Fact]
        public void Parse_MissingSubjectHeader_ThrowsFormatException()
        {
            string raw =
            "From: from@example.com\r\n" +
            "To: to@example.com\r\n" +
            "\r\n" +
            "Body";

            Action act = () => _parser.Parse(raw);
            act.Should().Throw<FormatException>().WithMessage("Missing or empty 'Subject' header.");
        }

        [Fact]
        public void Parse_MalformedHeaderLine_ThrowsFormatException()
        {
            string raw =
            "From from@example.com\r\n" +
            "To: to@example.com\r\n" +
            "Subject: Test\r\n" +
            "\r\n" +
            "Body";

            Action act = () => _parser.Parse(raw);
            act.Should().Throw<FormatException>().WithMessage("Invalid header line: 'From from@example.com'");
        }

        [Fact]
        public void Parse_ValidSingleAddressWithoutCc_ParsesCorrectly()
        {
            string raw =
            "From: sender@example.com\r\n" +
            "To: recipient@example.com\r\n" +
            "Subject: Hello\r\n" +
            "\r\n" +
            "This is the body.";

            EmailModel result = _parser.Parse(raw);
            result.From.Should().Be("sender@example.com");
            result.To.Should().BeEquivalentTo(new[] { "recipient@example.com" });
            result.Cc.Should().BeEmpty();
            result.Subject.Should().Be("Hello");
            result.Body.Should().Be("This is the body." + Environment.NewLine);
        }

        [Fact]
        public void Parse_ValidMultipleAddressesWithCc_ParsesLists()
        {
            string raw =
            "From: a@ex.com\r\n" +
            "To: b@ex.com, c@ex.com; d@ex.com\r\n" +
            "Cc: e@ex.com; f@ex.com, g@ex.com\r\n" +
            "Subject: Test\r\n" +
            "\r\n" +
            "Line1\r\n" +
            "Line2";

            EmailModel result = _parser.Parse(raw);
            result.To.Should().BeEquivalentTo(new[] { "b@ex.com", "c@ex.com", "d@ex.com" });
            result.Cc.Should().BeEquivalentTo(new[] { "e@ex.com", "f@ex.com", "g@ex.com" });
            result.Body.Should().Be("Line1" + Environment.NewLine + "Line2" + Environment.NewLine);
        }

        [Fact]
        public void Parse_HeaderFolding_MergesContinuationLines()
        {
            string raw =
            "From: sender@example.com\r\n" +
            "To: recipient@example.com\r\n" +
            "Subject: This is a long\r\n" +
            " continuation subject\r\n" +
            "\r\n" +
            "Body";

            EmailModel result = _parser.Parse(raw);
            result.Subject.Should().Be("This is a long continuation subject");
        }

        [Fact]
        public void Parse_HeadersCaseInsensitive_ParsesHeaders()
        {
            string raw =
            "from: one@ex.com\r\n" +
            "TO: two@ex.com\r\n" +
            "sUbJeCt: CaseTest\r\n" +
            "\r\n";

            EmailModel result = _parser.Parse(raw);
            result.From.Should().Be("one@ex.com");
            result.To.Should().BeEquivalentTo(new[] { "two@ex.com" });
            result.Subject.Should().Be("CaseTest");
        }
    }
}
