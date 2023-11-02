using System.Text.RegularExpressions;
using System.Xml;
using Forward.TestTask.DAL;
using Forward.TestTask.Domain.Entitys;
using MimeKit;

namespace Forward.TestTask.Parsers;

public class MailTemplateParser
{
	private readonly string _regex;
	private readonly bool _delete;
	private readonly UnitOfWork _unitOfWork;

	public MailTemplateParser(MailTemplate template, UnitOfWork unitOfWork)
	{
		_regex = template.Regex;
		_delete = template.IsDelete;
		_unitOfWork = unitOfWork;
	}

	public bool TryAddMessageToDb(MimeMessage message)
	{
		var sender = message.Sender.Name;

		if (!IsRegexMatch(sender)) return false;

		var isMailExist = IsMailExist(message.MessageId).Result;
		if (isMailExist) return false;

		var parser = new MailMessageParser(message);
		var mail = parser.ParseMessage();

		return AddMessageToDb(mail);
	}

	private bool AddMessageToDb(Mail entity)
	{
		return _unitOfWork.MailRepository.Add(entity).Result;
	}

	private bool IsRegexMatch(string target)
	{
		return Regex.IsMatch(_regex, target);
	}

	private string? GetSqlScriptFromXmlDocument(string document)
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(document);

		var value = xmlDoc["root"]?["sql"]?.InnerText.Trim();
		return value;
	}

	private async Task<bool> IsMailExist(string id)
	{
		var result = await _unitOfWork.MailRepository.GetByUniqueId(id);
		return result != null;
	}
}
