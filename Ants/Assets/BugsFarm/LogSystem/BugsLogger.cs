using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Text;
using BugsFarm.ReloadSystem;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.Logger
{
    public class BugsLogger 
    {
        private readonly IDisposable _gameReloadingEvent;
        private static StringBuilder _builder;
        private readonly double _logNum;

        public BugsLogger()
        {
            _gameReloadingEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);
            _builder = new StringBuilder();
            _logNum = Tools.UtcNow();
        }

        public void Initialize()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnGameReloading(GameReloadingReport report)
        {
            _gameReloadingEvent.Dispose();
            Dispose();
        }

        public static void AddToLog(string logText)
        {
            _builder.Append($"{logText}\n");
        }
        private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            _builder.Append("+----------------------------------+\n");
            _builder.Append($"{type.ToString()}\n");
            _builder.Append("+----------------------------------+\n");
            _builder.Append($"{condition}\n");
            _builder.Append("+----------------------------------+\n");
            _builder.Append($"{stacktrace}\n");

            if (type == LogType.Error)
            {
                CreateLogFile();
               //Log();
            }
            else if (type == LogType.Exception)
            {
                CreateLogFile();
                SendReport();
            }
        }

        private void CreateLogFile()
        {
            var path = Application.persistentDataPath + $"/BugsFarmLog{_logNum}.txt";
            using (StreamWriter writer = File.CreateText(path))
            {
                writer.WriteLine(_builder.ToString());
            }
        }

        private void SendReport()
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(
                "vingergrint@gmail.com", 
                "Dnkrbhki1");
            client.EnableSsl = true;
            MailAddress from = new MailAddress(
                "vingergrint@gmail.com",
                $"UnityReport at {Tools.UtcNow()}",
                System.Text.Encoding.UTF8);
            MailAddress to = new MailAddress("badcode@internet.ru");
            MailMessage message = new MailMessage(from, to);
            message.Body = _builder.ToString();
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = "Bug report";
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            string userState = "Bug report";
            client.SendAsync(message, userState);
        }
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                Debug.Log("Send canceled "+ token);
            }
            if (e.Error != null)
            {
                Debug.Log("[ "+token+" ] " + " " + e.Error.ToString());
            }
            else
            {
                Debug.Log("Message sent.");
            }
        }

        private void Dispose()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
        
        
    }

}
