using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml;

namespace Forward.TestTask.Domain.Entitys;

public class MailTemplate : BaseEntity
{
	public ulong EmailId { get; set; }
	[ForeignKey("EmailId")]
	public MailBoxSettings? MailBoxSettings { get; set; }
	public string Regex { get; set; }
	public bool IsDelete { get; set; }
	public string XmlDocument { get; set; }
}