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

    public async Task<bool> Execute(string sql, Mail mail)
    {
	    int rowsEffected = 0;
	    try
	    {
		    var query = sql;
			var connection = (SqlConnection)_connection;
		    var cmd = connection.CreateCommand();

		    cmd.Parameters.Add(new SqlParameter("@Email_Title", ));
			cmd.Parameters.Add()

			rowsEffected = await cmd.ExecuteNonQueryAsync(query, parameters);
	    }
	    catch (Exception ex) { }

	    return rowsEffected > 0;
	}
	public override async Task<bool> Add(Mail entity)
	{
		int rowsEffected = 0;
		try
		{
			var tableName = GetTableName();
			var columns = GetColumns(excludeKey: true);
			var properties = GetPropertyNames(excludeKey: true);
			var query = $"INSERT INTO {tableName} ([@EMail_Title],[@EMail_Body],[@EMail_Headers],[@EMail_Sender],[@EMail_Recipient]) VALUES ([{entity.Title}], [{entity.Body}])";

			rowsEffected = await _connection.ExecuteAsync(query, entity);
		}
		catch (Exception ex) { }

		return rowsEffected > 0;
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