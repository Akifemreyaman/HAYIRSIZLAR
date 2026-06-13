using System.Threading.Tasks;

namespace HayirsizlarApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
    }
}
