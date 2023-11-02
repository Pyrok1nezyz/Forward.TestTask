using MimeKit;
using Forward.TestTask.Domain.Entitys;

namespace Forward.TestTask.Parsers;

public class MailMessageParser
{
    private MimeMessage _message;

    public MailMessageParser(MimeMessage message)
    {
        _message = message;
    }

    public Mail ParseMessage()
    {
        return new Mail()
        {
            MailId = GetMessageId(),
            Body = GetBodyPart(),
            Headers = GetBodyPart(),
            Recipients = GetRecipients(),
            Sender = GetSenderPart(),
            Title = _message.Subject
        };
    }

    private string GetMessageId()
    {
        return MimeKit.Utils.MimeUtils.ParseMessageId(_message.MessageId);
    }

    private string GetBodyPart()
    {
        return _message.TextBody;
    }

    private string GetHeadersPart()
    {
        var headers = _message.Headers;
        return string.Join(" | ", headers);
    }

    private string GetSenderPart()
    {
        return _message.Sender.Address;
    }

    private string GetRecipients()
    {
        var reccipients = _message.GetRecipients();
        return string.Join(" | ", reccipients);
    }
}