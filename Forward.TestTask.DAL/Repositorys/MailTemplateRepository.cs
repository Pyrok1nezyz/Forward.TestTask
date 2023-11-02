using System.Data;
using System.Data.Common;
using Dapper;
using Forward.TestTask.Domain.Entitys;
using Forward.TestTask.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys;

public class MailTemplateRepository : BaseRepository<MailTemplate>, IMailTempRepository
{
    public MailTemplateRepository(IDbConnection connection) : base(connection)
    {
    }

    public async Task<MailTemplate> GetByMailBoxId(ulong id)
    {
	    try
	    {
		    var tableName = GetTableName();
		    var columns = GetColumns(excludeKey: false);
		    var columnsString = string.Join(", ", columns);
		    var sql = $"SELECT ({columnsString}) FROM {tableName} WHERE EmailId = {id}";
		    var results = await _connection.QueryAsync<MailTemplate>(sql);

		    return results.First();
	    }
	    catch (Exception ex)
	    {
		    return null;
	    }
	}
}