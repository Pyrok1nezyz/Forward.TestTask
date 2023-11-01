using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys.Interfaces;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys.Classes;

public class MailTemplateRepository : BaseRepository<MailTemplate>, IMailTempRepository
{
	public MailTemplateRepository(SqlConnection connection) : base(connection)
	{
	}
}