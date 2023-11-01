using Forward.TestTask.DAL.Entitys;
using MailKit;
using MimeKit;
using System.Text;

namespace Forward.TestTask.MailClient;

public class MailMessageParser
{
	private IMessageSummary _message;

	public MailMessageParser(IMessageSummary message)
	{
		_message = message;
	}

	private Mail ParseMessage(IMessageSummary summary)
	{
		return new Mail()
		{
			Id = GetMessageId(),
			Body = GetBodyPart(),
			Headers = GetBodyPart(),
			Recipients = GetRecipients(),
			Sender = GetSenderPart(),
			Title = _message.Envelope.Subject,
		};
	}

	public int GetMessageId()
	{
		return _message.Index;
	}

	public string GetBodyPart()
	{
		var bodyPart = _message.BodyParts.OfType<TextPart>().FirstOrDefault();
		if (bodyPart is not null) return bodyPart.Text;
		return string.Empty;
	}

	public string GetHeadersPart()
	{
		var headers = new StringBuilder();
		foreach (var head in _message.Headers)
		{
			headers.Append($"{head.Field}: {head.Value}");
		}
		return headers.ToString();
	}

	public string GetSenderPart()
	{
		var senderPart = _message.Envelope.From.FirstOrDefault();
		if (senderPart is not null) return senderPart.ToString();
		return string.Empty;
	}

	public string GetRecipients()
	{
		var recepeintsPart = new StringBuilder();
		foreach (var recepeint in _message.Envelope.To)
		{
			recepeintsPart.Append($"{recepeint.ToString()}");
		}
		return recepeintsPart.ToString();
	}
}