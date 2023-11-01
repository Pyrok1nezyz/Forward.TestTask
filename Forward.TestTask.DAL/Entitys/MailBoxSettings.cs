using System.ComponentModel.DataAnnotations.Schema;

namespace Forward.TestTask.DAL.Entitys;
[Table("email")]
public class MailBoxSettings
{
	public uint Id { get; init; }
	public string Login { get; set; }
	public string? Password { get; set; }
	public string Adress { get; set; }
	public int? SslProtocol { get; set; }
	public int Port { get; set; }
	public bool CertCheck {get; set; }
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
</root>




 5. Таблицы:
   - email (Проверяемые почтовые ящики), поля:
   - Id bigint IDENTITY PK
   - ... Остальные поля - по смыслу;
*/