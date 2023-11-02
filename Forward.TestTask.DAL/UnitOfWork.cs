using System.Data;
using Forward.TestTask.DAL.Repositories;
using Forward.TestTask.DAL.Repositorys;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask.DAL;

public class UnitOfWork : IDisposable
{
    private MailRepository? _mailRepository;
    private MailBoxRepository? _mailBoxRepository;
    private MailTemplateRepository? _mailTemplateRepository;
    private readonly IDbConnection _connection;
    public UnitOfWork(IConfiguration config)
    {
	    _connection = GetDbConnection(config);
    }

    public MailRepository MailRepository => _mailRepository ??= new MailRepository(_connection);
    public MailBoxRepository MailBoxRepository => _mailBoxRepository ??= new MailBoxRepository(_connection);
    public MailTemplateRepository MailTemplateRepository => _mailTemplateRepository ??= new MailTemplateRepository(_connection);

    private IDbConnection GetDbConnection(IConfiguration config)
    {
		var connectionString = config.GetSection("ConnectionString").Value;
		IDbConnection connection;
		try
		{
			connection = new SqlConnection(connectionString);
            connection.Open();
		}
		catch (SqlException ex)
		{
            Console.WriteLine(ex.Message);
		}
        return new SqlConnection(connectionString);
	}
	public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}