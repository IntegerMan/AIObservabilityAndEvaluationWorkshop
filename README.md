# AI Observability and Evaluation Workshop

Conducted by Matt Eland and Victor Frye from [Leading EDJE](https://LeadingEDJE.com)

See [AGENDA.md](AGENDA.md) for the workshop schedule and exercises.

## Setup and Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Tooling compatible with Aspire (VS 2022 / 2026), VS Code, Rider, Cursor, etc.](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling)

Some examples will use containerized resources. For these you will need either:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) * licensing may not be advisable on corporate machines
- [Podman](https://podman.io/)

We recommend launching the application in advance with a stable internet connection to allow the appropriate container images to be downloaded.

**Cloud Resource recommendations coming soon...**

### Troubleshooting

Here are a few tricky issues you may encounter when running the workshop code.

#### Inotify Limit Error on Linux

If you encounter the error:
```
System.IO.IOException: The configured user limit (128) on the number of inotify instances has been reached
```

This is a Linux system limitation. The Aspire AppHost creates file watchers for configuration files, which can exceed the default inotify limit.

**Solution:** Increase the system inotify limit:

```bash
# Temporary (until reboot)
sudo sysctl -w fs.inotify.max_user_instances=512

# Permanent
echo "fs.inotify.max_user_instances=512" | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

Alternatively, you can run the provided helper script:
```bash
./fix-inotify-limit.sh
```

#### SSL Connection Issues (UntrustedRoot) on Linux

If you are running on a machine with an SSL-inspecting proxy (common in some corporate environments or workshops) and you get an exception similar to:
`System.Security.Authentication.AuthenticationException: The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot`

**Solution:** Enable the `AllowUntrustedCertificates` parameter.

1. Locate the `AIObservabilityAndEvaluationWorkshop.AppHost/appsettings.json` file.
2. Under the `Parameters` section, set `"AllowUntrustedCertificates"` to `"true"`.

```json
{
  "Parameters": {
    "AllowUntrustedCertificates": "true"
  }
}
```

This will configure the application to bypass certificate validation for Azure and OpenAI connections.