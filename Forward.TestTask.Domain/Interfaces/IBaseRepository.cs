using System.Linq.Expressions;

namespace Forward.TestTask.Domain.Interfaces;

public interface IBaseRepository<T> where T : class
{
	Task<IEnumerable<T>> GetAll();
	public Task<bool> Add(T entity);
	public Task<bool> Delete(T entity);
	public Task<bool> Update(T entity);
}