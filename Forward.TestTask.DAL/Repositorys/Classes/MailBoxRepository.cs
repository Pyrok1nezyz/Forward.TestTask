using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys.Interfaces;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys.Classes;

public class MailBoxRepository : BaseRepository<MailBoxSettings>, IMailBoxRepository
{
	public MailBoxRepository(SqlConnection connection) : base(connection)
	{
	}
}