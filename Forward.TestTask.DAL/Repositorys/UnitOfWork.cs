using Forward.TestTask.DAL.Repositorys.Classes;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys;

public class UnitOfWork : IDisposable
{
	public MailRepository mailRepository;
	public MailBoxRepository mailBoxRepository;
	public MailTemplateRepository mailTemplateRepository;
	public SqlConnection _Connection;
	public UnitOfWork()
	{
		var connection = new SqlConnection();
		mailRepository = new MailRepository(connection);
		mailBoxRepository = new MailBoxRepository(connection);
		mailTemplateRepository = new MailTemplateRepository(connection);
	}
	public UnitOfWork(SqlConnection connection)
	{
		mailRepository = new MailRepository(connection);
		mailBoxRepository = new MailBoxRepository(connection);
		mailTemplateRepository = new MailTemplateRepository(connection);
	}
	public void Dispose()
	{
		_Connection.Dispose();
		GC.SuppressFinalize(mailRepository);
		GC.SuppressFinalize(mailBoxRepository);
		GC.SuppressFinalize(mailTemplateRepository);
		GC.SuppressFinalize(this);
	}
}