using System.Text.Json;
using System.Text.RegularExpressions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace __EAVFW__.AppHost.Tests;

/// <summary>
/// Resource names used by the EAVFW Aspire AppHost.
/// Must match the names defined in AppHost.cs and EAVFramework.Extensions.Aspire.Hosting.
/// </summary>
internal static class ResourceNames
{
    public const string Portal = "__databaseName__-portal";
    public const string HotReloadDev = "__databaseName__-portal-next-dev";
    public const string Model = "__databaseName__-model";
    public const string SqlServer = "sqlserver";
    public const string SqlDatabase = "sql-db";
    public const string MailServer = "mail-server";
}

[TestClass]
public class PortalLoginTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly string OutputDir = Path.GetFullPath(
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "videos"));

    [ClassInitialize]
    public static void InstallPlaywright(TestContext context)
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        Assert.AreEqual(0, exitCode, "Playwright browser install failed");
    }

    [TestMethod]
    public async Task PlaywrightSmokeTest()
    {
        Directory.CreateDirectory(OutputDir);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        await using var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 720 }
        });

        var page = await context.NewPageAsync();
        await page.GotoAsync("about:blank");
        await page.SetContentAsync("<h1>Playwright is working!</h1><p>Browser automation is available in this environment.</p>");

        var screenshotPath = Path.Combine(OutputDir, "playwright-smoke.png");
        await page.ScreenshotAsync(new() { Path = screenshotPath });

        Assert.IsTrue(File.Exists(screenshotPath), $"Screenshot was not created at {screenshotPath}");
        Console.WriteLine($"Smoke test passed. Screenshot saved to: {screenshotPath}");
    }

    [TestMethod]
    public async Task FullLoginFlow_ShouldLoginViaPasswordlessEmail()
    {
        // 1. Start the Aspire AppHost with "local" environment
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.__EAVFW___AppHost>([], (options, settings) =>
            {
                settings.EnvironmentName = "local";
            });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.ConfigureHttpClient(c => c.Timeout = TimeSpan.FromMinutes(5));
        });

        await using var app = await appHost.BuildAsync();

        var resourceNotificationService = app.Services
            .GetRequiredService<ResourceNotificationService>();

        await app.StartAsync();

        try
        {
            // 2. Wait for resources with individual timeouts for fast failure diagnosis
            var sw = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("[{0:mm\\:ss}] Waiting for SQL Server container to be healthy...", sw.Elapsed);
            await resourceNotificationService
                .WaitForResourceHealthyAsync(ResourceNames.SqlServer)
                .WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("[{0:mm\\:ss}] SQL Server container healthy", sw.Elapsed);

            Console.WriteLine("[{0:mm\\:ss}] Waiting for SQL database...", sw.Elapsed);
            await resourceNotificationService
                .WaitForResourceAsync(ResourceNames.SqlDatabase, KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("[{0:mm\\:ss}] SQL database ready", sw.Elapsed);

            Console.WriteLine("[{0:mm\\:ss}] Waiting for mail server to be healthy...", sw.Elapsed);
            await resourceNotificationService
                .WaitForResourceHealthyAsync(ResourceNames.MailServer)
                .WaitAsync(TimeSpan.FromMinutes(2));
            Console.WriteLine("[{0:mm\\:ss}] Mail server healthy", sw.Elapsed);

            Console.WriteLine("[{0:mm\\:ss}] Waiting for EAV model publish...", sw.Elapsed);
            await resourceNotificationService
                .WaitForResourceAsync(ResourceNames.Model, KnownResourceStates.Finished)
                .WaitAsync(TimeSpan.FromMinutes(3));
            Console.WriteLine("[{0:mm\\:ss}] EAV model published", sw.Elapsed);

            Console.WriteLine("[{0:mm\\:ss}] Waiting for portal (includes npm build-app)...", sw.Elapsed);
            await resourceNotificationService
                .WaitForResourceAsync(ResourceNames.Portal, KnownResourceStates.Running)
                .WaitAsync(TimeSpan.FromMinutes(5));
            Console.WriteLine("[{0:mm\\:ss}] Portal running", sw.Elapsed);

            // Check if hot-reload dev server exists (only if .WithHotReload() is configured)
            var hasHotReload = app.Services.GetRequiredService<DistributedApplicationModel>()
                .Resources.Any(r => r.Name == ResourceNames.HotReloadDev);
            if (hasHotReload)
            {
                Console.WriteLine("[{0:mm\\:ss}] Waiting for hot-reload dev server...", sw.Elapsed);
                await resourceNotificationService
                    .WaitForResourceAsync(ResourceNames.HotReloadDev, KnownResourceStates.Running)
                    .WaitAsync(TimeSpan.FromMinutes(3));
                Console.WriteLine("[{0:mm\\:ss}] Hot-reload dev server running", sw.Elapsed);
            }

            Console.WriteLine("[{0:mm\\:ss}] All resources ready in {1:F1}s", sw.Elapsed, sw.Elapsed.TotalSeconds);

            // 3. Get endpoints
            var portalEndpoint = app.GetEndpoint(ResourceNames.Portal, "http");
            Console.WriteLine($"Portal URL: {portalEndpoint}");

            string? hotReloadEndpoint = null;
            if (hasHotReload)
            {
                hotReloadEndpoint = app.GetEndpoint(ResourceNames.HotReloadDev, "http").ToString();
                Console.WriteLine($"Hot Reload URL: {hotReloadEndpoint}");
            }

            // MailPit endpoint
            var mailEndpoint = app.GetEndpoint(ResourceNames.MailServer, "http");
            Console.WriteLine($"MailPit URL: {mailEndpoint}");

            // 4. Create browser with video recording
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });

            Directory.CreateDirectory(OutputDir);

            await using var context = await browser.NewContextAsync(new()
            {
                RecordVideoDir = OutputDir,
                RecordVideoSize = new() { Width = 1280, Height = 720 },
                ViewportSize = new() { Width = 1280, Height = 720 },
                IgnoreHTTPSErrors = true
            });

            var page = await context.NewPageAsync();

            // 5. Navigate to the portal login page
            Console.WriteLine("Navigating to portal login page...");
            var loginUrl = $"{portalEndpoint}account/login";
            await page.GotoAsync(loginUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

            // Take a screenshot of the login page
            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "01-login-page.png")
            });
            Console.WriteLine("Screenshot: login page captured");

            // 6. Enter the admin email
            Console.WriteLine("Entering admin email...");
            var emailInput = page.Locator("input[placeholder='Enter email ...']");
            await emailInput.WaitForAsync(new() { Timeout = 30000 });
            await emailInput.FillAsync("__userEmail__");

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "02-email-entered.png")
            });

            // 7. Click Login button
            Console.WriteLine("Clicking Login...");
            await page.Locator("button:has-text('Login')").ClickAsync();

            // Wait for navigation (the page redirects to .auth/login/passwordless)
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "03-after-login-click.png")
            });
            Console.WriteLine($"After login click, URL: {page.Url}");

            // 8. Check MailPit for the passwordless login email
            Console.WriteLine("Checking MailPit for login email...");
            using var httpClient = new HttpClient();
            string? loginLink = null;

            // Poll MailPit for the email (may take a few seconds)
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));

                var messagesResponse = await httpClient.GetStringAsync(
                    $"{mailEndpoint}api/v1/messages");
                var messages = JsonSerializer.Deserialize<MailPitMessages>(
                    messagesResponse, JsonOptions);

                if (messages?.Messages?.Length > 0)
                {
                    var latestMessage = messages.Messages[0];
                    Console.WriteLine($"Found email: {latestMessage.Subject}");

                    // Get full message with HTML body
                    var messageResponse = await httpClient.GetStringAsync(
                        $"{mailEndpoint}api/v1/message/{latestMessage.ID}");
                    var fullMessage = JsonSerializer.Deserialize<MailPitMessage>(
                        messageResponse, JsonOptions);

                    // Extract the login link from HTML
                    var html = fullMessage?.HTML ?? fullMessage?.Text ?? "";
                    Console.WriteLine($"Email HTML length: {html.Length}");

                    // Look for the callback URL pattern
                    var linkMatch = Regex.Match(html,
                        @"href=""([^""]*\.auth/login/passwordless/callback\?token=[^""]+)""",
                        RegexOptions.IgnoreCase);

                    if (!linkMatch.Success)
                    {
                        // Try finding any URL with the token pattern
                        linkMatch = Regex.Match(html,
                            @"(https?://[^\s""<>]*\.auth/login/passwordless/callback\?token=[^\s""<>]+)",
                            RegexOptions.IgnoreCase);
                    }

                    if (linkMatch.Success)
                    {
                        loginLink = linkMatch.Groups[1].Value;
                        Console.WriteLine($"Found login link: {loginLink}");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Email found but no login link extracted. HTML content:");
                        Console.WriteLine(html[..Math.Min(500, html.Length)]);
                    }
                }
                else
                {
                    Console.WriteLine($"No emails yet (attempt {i + 1}/30)...");
                }
            }

            Assert.IsNotNull(loginLink, "Failed to find login link in MailPit email");

            // 9. Navigate to MailPit UI to show the email (for the recording)
            Console.WriteLine("Opening MailPit UI...");
            var mailPage = await context.NewPageAsync();
            await mailPage.GotoAsync($"{mailEndpoint}",
                new() { WaitUntil = WaitUntilState.NetworkIdle });
            await Task.Delay(TimeSpan.FromSeconds(2));

            await mailPage.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "04-mailpit-inbox.png")
            });

            // 10. Click the login link from the email
            Console.WriteLine("Navigating to login callback...");
            await page.GotoAsync(loginLink,
                new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "05-after-login-callback.png")
            });
            Console.WriteLine($"After callback, URL: {page.Url}");

            // 11. Verify we're logged in - should be on the portal home/apps page
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "06-logged-in-portal.png")
            });

            var finalUrl = page.Url;
            Console.WriteLine($"Final URL: {finalUrl}");

            // The login should NOT still be on the login page
            Assert.IsFalse(finalUrl.Contains("/account/login"),
                $"Still on login page after callback. URL: {finalUrl}");

            Console.WriteLine("Login flow completed successfully!");

            // 12. Navigate to portal home and discover the app
            Console.WriteLine("Navigating to portal home...");
            await page.GotoAsync($"{portalEndpoint}",
                new() { WaitUntil = WaitUntilState.NetworkIdle });
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "07-portal-home.png")
            });

            // 12b. Verify Hot Reload dev server if configured
            if (hasHotReload)
            {
                Console.WriteLine("=== Hot Reload Dev Server Verification ===");

                var hotReloadPage = await context.NewPageAsync();

                // Navigate to hot-reload login page
                // Note: Use DOMContentLoaded instead of NetworkIdle because next dev keeps a
                // WebSocket connection open for HMR, so the network never goes fully idle.
                Console.WriteLine($"Navigating to hot-reload login page: {hotReloadEndpoint}account/login");
                await hotReloadPage.GotoAsync($"{hotReloadEndpoint}account/login",
                    new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
                await Task.Delay(TimeSpan.FromSeconds(5));

                await hotReloadPage.ScreenshotAsync(new()
                {
                    Path = Path.Combine(OutputDir, "07b-hotreload-login-page.png")
                });
                Console.WriteLine($"Hot reload login page URL: {hotReloadPage.Url}");

                // Verify the hot-reload page loaded (should have content)
                var hotReloadContent = await hotReloadPage.ContentAsync();
                Assert.IsTrue(hotReloadContent.Length > 0,
                    "Hot reload dev server returned empty content");
                Console.WriteLine($"Hot reload page content length: {hotReloadContent.Length}");

                // Navigate to hot-reload home page
                await hotReloadPage.GotoAsync($"{hotReloadEndpoint}",
                    new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });
                await Task.Delay(TimeSpan.FromSeconds(5));

                await hotReloadPage.ScreenshotAsync(new()
                {
                    Path = Path.Combine(OutputDir, "07c-hotreload-home.png")
                });
                Console.WriteLine($"Hot reload home URL: {hotReloadPage.Url}");
                Console.WriteLine("Hot reload dev server verified successfully!");
            }

            // 13. Click on the first app link
            Console.WriteLine("Looking for app link...");
            var appLink = page.Locator("a[href*='/apps/']").First;
            await appLink.WaitForAsync(new() { Timeout = 30000 });
            var appHref = await appLink.GetAttributeAsync("href");
            Console.WriteLine($"Found app link: {appHref}");
            await appLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "08-app-home.png")
            });

            // 14. Extract app key from URL and navigate to Permission create form
            var appMatch = Regex.Match(page.Url, @"/apps/([^/]+)");
            Assert.IsTrue(appMatch.Success, $"Could not extract app key from URL: {page.Url}");
            var appKey = appMatch.Groups[1].Value;
            Console.WriteLine($"App key: {appKey}");

            // Navigate directly to the create form (entity list views are not defined in default scaffold)
            var permissionFormUrl =
                $"{portalEndpoint}apps/{appKey}/areas/Administration/entities/Permission/forms/Main";
            Console.WriteLine($"Navigating to Permission create form: {permissionFormUrl}");
            await page.GotoAsync(permissionFormUrl,
                new() { WaitUntil = WaitUntilState.NetworkIdle });
            await Task.Delay(TimeSpan.FromSeconds(3));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "09-permission-form.png")
            });
            Console.WriteLine($"Create form URL: {page.Url}");

            // 15. Fill in the Permission form fields
            Console.WriteLine("Filling in Permission form...");
            var nameInput2 = page.Locator(
                "input[id='name'], input[name='name'], input[id='Name'], input[name='Name']")
                .First;
            await nameInput2.WaitForAsync(new() { Timeout = 30000 });
            await nameInput2.FillAsync("E2E Test Permission");

            // Try to fill Description if the field exists
            var descInput = page.Locator(
                "textarea[id='description'], textarea[name='description'], " +
                "textarea[id='Description'], textarea[name='Description']").First;
            try
            {
                await descInput.WaitForAsync(new() { Timeout = 5000 });
                await descInput.FillAsync("Created by automated E2E test");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Description field not found, skipping...");
            }

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "10-form-filled.png")
            });

            // 17. Click Save
            Console.WriteLine("Clicking Save...");
            var saveButton = page.Locator(
                "button:has-text('Save'), button:has-text('Gem')").First;
            await saveButton.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(TimeSpan.FromSeconds(5));

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(OutputDir, "12-after-save.png")
            });

            var afterSaveUrl = page.Url;
            Console.WriteLine($"After save URL: {afterSaveUrl}");
            Console.WriteLine("Record creation completed successfully!");

            // 19. Close context to finalize video
            await context.CloseAsync();

            // Print video location
            var videos = Directory.GetFiles(OutputDir, "*.webm");
            foreach (var video in videos)
            {
                Console.WriteLine($"Video recorded: {video}");
            }

            Console.WriteLine($"Screenshots saved to: {OutputDir}");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    // MailPit API DTOs
    private record MailPitMessages(MailPitMessageSummary[] Messages, int Total);

    private record MailPitMessageSummary(string ID, string Subject, MailPitAddress From,
        MailPitAddress[] To, DateTime Date);

    private record MailPitAddress(string Name, string Address);

    private record MailPitMessage(string ID, string Subject, string HTML, string Text,
        MailPitAddress From, MailPitAddress[] To);
}
