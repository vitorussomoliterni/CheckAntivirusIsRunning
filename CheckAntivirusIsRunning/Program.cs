using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckAntivirusIsRunning
{
    class Program
    {
        static void Main(string[] args)
        {
            if (AntivirusExecutableMissing())
            {
                // TODO: Add what to do if antivirus executable is missing.
            }
            var machineInfo = GetMachineInfo();

        }

        private static bool AntivirusExecutableMissing()
        {
            try
            {
                if (File.Exists(ConnectionDetails.ExecutablePath))
                {
                    return true;
                }
                else
                {
                    throw new FileNotFoundException("No installation found");
                }
            }
            catch (Exception e)
            {
                sendEmail(e.ToString());
            }

            return false;
        }

        private static string GetMachineInfo()
        {
            string machineInfo = "Machine Name: " + Environment.MachineName;
            machineInfo += "\nUser: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name; // Gets logged in username
            machineInfo += "\nDate: " + DateTime.Now.ToLongDateString();
            machineInfo += "\nTime: " + DateTime.Now.ToLongTimeString();

            return machineInfo;
        }

        private static string GetUserName()
        {
            var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name; // Gets logged in username

            while (userName == null || userName == string.Empty)
            {
                Thread.Sleep(600000); // Pauses the script for 10 minutes (600000 milliseconds)
                userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name; // Gets logged in username
            }

            return userName;
        }

        public static void sendEmail(string emailText)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(ConnectionDetails.RecipientAddress);
            mail.From = new MailAddress(ConnectionDetails.SenderAddress, ConnectionDetails.DisplayName, Encoding.UTF8);
            mail.Subject = ConnectionDetails.Subject;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = emailText;
            mail.BodyEncoding = Encoding.UTF8;
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
