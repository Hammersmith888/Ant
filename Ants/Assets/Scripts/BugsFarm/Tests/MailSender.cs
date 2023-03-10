using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;


public class MyStringStream : MemoryStream
{
	protected override void Dispose( bool disposing )					// (!!!) Never called
	{
		string message			= "MyStringStream.Dispose() call";

		Debug.Log( message );				// for testing in Unity
		Console.WriteLine( message );		// for testing outside of Unity
	}
}


public static class MailSender
{
	public static void Send( string fromEmail, string password, string toEmail )
	{
		using (
				MailMessage mail			= new MailMessage
				{
					From					= new MailAddress( fromEmail ),
					Subject					= "Test",
					Body					= "Hello!"
				}
			)
		using (mail.Attachments)										// (!!!) Uncomment this to make MyStringStream.Dispose() called
		{
			mail.To.Add( toEmail );

			mail.Attachments.Add( new Attachment( new MyStringStream(), "empty.txt" ) );

			using (
					SmtpClient smtpClient	= new SmtpClient
					{
						Host				= "smtp.gmail.com",
						Port				= 587,
						DeliveryMethod		= SmtpDeliveryMethod.Network,
						Credentials			= new NetworkCredential( fromEmail, password ),
						EnableSsl			= true
					}
				)
			{
				ServicePointManager.ServerCertificateValidationCallback			= delegate { return true; };

				smtpClient.Send( mail );
			}
		}
		
		
	}
}


