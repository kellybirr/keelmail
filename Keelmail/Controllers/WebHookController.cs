using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Keelmail.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable CA1031 // Do not catch general exception types
namespace Keelmail.Controllers
{
    [Route("/")]
    public class WebHookController : ControllerBase
    {
        private readonly SmtpConfig _smtpConfig;
        private readonly string _clusterName;
        private readonly ILogger _logger;

        public WebHookController(IOptions<SmtpConfig> smtpConfig, IConfiguration config, ILogger<WebHookController> logger)
        {
            _smtpConfig = smtpConfig.Value;
            _clusterName = config["ClusterName"];
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return _smtpConfig.IsValid()
                ? Content($"ALIVE SINCE '{Program.StartedUtc:G}' ON '{Environment.MachineName}'", MediaTypeNames.Text.Plain)
                : (IActionResult)Problem("Invalid `Smtp` Configuration", statusCode: (int)HttpStatusCode.ServiceUnavailable);
        }

        [HttpPost]
        public IActionResult Post([FromBody]KeelPost model)
        {
            try
            {
                _logger.LogDebug($"{model.Name} @ {model.CreatedAt}");
                _logger.LogInformation(model.Message);

                var message = new MailMessage
                {
                    Priority = MailPriority.High,
                    From = new MailAddress(_smtpConfig.FromAddress, "Keel"),
                    To = { _smtpConfig.ToAddress},
                    Subject = model.Name ?? "",
                    Body = model.Message,
                    IsBodyHtml = false
                };

                if (! string.IsNullOrEmpty(_clusterName))
                    message.Subject += $" in {_clusterName}";

                var smtpClient = new SmtpClient
                {
                    Host = _smtpConfig.ServerUri.Host,
                    Port = _smtpConfig.ServerUri.Port,
                    EnableSsl = (_smtpConfig.ServerUri.Scheme == "smpts")
                };

                if (!string.IsNullOrEmpty(_smtpConfig.User))
                {
                    smtpClient.Credentials = new NetworkCredential
                    {
                        UserName = _smtpConfig.User,
                        Password = _smtpConfig.Password
                    };
                }

                smtpClient.SendMailAsync(message).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        _logger.LogInformation("Email Sent To: `{ToAddress}`", _smtpConfig.ToAddress);
                    }
                    else
                    {
                        _logger.LogError(
                            exception: t.Exception, 
                            message: "Send Failed To: `{ToAddress}`, Server: `{ServerUri}`",
                            args: new object[] { _smtpConfig.ToAddress, _smtpConfig.ServerUri }
                        );

                        t.Exception?.Handle(_ => true);
                    }

                    smtpClient.Dispose();
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notify Failed");

                return Problem(
                    detail: ex.Message, 
                    statusCode: (int)HttpStatusCode.InternalServerError
                    );
            }
        }
    }
}
