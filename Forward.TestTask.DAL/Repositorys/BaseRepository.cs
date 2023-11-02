using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using Dapper;
using System.Text;
using System.Linq.Expressions;
using Forward.TestTask.Domain.Interfaces;

namespace Forward.TestTask.DAL.Repositorys;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
	private protected readonly IDbConnection _connection;

    public BaseRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    public async Task<IEnumerable<T>> GetAll()
    {
        IEnumerable<T> result = null;
        try
        {
            var tableName = GetTableName();
            var columns = GetColumns(excludeKey: false);
            var query = $"SELECT ({columns}) FROM {tableName}";

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
	        var tableName = GetTableName();
	        var columns = GetColumns(excludeKey: true);
	        var properties = GetPropertyNames(excludeKey: true);
	        var query = $"INSERT INTO {tableName} ({columns}) VALUES ({properties})";

            rowsEffected = await _connection.ExecuteAsync(query, entity);
        }
        catch (Exception ex) { }

        return rowsEffected > 0;
    }

    public async Task<bool> Delete(T entity)
    {
        int rowsEffected = 0;
        try
        {
	        var tableName = GetTableName();
	        var keyColumn = GetKeyColumnName();
	        var keyProperty = GetKeyPropertyName();
	        var query = $"DELETE FROM {tableName} WHERE {keyColumn} = @{keyProperty}";

            rowsEffected = await _connection.ExecuteAsync(query, entity);
        }
        catch (Exception ex) { }

        return rowsEffected > 0;
    }

    public async Task<bool> Update(T entity)
    {
        int rowsEffected = 0;
        try
        {
	        var tableName = GetTableName();
	        var keyColumn = GetKeyColumnName();
	        var keyProperty = GetKeyPropertyName();

            StringBuilder query = new StringBuilder();
            query.Append($"UPDATE {tableName} SET ");

            foreach (var property in GetProperties(true))
            {
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

                var propertyName = property.Name;
                var columnName = columnAttr.Name;

                query.Append($"{columnName} = @{propertyName},");
            }

            query.Remove(query.Length - 1, 1);

            query.Append($" WHERE {keyColumn} = @{keyProperty}");

            rowsEffected = await _connection.ExecuteAsync(query.ToString(), entity);
        }
        catch (Exception ex) { }

        return rowsEffected > 0;
    }

    //TODO
    public async Task<IEnumerable<T>> GetById(ulong id)
    {
        try
        {
	        var tableName = GetTableName();
            var columns = GetColumns(excludeKey: false);
            var columnsString = string.Join(", ", columns);
            var sql = $"SELECT ({columnsString}) FROM {tableName} WHERE Id = {id}";
            var results = await _connection.QueryAsync<T>(sql);

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
	        return null;
        }
    }

    private protected string GetTableName()
    {
	    var tableName = "";
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (tableAttr != null)
        {
            tableName = tableAttr.Name;
            return tableName;
        }

        return type.Name + "s";
    }

    private protected static string GetKeyColumnName()
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

    private protected string GetColumns(bool excludeKey = false)
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
}