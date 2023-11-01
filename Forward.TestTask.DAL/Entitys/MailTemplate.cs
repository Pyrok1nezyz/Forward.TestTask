using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml;

namespace Forward.TestTask.DAL.Entitys;

public class MailTemplate
{
	[Key]
	public uint Id { get; init; }
	[ForeignKey("EmailId")]
	public Mail? Mail { get; set; }
	public Regex? Regex { get; set; }
	public bool IsDelete { get; set; }
	public XmlDocument? XmlDocument { get; set; }
}

/*
   - Id bigint IDENTITY PK
   - EMailId bigint FK (табл.EMail)
   - Regexp varchar(150)
   - IsDelete bit (настройка, определяющая необходимость удаления почтовых сообщений после выполнения sql-скрипта)
   - XmlInfo xml, структура xml:
   <root>
   <sql><![CDATA[
   Содержимое sql-скрипта
   ]]></sql>
   </root> */