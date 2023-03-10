using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
// using Firebase.Crashlytics;
using UnityEngine;

/*
	https://stackoverflow.com/questions/46272657/how-to-export-android-unity-error-and-debug-logs-to-file
	https://stackoverflow.com/questions/4657974/how-to-generate-unique-file-names-in-c-sharp/4657981
*/

public class LogSaverAndSender : MB_Singleton<LogSaverAndSender>
{
    const string EmailTo = "bugsfarmgame@gmail.com";
    const string EmailFrom = "d.sankov.sender@gmail.com";
    const string Password = "dokoygihoweekiid";

    const int MaxFiles = 25;
    const int MaxDays = 7;

    string _log;
    int? _frameCount;
    HashSet<LogType> _logTypes = new HashSet<LogType>();

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Not spamming same LogTypes in different frames
        if (
            _frameCount.HasValue &&
            _frameCount != Time.frameCount &&
            _logTypes.Contains(type)
            )
            return;


        bool isNormal = type == LogType.Log;


        // Log
        var trace = new System.Diagnostics.StackTrace(6);
        _log =
            logString + "\n\n" +
            (isNormal ? "" : "Unity:\n" + stackTrace + "\n\n") +
            (isNormal ? "" : "System.Diagnostics.StackTrace:\n" + trace + "\n\n") +
            _log
            ;
        _logTypes.Add(type);


        // If not just LogType.Log, then open "Oops" panel next frame
        if (
            !isNormal
        #if UNITY_EDITOR
            && type != LogType.Warning // To fix annoying warning from Facebook SDK
        #endif
            )
            _frameCount = Time.frameCount;
    }

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        Application.logMessageReceived += HandleLog;
    }

    void Update()
    {
        if (_frameCount != Time.frameCount - 1)
            return;

        Oops();
    }

    public void Oops()
    {
        const bool openOopsPanel = false;
        const bool dumpLogOnDisk = false;

        // Open "Oops" panel
        // if (
        //     openOopsPanel &&
        //     UI_Control.Instance
        //     )
        // {
        //     UI_Control.Instance.Open(PanelID.Log);
        //     Panel_Log.Instance.SetText(_log);
        // }


        if (dumpLogOnDisk)
        {
            // Dump log on disk
            string folder = Path.Combine(Application.persistentDataPath, "Logs");
            SavingSystem.SaveText(
                                  Path.Combine(folder, GenerateFileName("Log_", ".log")),
                                  _log
                                 );

            // Delete old logs
            var oldLogs =
                    Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                        .Select(fileName => new FileInfo(fileName))
                        .OrderByDescending(fileInfo => fileInfo.CreationTime)
                        .Skip(MaxFiles)
                        .Select(x => x)
                        .Where(x => (DateTime.Now - x.CreationTime).TotalDays > MaxDays)
                ;
            foreach (FileInfo oldLog in oldLogs)
                oldLog.Delete();
        }


        // Send mail
        BugsFarm.Game.GameInit.CopySaveFile(BugsFarm.Game.GameInit.SaveFilePath_Last);

        string subject =
            $"Log - deviceType: {SystemInfo.deviceType}, deviceModel: {SystemInfo.deviceModel}, deviceName: {SystemInfo.deviceName}";
        string body =
                $"Application.identifier: {Application.identifier}\n" +
                $"Application.version: {Application.version}\n" +
                $"SaveFormatVer: {BugsFarm.Game.GameInit.SaveFormatVer}\n" +
                $"Simulation.type: {SimulationOld.Type}\n\n" +
                $"{_log}"
            ;
        string[] attachments = {BugsFarm.Game.GameInit.SaveFilePath, BugsFarm.Game.GameInit.SaveFilePath_AtLoad, BugsFarm.Game.GameInit.SaveFilePath_Last};
        SendMail(EmailFrom, Password, EmailTo, subject, body, attachments);


        // Add log to Firebase Crashlytics
        try
        {
            // Crashlytics.Log( body );
        }
        catch (Exception)
        {
            /* ignore */
        }
    }

    void SendMail(string fromEmail, string password, string toEmail, string subject, string body,
                  string[] attachmentPaths = null)
    {
        try
        {
            using (MailMessage mail = new MailMessage())
            using (mail.Attachments) // (!) Important (!)
            {
                mail.From = new MailAddress(fromEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.To.Add(toEmail);

                if (attachmentPaths != null)
                    foreach (string attachmentPath in attachmentPaths)
                        if (File.Exists(attachmentPath))
                            mail.Attachments.Add(new Attachment(attachmentPath));

                using (
                    SmtpClient smtpClient = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(fromEmail, password),
                        EnableSsl = true
                    }
                    )
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    smtpClient.Send(mail);
                }
            }
        }
        catch (Exception e)
        {
            string str = $"Failed to send email: {e}";
            _log = str + "\n\n" + _log;

            Panel_Log.Instance.SetText(_log);

            Debug.Log(str);
        }
    }

    string GenerateFileName(string prefix, string extension = "")
    {
        return
            prefix +
            DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff") + "_" +
            Guid.NewGuid().ToString("N") +
            extension
            ;
    }
}