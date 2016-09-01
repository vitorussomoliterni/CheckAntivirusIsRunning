using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static string _errorLog;

        static void Main(string[] args)
        {
            //CheckIfMachineIsToIgnore(MachinesToIgnore);

            var machineInfo = GetMachineInfo();

            //if (AntivirusExecutableMissing())
            //{
            //    var emailText = machineInfo + _errorLog;
            //    sendEmail(emailText); // TODO: Add error to log file
            //    Environment.Exit(0);
            //}

            //Thread.Sleep(600000); // Pauses the thread for 10 minutes (600000 milliseconds)

            var userName = "User name: " + GetUserName() + "\n";

            if (!ProcessesAreRunning(ConnectionDetails.Processes))
            {
                var emailText = userName + machineInfo + _errorLog;
                sendEmail(emailText);
            }
        }

        private static bool ProcessesAreRunning(List<string> processes)
        {
            foreach (var process in processes)
            {
                var processFound = false;
                foreach (var p in Process.GetProcesses())
                {
                    if (p.ProcessName.Trim().ToLower().Contains(process))
                    {
                        processFound = true;
                        break;
                    }
                }

                if (processFound == false)
                {
                    _errorLog = "Process error: " + process + " is not running.";
                    return false;
                }
            }

            return true;
        }

        private static void CheckIfMachineIsToIgnore(List<string> machinesToIgnore)
        {
            var currentMachine = Environment.MachineName.ToLower().Trim();

            foreach (var item in machinesToIgnore)
            {
                if (item.Equals(currentMachine))
                {
                    Environment.Exit(0);
                }
            }
        }

        private static bool AntivirusExecutableMissing()
        {
            try
            {
                if (File.Exists(ConnectionDetails.ExecutablePath))
                {
                    return false;
                }
                else
                {
                    throw new FileNotFoundException("Installation error: the executable was not found.");
                }
            }
            catch (Exception e)
            {
                _errorLog += e.Message;
            }

            return true;
        }

        private static string GetMachineInfo()
        {
            string machineInfo = "Machine Name: " + Environment.MachineName + "\n";
            machineInfo += "Date: " + DateTime.Now.ToLongDateString() + "\n";
            machineInfo += "Time: " + DateTime.Now.ToLongTimeString() + "\n";

            return machineInfo;
        }

        private static string GetUserName()
        {
            var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name; // Gets logged in username

            while (string.IsNullOrEmpty(userName))
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
