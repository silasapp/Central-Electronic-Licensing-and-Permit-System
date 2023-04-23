using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mail;
using System.Configuration;


namespace ELPS.Helpers
{
    public class MailHelper
    {
        public static void SendEmail(string to, string subject, string message, string bcc = "")
        {
            var loginInfo = new System.Net.NetworkCredential(EmailSettings.Username, EmailSettings.Password);
            var smtpClient = new SmtpClient(EmailSettings.host, EmailSettings.ServerPort);
            smtpClient.EnableSsl = EmailSettings.UseSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = loginInfo;

            var msg = new System.Net.Mail.MailMessage();
            msg.From = new MailAddress(EmailSettings.MailFromAddress);
            msg.To.Add(new MailAddress(to));
            if(!string.IsNullOrEmpty(bcc))
                msg.Bcc.Add(new MailAddress(bcc));
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = true;

            if (EmailSettings.WriteAsFile)
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtpClient.PickupDirectoryLocation = EmailSettings.FileLocation;
                smtpClient.EnableSsl = false;
            }

            try
            {
                smtpClient.Send(msg);
            }
            catch (SmtpException ex)
            {
                //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                //Do nothing
                var rt = ex;
            }
        }

        public static void SendEmail(string to, string subject, string message, Attachment attachment)
        {
            var loginInfo = new System.Net.NetworkCredential(EmailSettings.Username, EmailSettings.Password);
            var smtpClient = new SmtpClient(EmailSettings.host, EmailSettings.ServerPort);
            smtpClient.EnableSsl = EmailSettings.UseSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = loginInfo;

            var msg = new System.Net.Mail.MailMessage();
            msg.From = new MailAddress(EmailSettings.MailFromAddress);
            msg.To.Add(new MailAddress(to));
            //msg.Bcc.Add(EmailSettings.bcc);
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = true;
            msg.Attachments.Add(attachment);

            if (EmailSettings.WriteAsFile)
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtpClient.PickupDirectoryLocation = EmailSettings.FileLocation;
                smtpClient.EnableSsl = false;
            }
            smtpClient.Send(msg);

        }

        public static Task SendMailAsync(string to, string subject, string message)
        {
            var loginInfo = new System.Net.NetworkCredential(EmailSettings.Username, EmailSettings.Password);
            var smtpClient = new SmtpClient(EmailSettings.host, EmailSettings.ServerPort);
            smtpClient.EnableSsl = EmailSettings.UseSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = loginInfo;

            var msg = new System.Net.Mail.MailMessage();
            msg.From = new MailAddress(EmailSettings.MailFromAddress);
            msg.To.Add(new MailAddress(to));
            //msg.Bcc.Add(EmailSettings.bcc);
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = true;

            if (EmailSettings.WriteAsFile)
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtpClient.PickupDirectoryLocation = EmailSettings.FileLocation;
                smtpClient.EnableSsl = false;
            }

            var x = smtpClient.SendMailAsync(msg);


            return x;
        }
    }




    public class EmailSettings
    {
        public static string MailToAddress = ConfigurationManager.AppSettings["mailSender"];
        public static string MailFromAddress = ConfigurationManager.AppSettings["mailSender"];
        public static string bcc = ConfigurationManager.AppSettings["MailBcc"];
        public static bool UseSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSsl"]);
        public static string Username = ConfigurationManager.AppSettings["UserName"];
        public static string Password = ConfigurationManager.AppSettings["mailpass"];
        public static string host = ConfigurationManager.AppSettings["mailHost"];
        public static int ServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServrPort"]);//465; 26;
        //public static bool WriteAsFile = true;
        public static bool WriteAsFile = Convert.ToBoolean(ConfigurationManager.AppSettings["WFile"]);
        public static string FileLocation = @"c:\MyTempEmails";
    }



}