﻿
using MailKit.Net.Pop3;
using MailPractice.Settings;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
// using System.Net.Mail;
using System.Threading.Tasks;

namespace MailPractice.MailsManager
{
    public class MailManager : IMailManager
    {
        public static int num = 0;

        public readonly IEmailSettings settings;
        public ILogger<MailManager> Logger { get; set; }
        public MailManager(IEmailSettings settings, ILogger<MailManager> Logger)
        {
            this.settings = settings;
            this.Logger = Logger;
            
        }
        public async Task<List<MimeMessage>> Receive()
        {
            Pop3Client client = new Pop3Client();
            await client.ConnectAsync(settings.Pop3Server,settings.Pop3Port,true);
            await client.AuthenticateAsync(settings.Pop3Username,settings.SmtpPassword);
            List<MimeMessage> messages = new List<MimeMessage>();
            Logger.LogInformation($"Count of messages : {client.Count}");
            
            for (int i = client.Count-1; i >=0 ; i--)
            {
                 messages.Add(await client.GetMessageAsync(i));
            }
            await client.DeleteAllMessagesAsync();
            await client.DisconnectAsync(true);
            client.Dispose();
            return messages;
        }
        public async Task Send(MimeMessage message)
        {
            var smtp = new SmtpClient();
            await smtp.ConnectAsync(settings.SmtpServer, settings.SmtpPort,true);
            await smtp.AuthenticateAsync(settings.SmtpUsername,settings.SmtpPassword);
            num += 1;
            smtp.MessageSent += Smtp_MessageSent;
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
            
            smtp.Dispose();

        }

        private void Smtp_MessageSent(object sender, MailKit.MessageSentEventArgs e)
        {
            Logger.LogInformation($"Successfully sent {num}  {DateTime.Now}");
        }

       
    }
}
