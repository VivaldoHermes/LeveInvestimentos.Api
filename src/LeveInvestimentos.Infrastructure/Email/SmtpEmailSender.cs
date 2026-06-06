using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeveInvestimentos.Core.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LeveInvestimentos.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateMessage(to, subject, body);
        ValidateOptions();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject.Trim();
        message.Body = new TextPart("plain")
        {
            Text = body.Trim(),
        };

        using var client = new SmtpClient();
        var socketOptions = _options.UseSsl
            ? SecureSocketOptions.Auto
            : SecureSocketOptions.None;

        await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await client.AuthenticateAsync(
                _options.UserName,
                GetPassword(),
                cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private string GetPassword()
    {
        if (!string.Equals(_options.Host, "smtp.gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            return _options.Password;
        }

        return new string(_options.Password.Where(character => !char.IsWhiteSpace(character)).ToArray());
    }

    private static void ValidateMessage(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("Recipient e-mail is required.", nameof(to));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("E-mail subject is required.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("E-mail body is required.", nameof(body));
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException("SMTP host was not configured.");
        }

        if (_options.Port <= 0)
        {
            throw new InvalidOperationException("SMTP port must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException("SMTP sender e-mail was not configured.");
        }
    }
}
