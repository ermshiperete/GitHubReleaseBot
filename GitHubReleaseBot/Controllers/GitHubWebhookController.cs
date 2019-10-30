using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace GitHubReleaseBot.Controllers
{
	[Route("webhooks/github")]
	public class GitHubWebhookController : Controller
	{
		private const    string                           Sha1Prefix = "sha1=";
		private readonly string                           _gitHubWebhookSecret;
		private readonly ILogger<GitHubWebhookController> _logger;

		public GitHubWebhookController(IConfiguration configuration, ILogger<GitHubWebhookController> logger)
		{
			_logger = logger;

			_gitHubWebhookSecret = configuration["GitHubWebhookSecret"];
		}

		private bool IsGitHubSignatureValid(string payload, string signatureWithPrefix)
		{
			if (string.IsNullOrWhiteSpace(payload))
				throw new ArgumentNullException(nameof(payload));
			if (string.IsNullOrWhiteSpace(signatureWithPrefix))
				throw new ArgumentNullException(nameof(signatureWithPrefix));

			if (!signatureWithPrefix.StartsWith(Sha1Prefix, StringComparison.OrdinalIgnoreCase))
				return false;

			var signature = signatureWithPrefix.Substring(Sha1Prefix.Length);
			var secret = Encoding.ASCII.GetBytes(_gitHubWebhookSecret);
			var payloadBytes = Encoding.ASCII.GetBytes(payload);

			using var hashMethod = new HMACSHA1(secret);
			var hash = hashMethod.ComputeHash(payloadBytes);

			var hashString = ToHexString(hash);

			return hashString.Equals(signature);
		}

		private static string ToHexString(byte[] bytes)
		{
			var builder = new StringBuilder(bytes.Length * 2);
			foreach (byte b in bytes)
			{
				builder.AppendFormat("{0:x2}", b);
			}

			return builder.ToString();
		}

		[HttpPost("")]
		public async Task<IActionResult> Receive()
		{
			Request.Headers.TryGetValue("X-GitHub-Delivery", out StringValues gitHubDeliveryId);
			Request.Headers.TryGetValue("X-GitHub-Event", out StringValues gitHubEvent);
			Request.Headers.TryGetValue("X-Hub-Signature", out StringValues gitHubSignature);

			_logger.LogInformation("Received GitHub delivery {GitHubDeliveryId} for event {gitHubEvent}", gitHubDeliveryId, gitHubEvent);

			using (var reader = new StreamReader(Request.Body))
			{
				var txt = await reader.ReadToEndAsync();

				if (IsGitHubSignatureValid(txt, gitHubSignature))
				{
					return Ok("works with configured secret!");
				}
			}

			return Unauthorized();
		}
	}

}