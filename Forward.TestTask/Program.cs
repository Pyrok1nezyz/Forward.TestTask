using Forward.TestTask.MailClient;
using Microsoft.Extensions.Configuration;

namespace Forward.TestTask
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var dir = Directory.GetCurrentDirectory();
			IConfiguration configuration = new ConfigurationBuilder()
				.SetBasePath(dir)
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables().Build();

			var mailManager = new MailClientManager(configuration, true);
			mailManager.InitWatchers(false);
		}
	}
}