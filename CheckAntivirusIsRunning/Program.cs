using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CheckAntivirusIsRunning
{
    class Program
    {
        static void Main(string[] args)
        {

        }

        public static void sendEmail(string emailText)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ConnectionDetails.RecipientAddress);
            mail.From = new MailAddress(ConnectionDetails.SenderAddress, ConnectionDetails.DisplayName, System.Text.Encoding.UTF8);
            mail.Subject = ConnectionDetails.Subject;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = emailText;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = false;
            mail.Priority = MailPriority.High;
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(ConnectionDetails.AccountUserName, ConnectionDetails.AccountPassword);
            client.Port = 25;
            client.Host = ConnectionDetails.HostAddress;
            client.EnableSsl = false;
            try
            {
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Exception ex2 = ex;
                string errorMessage = string.Empty;
                while (ex2 != null)
                {
                    errorMessage += ex2.ToString();
                    ex2 = ex2.InnerException;
                }
            }
        }
    }
}
