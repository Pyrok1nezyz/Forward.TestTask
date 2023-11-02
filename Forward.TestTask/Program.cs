using Forward.TestTask.MailClient;
using Microsoft.Extensions.Configuration;
using Topshelf;

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

			HostFactory.Run(hostConfigurator =>
			{
				hostConfigurator.SetDisplayName("SimpleMailService");
				hostConfigurator.SetServiceName("SimpleMailService");
				hostConfigurator.SetDescription(
					"Сервис парсит сообщения с почт расположенных в таблице email и закидывает сообщения в email_message");

				hostConfigurator.RunAsLocalSystem();

				hostConfigurator.Service<MailClientManager>(serviceConfigurator =>
				{
					serviceConfigurator.ConstructUsing(() => new MailClientManager(configuration, false));

					serviceConfigurator.WhenStarted(service => service.InitWatchers(false));
					serviceConfigurator.WhenStopped(service => service.Stop());
				});
			});
		}
	}
}