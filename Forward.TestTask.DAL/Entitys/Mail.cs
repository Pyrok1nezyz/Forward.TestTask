using System.ComponentModel.DataAnnotations.Schema;

namespace Forward.TestTask.DAL.Entitys;
[Table("email_Message")]
public class Mail
{
    public int Id { get; init; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Headers { get; set; }
    public string? Sender { get; set; }
    public string? Recipients { get; set; }
}

/*2. Для каждого, подходящего по регулярному выражению письма, необходимо выполнить sql-скрипт, оперирующий параметрами:
- @EMail_Title varchar(MAX) (Заголовок письма);
- @EMail_Body varchar(MAX) (Содержимое письма);
- @EMail_Headers varchar(MAX) (Заголовки почтового сообщения);
- @EMail_Sender varchar(MAX) (Отправитель);
- @EMail_Recipients varchar(MAX) (Список получателей).*/