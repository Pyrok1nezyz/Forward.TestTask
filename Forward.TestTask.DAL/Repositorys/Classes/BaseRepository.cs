using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Dapper;
using Forward.TestTask.DAL.Repositorys.Interfaces;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Forward.TestTask.DAL.Repositorys.Classes;

public abstract class BaseRepository<T> : IDisposable, IAsyncDisposable, IBaseRepository<T> where T : class
{
	public IQueryable<T> Table { get; }
	SqlConnection _connection;
	public async Task<IEnumerable<T>> GetAll()
	{
		IEnumerable<T> result = null;
		try
		{
			string tableName = GetTableName();
			string columns = GetColumns(excludeKey: false);
			string query = $"SELECT ({columns}) FROM {tableName}";

			result = await _connection.QueryAsync<T>(query);
		}
		catch (Exception ex) { }

		return result;
	}

	public async Task<bool> Add(T entity)
	{
		int rowsEffected = 0;
		try
		{
			string tableName = GetTableName();
			string columns = GetColumns(excludeKey: true);
			string properties = GetPropertyNames(excludeKey: true);
			string query = $"INSERT INTO {tableName} ({columns}) VALUES ({properties})";

			rowsEffected = await _connection.ExecuteAsync(query, entity);
		}
		catch (Exception ex) { }

		return rowsEffected > 0 ? true : false;
	}

	public async Task<bool> Delete(T entity)
	{
		int rowsEffected = 0;
		try
		{
			string tableName = GetTableName();
			string keyColumn = GetKeyColumnName();
			string keyProperty = GetKeyPropertyName();
			string query = $"DELETE FROM {tableName} WHERE {keyColumn} = @{keyProperty}";

			rowsEffected = await _connection.ExecuteAsync(query, entity);
		}
		catch (Exception ex) { }

		return rowsEffected > 0 ? true : false;
	}

	public async Task<bool> Update(T entity)
	{
		int rowsEffected = 0;
		try
		{
			string tableName = GetTableName();
			string keyColumn = GetKeyColumnName();
			string keyProperty = GetKeyPropertyName();

			StringBuilder query = new StringBuilder();
			query.Append($"UPDATE {tableName} SET ");

			foreach (var property in GetProperties(true))
			{
				var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

				string propertyName = property.Name;
				string columnName = columnAttr.Name;

				query.Append($"{columnName} = @{propertyName},");
			}

			query.Remove(query.Length - 1, 1);

			query.Append($" WHERE {keyColumn} = @{keyProperty}");

			rowsEffected = await _connection.ExecuteAsync(query.ToString(), entity);
		}
		catch (Exception ex) { }

		return rowsEffected > 0 ? true : false;
	}

	private string GetTableName()
	{
		string tableName = "";
		var type = typeof(T);
		var tableAttr = type.GetCustomAttribute<TableAttribute>();
		if (tableAttr != null)
		{
			tableName = tableAttr.Name;
			return tableName;
		}

		return type.Name + "s";
	}

	public static string GetKeyColumnName()
	{
		PropertyInfo[] properties = typeof(T).GetProperties();

		foreach (PropertyInfo property in properties)
		{
			object[] keyAttributes = property.GetCustomAttributes(typeof(KeyAttribute), true);

			if (keyAttributes != null && keyAttributes.Length > 0)
			{
				object[] columnAttributes = property.GetCustomAttributes(typeof(ColumnAttribute), true);

				if (columnAttributes != null && columnAttributes.Length > 0)
				{
					ColumnAttribute columnAttribute = (ColumnAttribute)columnAttributes[0];
					return columnAttribute.Name;
				}
				else
				{
					return property.Name;
				}
			}
		}

		return null;
	}

	private string GetColumns(bool excludeKey = false)
	{
		var type = typeof(T);
		var columns = string.Join(", ", type.GetProperties()
			.Where(p => !excludeKey || !p.IsDefined(typeof(KeyAttribute)))
			.Select(p =>
			{
				var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
				return columnAttr != null ? columnAttr.Name : p.Name;
			}));

		return columns;
	}

	protected string GetPropertyNames(bool excludeKey = false)
	{
		var properties = typeof(T).GetProperties()
			.Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

		var values = string.Join(", ", properties.Select(p =>
		{
			return $"@{p.Name}";
		}));

		return values;
	}

	protected IEnumerable<PropertyInfo> GetProperties(bool excludeKey = false)
	{
		var properties = typeof(T).GetProperties()
			.Where(p => !excludeKey || p.GetCustomAttribute<KeyAttribute>() == null);

		return properties;
	}

	protected string GetKeyPropertyName()
	{
		var properties = typeof(T).GetProperties()
			.Where(p => p.GetCustomAttribute<KeyAttribute>() != null);

		if (properties.Any())
		{
			return properties.FirstOrDefault().Name;
		}

		return null;
	}

	public void Dispose()
	{
		_connection.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		await _connection.DisposeAsync();
	}
}