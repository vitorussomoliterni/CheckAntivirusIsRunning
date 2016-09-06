using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Management;
using System.Linq;

namespace CheckAntivirusIsRunning
{
    class Program
    {
        public static string _errorLog;
        public static DateTime _startDate = DateTime.Now;

        static void Main(string[] args)
        {
            CheckIfMachineIsToIgnore(ConnectionDetails.MachinesToIgnore);

            var machineInfo = GetMachineInfo();

            if (AntivirusExecutableMissing())
            {
                var emailText = machineInfo + _errorLog;
                sendEmail(emailText);
                Log(emailText);
                Environment.Exit(0);
            }

            WaitUserToBeLoggedIn();
            
            if (!ProcessesAreRunning(ConnectionDetails.Processes))
            {
                machineInfo = "User name: " + GetUserName() + "\n" + machineInfo;
                var emailText = machineInfo + _errorLog;
                sendEmail(emailText);
                Log(emailText);
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

        private static bool WaitUserToBeLoggedIn()
        {
            var userName = GetUserName();

            while (string.IsNullOrEmpty(userName))
            {
                var elapsedTime = DateTime.Now - _startDate;
                if (elapsedTime.Minutes > 60) // Closes the script if it has been running pointlessly for over an hour.
                {
                    Environment.Exit(0);
                }
                Thread.Sleep(600000); // Pauses the script for 10 minutes (600000 milliseconds)
                userName = GetUserName();
            }

            return true;
        }

        private static string GetUserName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();

            if (collection.Count == 0)
            {
                return null;
            }

            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];

            return username;
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

        public static void Log(string logMessage)
        {
            try
            {
                using (StreamWriter w = File.AppendText(ConnectionDetails.LogFilePath))
                {
                    w.WriteLine(logMessage);
                    w.WriteLine("---------------------------------------------------------------------------\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
