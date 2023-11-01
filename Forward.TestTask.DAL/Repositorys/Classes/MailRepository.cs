﻿using Forward.TestTask.DAL.Entitys;
using Forward.TestTask.DAL.Repositorys.Interfaces;
using Microsoft.Data.SqlClient;

namespace Forward.TestTask.DAL.Repositorys.Classes;

public class MailRepository : BaseRepository<Mail>
{
	public MailRepository(SqlConnection connection) : base(connection)
	{
		
	}
}