using System.ComponentModel.DataAnnotations.Schema;

namespace Forward.TestTask.Domain.Entitys;
[Table("email")]
public class MailBoxSettings : BaseEntity
{
	public string Login { get; set; }
	public string? Password { get; set; }
	public string Adress { get; set; }
	public int Port { get; set; }
	public bool? CertCheck {get; set; }
}