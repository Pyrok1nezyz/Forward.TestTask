using System.Data;
using System.Xml;
using Dapper;
using Forward.TestTask.Domain.Entitys;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys;

public class MailRepository : BaseRepository<Mail>
{
    public MailRepository(IDbConnection connection) : base(connection)
    {

    }

    public async Task<MailTemplate> GetByUniqueId(string id)
    {
	    try
	    {
		    var tableName = GetTableName();
		    var columns = GetColumns(excludeKey: false);
		    var columnsString = string.Join(", ", columns);
		    var sql = $"SELECT ({columnsString}) FROM {tableName} WHERE MailId = {id}";
		    var results = await _connection.QueryAsync<MailTemplate>(sql);

		    return results.First();
	    }
	    catch (Exception ex)
	    {
		    return null;
	    }
    }
}