using Forward.TestTask.DAL;
using Forward.TestTask.MailClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask
{
	internal class Program
	{
		static void Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
			var mailManager = new MailClientManager(configuration);

		}
	}
}