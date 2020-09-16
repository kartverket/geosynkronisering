using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.UI.WebControls;
using Kartverket.Geosynkronisering.Database;
using NLog;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;


namespace Kartverket.Geosynkronisering
{
    /// <summary>
    ///  send email messages
    /// </summary>
    public class Mail
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// From https://github.com/jstedfast/MailKit nuget package MailKit.
        /// </summary>
        /// <param name="contents"></param>
        public void SendMail(string contents)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                var message = new MimeMessage();
                
                var mailFrom = appSettings["mailFrom"]; // "noreply@geosynkronisering.no";
                
                message.From.Add(new MailboxAddress(mailFrom, mailFrom));

                var mailto = ServiceData.EMail();
                message.To.Add(new MailboxAddress(mailto, mailto));
                message.Subject = "Feil motatt fra GeoSynkronisering abonnent i GeoSynkronisering tilbyder";
                message.Body = new TextPart("plain")
                {
                    Text = contents
                };

                var part = message.BodyParts.OfType<TextPart>().FirstOrDefault();
                var footer = "Dette er en automatisk utsendelse som det ikke kan svares på";
                part.Text += Environment.NewLine + Environment.NewLine + footer;

                using (var client = new SmtpClient())
                {
                    var host  = appSettings["SmtpHost"];
                    var port = appSettings["SmtpPort"];
                    var username = appSettings["SmtpUsername"];
                    var password = appSettings["SmtpPassword"];
                    if (host.Length > 0)
                    {
                        if (port.Length == 0)
                        {
                            client.Connect(host);
                        }
                        else
                        {
                           client.Connect(host:host, port:Convert.ToInt32(port), useSsl:false);
                        }

                        if (username.Length > 0 && password.Length > 0 )
                        {
                            // Note: only needed if the SMTP server requires authentication
                            client.Authenticate(username, password);
                        }
                    }
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"SendMail failed");
                throw;
            }
        }


    }
}