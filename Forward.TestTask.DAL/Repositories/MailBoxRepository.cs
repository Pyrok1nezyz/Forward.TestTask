using System.Data;
using Forward.TestTask.Domain.Entitys;
using Forward.TestTask.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys;

public class MailBoxRepository : BaseRepository<MailBoxSettings>, IMailBoxRepository
{
    public MailBoxRepository(IDbConnection connection) : base(connection)
    {
    }
}