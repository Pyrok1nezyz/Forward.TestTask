using System.Data;
using Dapper;
using Forward.TestTask.DAL.Repositorys;
using Forward.TestTask.Domain.Entitys;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositories;

public class MailRepository : BaseRepository<Mail>
{
	public MailRepository(IDbConnection connection) : base(connection)
	{

	}

	public async Task<bool> Execute(string sql, Mail mail)
	{
		int rowsEffected = 0;

		var dynamicParameters = new DynamicParameters();
		dynamicParameters.Add("@Email_Title", mail.Title);
		dynamicParameters.Add("@EMail_Body", mail.Body);
		dynamicParameters.Add("@EMail_Headers", mail.Headers);
		dynamicParameters.Add("@EMail_Sender", mail.Sender);
		dynamicParameters.Add("@EMail_Recipients", mail.Recipients);
		await _connection.ExecuteAsync(sql, dynamicParameters);

		return rowsEffected > 0;
	}

	public async Task<MailTemplate> GetByUniqueId(string id)
	{

		var tableName = GetTableName();
		var columns = GetColumns(excludeKey: false);
		var columnsString = string.Join(", ", columns);
		var sql = $"SELECT ({columnsString}) FROM {tableName} WHERE MailId = {id}";
		var results = await _connection.QueryAsync<MailTemplate>(sql);

		return results.First();
	}
}