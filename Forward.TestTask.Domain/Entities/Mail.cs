using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Forward.TestTask.Domain.Entitys;
[Table("email_Message")]
public class Mail : BaseEntity
{
    public string MailId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Headers { get; set; }
    public string? Sender { get; set; }
    public string? Recipients { get; set; }
}
