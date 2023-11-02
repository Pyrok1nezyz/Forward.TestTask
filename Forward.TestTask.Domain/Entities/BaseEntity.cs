using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Forward.TestTask.Domain.Entitys;

public abstract class BaseEntity
{
	[Key]
	public ulong Id { get; set; }
}