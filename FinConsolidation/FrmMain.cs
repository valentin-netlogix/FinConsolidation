

using Microsoft.Data.SqlClient;
using Microsoft.Web.WebView2.Core;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;


namespace FinConsolidation
{
    public partial class FrmMain : Form
    {

        private string appMode = "AP";
        private string appTitle = "Fin.Consolidation";
        private string sqlServer = "";
        private string sqlDatabase = "";

        private enum Env { Live, Uat }
        private Env _env = Env.Live;
        private string _serverLive = "";
        private string _dbLive = "";
        private string _serverUat = "";
        private string _dbUat = "";
        private bool _envConnected = false;
        private CancellationTokenSource? _connTestCts;
        private CancellationTokenSource? _currentLoadCts;
        private bool _suppressEnvHandler = false;

        private int connectTimeout = 5;
        private bool trustServerCertificate = false;
        private bool _webViewReady = false;
        private bool _isRunning = false;
        
        private readonly CultureInfo _nz = new("en-NZ");

        private TaskCompletionSource<bool>? _renderCompleteTcs;
        private const int RenderCompleteTimeoutMs = 5000;
        private const string UiDash = "—";

        private const string PromptEnterTrip = "Please enter a Trip Code before applying APC.";
        private const string PromptEnterCons = "Please enter a Consignment before applying ARC.";
        private const string PromptEnterTripRef = "Please enter a Trip Code before refreshing rates.";
        private const string PromptEnterConsRef = "Please enter a Consignment before refreshing rates.";


        private sealed class TableTotals
        {
            public int RowCount { get; set; }
            public int GroupCount { get; set; }
            // Finance totals
            public decimal TVAmt { get; set; }
            public decimal ConAmt { get; set; }
            public decimal CalcAmt { get; set; }
            public bool HasConAmt { get; set; }
            public decimal AfterCombinedAmt { get; set; }
            public string? Carrier { get; set; }
            public string? ConPass { get; set; }
            public string? CalcPass { get; set; }

        }


        private enum EnumActionBtn
        {
            BtnFinChoiceAP,
            BtnFinChoiceAR,
            BtnItemSearch,
            BtnApplyCon,
            BtnRefreshRates
        }


        public FrmMain()
        {
            InitializeComponent();

            ResetActionButtons();

            RbEnvLive.CheckedChanged += EnvRadio_CheckedChanged;
            RbEnvUat.CheckedChanged += EnvRadio_CheckedChanged;

            // suppress the handler while we set the initial radio
            _suppressEnvHandler = true;
            RbEnvLive.Checked = true;       // default selection
            _suppressEnvHandler = false;


            BtnFinChoiceAP.Enabled = false;
            BtnFinChoiceAR.Enabled = false;


            _serverLive = ConfigurationManager.AppSettings["SqlServerName_Live"]
                       ?? ConfigurationManager.AppSettings["SqlServerName"]
                       ?? throw new InvalidOperationException("Missing SqlServerName or SqlServerName_Live.");

            _dbLive = ConfigurationManager.AppSettings["SqlDatabaseName"]
                   ?? ConfigurationManager.AppSettings["SqlDatabaseName"]
                   ?? throw new InvalidOperationException("Missing SqlDatabaseName or SqlDatabaseName.");

            _serverUat = ConfigurationManager.AppSettings["SqlServerName_Uat"]
                      ?? _serverLive;

            _dbUat = ConfigurationManager.AppSettings["SqlDatabaseName"]
                  ?? _dbLive;

            ApplyEnvironmentToConnectionFields();

            this.Text = appTitle;
            LblAppTitle.Text = appTitle;

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("FinConsolidation.Resources.fin_con.ico");

            LblSelInd1.BackColor = SystemColors.Control;
            LblSelInd2.BackColor = SystemColors.Control;

            BtnFinChoiceAP.Click += BtnFinChoice_Click;
            BtnFinChoiceAR.Click += BtnFinChoice_Click;
            BtnItemSearch.Click += BtnItemSearch_Click;
            BtnApplyCon.Click += BtnApplyCon_Click;
            BtnRefreshRates.Click += BtnRefreshRates_Click;

            BtnFinChoiceAP.Tag = EnumActionBtn.BtnFinChoiceAP;
            BtnFinChoiceAR.Tag = EnumActionBtn.BtnFinChoiceAR;
            
            BtnItemSearch.Enabled = false;

            TxtItemSearch.TextChanged += (_, __) => UpdateSearchEnabled();

            PnlItemSearch.Visible = false;
            PnlModRates.Visible = false;
            PnlTotals.Visible = false;

            if (!int.TryParse(ConfigurationManager.AppSettings["SqlConnectTimeoutSeconds"], out connectTimeout)) connectTimeout = 60;

            trustServerCertificate = string.Equals(
                    ConfigurationManager.AppSettings["TrustServerCertificate"],
                    "true", StringComparison.OrdinalIgnoreCase);

            var appUser = WindowsIdentity.GetCurrent().Name;
            var user = appUser.Contains('\\') ? appUser[(appUser.LastIndexOf('\\') + 1)..] : appUser;
    
            LblUser.Text = user;
            LblStatus.Text = "Not connected";
            ProgBar.Visible = false;

            this.Shown += FrmMain_ShownAsync;

            // Show three-part version in LblVersion
            try
            {
                // Prefer the informational/version metadata (maps to <Version> / <InformationalVersion>)
                var asm = Assembly.GetExecutingAssembly();
                var infoAttr = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                string? productVer = infoAttr?.InformationalVersion;

                // Fallback to assembly name (may be 4-part); we'll trim to 3 parts if needed
                if (string.IsNullOrWhiteSpace(productVer))
                {
                    var v = asm.GetName().Version; // e.g., 1.2.3.0
                    if (v != null)
                        productVer = $"{v.Major}.{v.Minor}.{v.Build}";
                    else
                        productVer = "0.0.0";
                }
                else
                {
                    // If informational has a suffix (e.g., "1.2.3-beta"), keep it as-is for About boxes,
                    // but your requirement is strictly three-part. We'll strip any suffix here.
                    // Also, if someone accidentally set a 4-part version, we'll reduce it to 3.
                    var parts = productVer.Split('-', '+')[0]           // drop pre-release/build metadata
                                          .Split('.', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                        productVer = $"{parts[0]}.{parts[1]}.{parts[2]}";
                    else
                        productVer = "0.0.0";
                }

                LblVersion.Text = $"v{productVer}";
            }
            catch
            {
                LblVersion.Text = "v0.0.0";
            }
        }

        // Call this whenever you switch modes (AR <-> AP) or want to clear the screen.
        private void ResetUiForModeChange()
        {
            // 1) Cancel any inflight work (if you load data asynchronously)
            try
            {
                _currentLoadCts?.Cancel();
            }
            catch { /* ignored */ }
            finally
            {
                _currentLoadCts?.Dispose();
                _currentLoadCts = null;
            }

            // 2) Clear the search box and return focus there (optional)
            TxtItemSearch.Text = string.Empty;
            TxtItemSearch.Focus();

            // 3) Clear totals / badges / counters / status
            LblEntity.Text = UiDash;
            LblAmtBefore.Text = UiDash;
            LblAmtAfter.Text = UiDash;
            LblStatus.Text = "Ready";       // or "—" if you prefer a blank status

            // If you have checkboxes, filters, etc. that should reset per mode:
            // ChkShowExcluded.Checked = false;
            // CmbSort.SelectedIndex = 0;

            // 4) Clear the WebView2 content IN-PLACE (keep index.html + app.js loaded)
            if (WebView?.CoreWebView2 != null)
            {
                _ = WebView.CoreWebView2.ExecuteScriptAsync(@"
                    (function(){
                        const thead = document.getElementById('thead');
                        const tbody = document.getElementById('tbody');
                        const status = document.getElementById('status');
                        const tbl = document.getElementById('tbl');

                        if (thead) thead.innerHTML = '';
                        if (tbody) tbody.innerHTML = '';
                        if (status) status.textContent = '';
                        if (tbl) tbl.querySelectorAll('colgroup').forEach(cg => cg.remove());
                    })();
                ");
            }
        }

        private void ApplyEnvironmentToConnectionFields()
        {

            if (_env == Env.Live) { sqlServer = _serverLive; sqlDatabase = _dbLive; }
            else { sqlServer = _serverUat; sqlDatabase = _dbUat; }

        }

        private static async Task<bool> TestSqlConnectivityBoundedAsync(
            string connStr, int timeoutSeconds, CancellationToken token)
        {
            var openTask = Task.Run(async () =>
            {
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync(token).ConfigureAwait(false);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = Math.Max(5, timeoutSeconds);
                await cmd.ExecuteScalarAsync(token).ConfigureAwait(false);

                return true;
            }, token);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), token);

            var winner = await Task.WhenAny(openTask, timeoutTask).ConfigureAwait(false);
            if (winner == openTask)
            {
                // propagate exception if any
                return await openTask.ConfigureAwait(false);
            }
            // timed out: return false (caller updates UI)
            return false;
        }

        private void ShowConnProgress()
        {
            var owner = ProgBar.GetCurrentParent(); // StatusStrip
            if (owner != null && !owner.Visible) owner.Visible = true;

            ProgBar.Style = ProgressBarStyle.Marquee;
            ProgBar.MarqueeAnimationSpeed = 30;
            ProgBar.Visible = true;

            owner?.PerformLayout();
            owner?.Refresh();
        }

        private void HideConnProgress()
        {
            ProgBar.MarqueeAnimationSpeed = 0;
            ProgBar.Style = ProgressBarStyle.Blocks;
            ProgBar.Visible = false;

            var owner = ProgBar.GetCurrentParent();
            owner?.PerformLayout();
            owner?.Refresh();
        }

        private async Task TriggerEnvConnectionCheckAsync()
        {
            // Disable AP/AR while checking
            BtnFinChoiceAP.Enabled = false;
            BtnFinChoiceAR.Enabled = false;
            _envConnected = false;

            // Show ToolStrip progress
            ShowConnProgress();
            await Task.Yield(); // let the UI paint the status strip + progress bar

            // Cancel any in-flight env check (use your dedicated CTS if you added it)
            _connTestCts?.Cancel();
            _connTestCts?.Dispose();
            _connTestCts = new CancellationTokenSource();
            var token = _connTestCts.Token;

            LblStatus.Text = _env == Env.Live ? "Checking Live SQL..." : "Checking UAT SQL...";

            try
            {
                var ok = await TestSqlConnectivityBoundedAsync(
                    BuildConnectionString(), timeoutSeconds: GetSqlPingTimeoutSeconds(), token);

                if (token.IsCancellationRequested) return;

                _envConnected = ok;
                if (ok)
                {
                    LblStatus.Text = "✅ Connected";
                    BtnFinChoiceAP.Enabled = true;
                    BtnFinChoiceAR.Enabled = true;
                }
                else
                {
                    LblStatus.Text = "❌ Connection failed";
                }
            }
            catch (OperationCanceledException)
            {
                // ignored – superseded by a newer env selection
            }
            catch (Exception ex)
            {
                LblStatus.Text = $"❌ Error — {ex.Message}";
            }
            finally
            {
                SetEnvActionButtons();
                HideConnProgress();
                _connTestCts?.Dispose();
                _connTestCts = null;
            }
        }

        private void UpdateSearchEnabled()
        {
            // Only enable when there is non-whitespace text and the app is not busy
            bool hasText = !string.IsNullOrWhiteSpace(TxtItemSearch.Text);
            BtnItemSearch.Enabled = hasText && !_isRunning;
        }

        private void SetBusy(bool isBusy)
        {
            _isRunning = isBusy;

            // While busy: disable search + input (optional to keep input editable)
            if (isBusy)
            {
                ResetActionButtons();
                //SetActionButtonsEnabled(false);  // guard from clicks while busy
                
                SetActionButtonsEnabled(false);  // disable AP/AR buttons immediately when an action starts
                BtnItemSearch.Enabled = false;
                TxtItemSearch.Enabled = false;      // optional—remove if you want users to edit during run
                Cursor = Cursors.AppStarting;

                ProgBar.Visible = true;
                ProgBar.Style = ProgressBarStyle.Marquee;
                ProgBar.MarqueeAnimationSpeed = 30;
            }
            else
            {
                TxtItemSearch.Enabled = true;
                Cursor = Cursors.Default;

                ProgBar.MarqueeAnimationSpeed = 0;
                ProgBar.Style = ProgressBarStyle.Blocks;
                ProgBar.Visible = false;

                // Recompute enablement based on current text
                UpdateSearchEnabled();
                SetEnvActionButtons();
            }
        }

        private void UpdateAmountLabels(TableTotals t)
        {
            void apply()
            {

                // LblAmtBefore remains the AP TVAmt / AR SVAmt sum (as today)
                LblAmtBefore.Text = string.Format(_nz, "{0:C2}", t.TVAmt);

                // NEW: LblAmtAfter uses Σ COALESCE(ConAmt, BaseAmt) computed row-by-row
                LblAmtAfter.Text = string.Format(_nz, "{0:C2}", t.AfterCombinedAmt);

                LblCalcAmt.Text = string.Format(_nz, "{0:C2}", t.CalcAmt);

            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private void UpdateEntityLabel(string? carrier)
        {
            void apply()
            {
                LblEntity.Text = string.IsNullOrWhiteSpace(carrier)
                    ? "--"
                    : $"{carrier}";
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private void UpdateConPassLabel(string? conpass)
        {
            void apply()
            {
                LblConPass.Text = string.IsNullOrWhiteSpace(conpass)
                    ? "--"
                    : $"{conpass}";
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private void SetEnvActionButtons()
        {
            void apply()
            {
                bool on = _envConnected && !_isRunning;
                BtnFinChoiceAP.Enabled = on;
                BtnFinChoiceAR.Enabled = on;
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private async Task InitializeWebViewAsync()
        {
            if (_webViewReady) return;

            await WebView.EnsureCoreWebView2Async();

            ConfigureWebView(WebView.CoreWebView2);

            // Map .\ui\ to https://app.local/
            var uiFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui");
            Directory.CreateDirectory(uiFolder);

            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app.local",
                uiFolder,
                CoreWebView2HostResourceAccessKind.DenyCors);

            // Navigate to your local page (root-relative paths in index.html work)
            WebView.CoreWebView2.Navigate("https://app.local/index.html");

            _webViewReady = true;
        }

        private void ConfigureWebView(CoreWebView2 core)
        {
            core.Settings.AreDefaultContextMenusEnabled = false;
            #if !DEBUG
            core.Settings.AreDevToolsEnabled = false;
            #endif

            core.PermissionRequested += (s, e) => e.State = CoreWebView2PermissionState.Deny;
            core.NewWindowRequested += (s, e) => e.Handled = true;

            core.NavigationStarting += (s, e) =>
            {
                try
                {
                    var uri = new Uri(e.Uri);

                    // 1) Your app content (virtual host)
                    if (uri.Scheme == Uri.UriSchemeHttps &&
                        uri.Host.Equals("app.local", StringComparison.OrdinalIgnoreCase))
                        return;

                    // 2) Internal/blanking navigations used by your reset logic and WebView internals
                    if (uri.Scheme.Equals("about", StringComparison.OrdinalIgnoreCase)) return;   // e.g., about:blank
                    if (uri.Scheme.Equals("data", StringComparison.OrdinalIgnoreCase)) return;   // data:text/html,...
                    if (uri.Scheme.Equals("edge", StringComparison.OrdinalIgnoreCase)) return;   // edge://version etc
                    if (uri.Scheme.Equals("devtools", StringComparison.OrdinalIgnoreCase)) return;

                    // Block everything else
                    e.Cancel = true;
                }
                catch
                {
                    // If parsing fails, be permissive (NavigateToString may transiently surface blank)
                    e.Cancel = false;
                }
            };


            core.WebMessageReceived += (s, e) =>
            {
                try
                {
                    var json = e.WebMessageAsJson; // safe even if it's not pure string
                                                   // Minimal check: look for our type token
                    if (json != null && json.Contains("\"type\":\"render-complete\"", StringComparison.OrdinalIgnoreCase))
                    {
                        _renderCompleteTcs?.TrySetResult(true);
                    }
                }
                catch
                {
                    _renderCompleteTcs?.TrySetResult(true); // be permissive
                }
            };

        }

        private async Task<bool> AwaitNextRenderCompleteAsync(int timeoutMs = RenderCompleteTimeoutMs)
        {
            _renderCompleteTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var cts = new CancellationTokenSource(timeoutMs);
            using (cts.Token.Register(() => _renderCompleteTcs.TrySetResult(false)))
            {
                var ok = await _renderCompleteTcs.Task.ConfigureAwait(false);
                _renderCompleteTcs = null;
                return ok;
            }
        }

        private async Task EnsureCoreAsync()
        {
            if (WebView.CoreWebView2 == null)
                await WebView.EnsureCoreWebView2Async();
        }

        private Task NavigateAndWaitAsync(Action navigateAction, int timeoutMs = 2000)
        {
            var core = WebView.CoreWebView2!;
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<CoreWebView2NavigationCompletedEventArgs>? handler = null;
            var cts = new CancellationTokenSource(timeoutMs);

            handler = (s, e) =>
            {
                try { core.NavigationCompleted -= handler; } catch { }
                tcs.TrySetResult(true);
            };

            core.NavigationCompleted += handler;

            cts.Token.Register(() =>
            {
                try { core.NavigationCompleted -= handler; } catch { }
                // We still proceed on timeout so UI doesn’t hang
                tcs.TrySetResult(false);
            });

            // Issue the navigation AFTER subscribing
            navigateAction();

            return tcs.Task;
        }

        private async Task ResetThenNavigateAsync(string? targetUrl = null, string? targetHtml = null)
        {
            if (InvokeRequired)
            {
                await (Task)Invoke(new Func<Task>(() => ResetThenNavigateAsync(targetUrl, targetHtml)));
                return;
            }

            await EnsureCoreAsync();
            var core = WebView.CoreWebView2!;

            // 1) Stop any in-flight navigation BEFORE blanking
            try { core.Stop(); } catch { /* ignore */ }

            // 2) Navigate to a lightweight blank page and await it.
            // Use data: URI to avoid analyzer noise.
            await NavigateAndWaitAsync(
                () => core.Navigate("data:text/html,<html><head><meta charset='utf-8'></head><body></body></html>"),
                timeoutMs: 1500
            );

            // 3) Now go to the actual target (if any). We don't call Stop() again.
            if (!string.IsNullOrWhiteSpace(targetHtml))
            {
                await NavigateAndWaitAsync(() => core.NavigateToString(targetHtml), timeoutMs: 3000);
            }
            else if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                await NavigateAndWaitAsync(() => core.Navigate(targetUrl), timeoutMs: 5000);
            }
            // else → intentionally stays blank
        }

        private string BuildConnectionString()
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = sqlServer,
                InitialCatalog = string.IsNullOrWhiteSpace(sqlDatabase) ? "master" : sqlDatabase,
                IntegratedSecurity = true,
                TrustServerCertificate = trustServerCertificate,
                ConnectTimeout = GetSqlTimeoutSeconds()
            };

            return csb.ConnectionString;
        }

        private static async Task<(bool ok, string message)> TryConnectAsync(string connectionString, CancellationToken ct)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync(ct);

                // Optional: ping
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = GetSqlPingTimeoutSeconds();

                await cmd.ExecuteScalarAsync(ct);

                return (true, "Connected");
            }
            catch (SqlException ex)
            {
                return (false, $"SQL error ({ex.Number}): {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                return (false, "Operation cancelled.");
            }
            catch (Exception ex)
            {
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        private async Task<bool> WithConnectivityCheckAsync()
        {
            string connStr = BuildConnectionString();

            // Show progress
            if (LblStatus != null) LblStatus.Text = "Checking SQL connectivity…";


            using var cts = new CancellationTokenSource();
            try
            {
                var (ok, message) = await TryConnectAsync(connStr, cts.Token);
                if (!ok)
                {
                    if (LblStatus != null) LblStatus.Text = "❌ Connection failed";
                    MessageBox.Show(message, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (LblStatus != null) LblStatus.Text = "✅ Connected";
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }

        }


        private static readonly Dictionary<string, string> FriendlyHeaderMap =
            new(StringComparer.OrdinalIgnoreCase)
        {
            { "Client", "Client" },
            { "Consignment", "Consignment" },
            { "SRRef", "SRRef" },
            { "Date", "Date" },
            { "Status", "Status" },
            { "Trip", "Trip" },
            { "Carrier", "Carrier" },
            { "VehType", "VehType" },
            { "Vol", "Vol" },
            { "Wgt", "Wgt" },

            // --- Finance columns (AP + AR) ---
            // AP: TVAmt     → header "TVAmt"
            // AR: SVAmt     → show as "TVAmt" in UI for consistency
            { "TVAmt",   "TVAmt" },
            { "SVAmt",   "TVAmt" },

            { "Inv", "Inv" },
            { "IsCon", "IsCon" },
            { "ConCharge", "ConChCode" },
            { "ConAmt", "ConAmt" },
            { "ConMod", "ConMod" },
            { "CalcCharge", "CalcChCode" },

            // AP: CalcTotAmt → header "CalcTotAmt"
            // AR: CalcAmt     → show as "CalcTotAmt" in UI for consistency
            { "CalcTotAmt", "CalcTotAmt" },
            { "CalcAmt",    "CalcTotAmt" },

            { "CalcFaf", "CalcFafAmt" },  // match your existing "CalcFafAmt" header
            { "CalcFafAmt", "CalcFafAmt" },

            // Existing AP-only (kept for compatibility)
            { "Domain", "Domain" },
            { "ConChCode", "ConChCode" },
            { "CalcChCode", "CalcChCode" },
            { "CalcMod", "CalcMod" },
            { "ConPass", "ConPass" },     // AR does not supply this; stays empty in AR
        };

        private sealed class TablePayload
        {
            public required List<string> Columns { get; init; }

            public required List<string> Headers { get; init; }

            public required List<List<object?>> Rows { get; init; }
        }

        private static string GetApcStoredProcName()
        {
            // Fall back to your current default if the key is missing
            return ConfigurationManager.AppSettings["SqlStoredProcedureApc"]
                   ?? "fin.nsp_FinConsolidation_AP_Test";
        }

        private static string GetArcStoredProcName()
        {
            // Fallback helps during early dev if the key isn't present
            return ConfigurationManager.AppSettings["SqlStoredProcedureArc"]
                   ?? "fin.nsp_FinConsolidation_AR_Test";
        }


        // Single source of truth for DB timeouts (seconds)
        private static int GetSqlTimeoutSeconds()
        {
            // Use your config value; default to 60 if missing/invalid
            if (int.TryParse(ConfigurationManager.AppSettings["SqlConnectTimeoutSeconds"], out var s) && s > 0)
                return s;
            return 60;
        }

        // Optional: if you want a different floor when used for "quick pings"
        private static int GetSqlPingTimeoutSeconds()
        {
            // Keep a minimum of 5s for quick checks
            return Math.Max(5, GetSqlTimeoutSeconds());
        }

        private async Task<(TablePayload table, TableTotals totals)> ExecApTestStrictWithTotalsAsync(
            string? tripCode,
            int action,
            CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetApcStoredProcName();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@tripCode", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(tripCode) ? DBNull.Value : tripCode;
            cmd.Parameters.Add("@action", SqlDbType.Int).Value = action;
            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            using var rdr = await cmd.ExecuteReaderAsync(ct);

            var columns = new List<string>(rdr.FieldCount);
            var headers = new List<string>(rdr.FieldCount);
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                var col = rdr.GetName(i);
                columns.Add(col);
                headers.Add(FriendlyHeaderMap.TryGetValue(col, out var label) ? label : col);
            }

            int idxTVAmt = columns.FindIndex(c => c.Equals("TVAmt", StringComparison.OrdinalIgnoreCase));
            int idxConAmt = columns.FindIndex(c => c.Equals("ConAmt", StringComparison.OrdinalIgnoreCase));
            int idxCalcAmt = columns.FindIndex(c => c.Equals("CalcTotAmt", StringComparison.OrdinalIgnoreCase));
            int idxCarrier = columns.FindIndex(c => c.Equals("Carrier", StringComparison.OrdinalIgnoreCase));
            int idxConPass = columns.FindIndex(c => c.Equals("ConPass", StringComparison.OrdinalIgnoreCase));
            int idxCalcPass = columns.FindIndex(c => c.Equals("CalcPass", StringComparison.OrdinalIgnoreCase));
            int idxOrigin = columns.FindIndex(c => c.Equals("Origin", StringComparison.OrdinalIgnoreCase));
            int idxDest = columns.FindIndex(c => c.Equals("Destination", StringComparison.OrdinalIgnoreCase));

            var totals = new TableTotals();
            var rows = new List<List<object?>>(256);

            HashSet<(string? o, string? d)>? groupSet =
                (idxOrigin >= 0 && idxDest >= 0) ? new() : null;

            while (await rdr.ReadAsync(ct))
            {
                totals.RowCount++;

                // Capture Carrier once
                if (idxCarrier >= 0 && totals.Carrier == null && !rdr.IsDBNull(idxCarrier))
                {
                    var c = rdr.GetValue(idxCarrier)?.ToString();
                    if (!string.IsNullOrWhiteSpace(c)) totals.Carrier = c;
                }

                // Capture ConPass once
                if (idxConPass >= 0 && totals.ConPass == null && !rdr.IsDBNull(idxConPass))
                {
                    var cp = rdr.GetValue(idxConPass)?.ToString();
                    if (!string.IsNullOrWhiteSpace(cp)) totals.ConPass = cp;
                }

                // Capture CalcPass once
                if (idxCalcPass >= 0 && totals.CalcPass == null && !rdr.IsDBNull(idxCalcPass))
                {
                    var cp = rdr.GetValue(idxCalcPass)?.ToString();
                    if (!string.IsNullOrWhiteSpace(cp)) totals.CalcPass = cp;
                }

                // Track Origin/Dest groups (optional)
                if (groupSet != null)
                {
                    string? o = idxOrigin >= 0 && !rdr.IsDBNull(idxOrigin) ? rdr.GetValue(idxOrigin)?.ToString() : null;
                    string? d = idxDest >= 0 && !rdr.IsDBNull(idxDest) ? rdr.GetValue(idxDest)?.ToString() : null;
                    groupSet.Add((o, d));
                }

                var arr = new List<object?>(rdr.FieldCount);
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    if (rdr.IsDBNull(i)) { arr.Add(null); continue; }
                    var v = rdr.GetValue(i);

                    // Accumulate precise totals for finance
                    if (i == idxTVAmt)
                    {
                        if (v is decimal dv) totals.TVAmt += dv;
                        else if (v is double dd) totals.TVAmt += (decimal)dd;
                        else if (v is float ff) totals.TVAmt += (decimal)ff;
                    }
                    else if (i == idxConAmt)
                    {
                        totals.HasConAmt = true;

                        //if (v is decimal dv) totals.ConAmt += dv;
                        //else if (v is double dd) totals.ConAmt += (decimal)dd;
                        //else if (v is float ff) totals.ConAmt += (decimal)ff;


                        totals.HasConAmt = true;
                        decimal val = v switch
                        {
                            decimal dv => dv,
                            double dd => (decimal)dd,
                            float ff => (decimal)ff,
                            _ => 0m
                        };
                        //if (val != 0m) totals.HasNonZeroConAmt = true;
                        totals.ConAmt += val;

                    }
                    else if (i == idxCalcAmt)
                    {
                        if (v is decimal dv) totals.CalcAmt += dv;
                        else if (v is double dd) totals.CalcAmt += (decimal)dd;
                        else if (v is float ff) totals.CalcAmt += (decimal)ff;
                    }

                    // Normalize numeric/date types for JS payload
                    if (v is DateTime dt) arr.Add(dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    else if (v is decimal dec) arr.Add((double)dec);
                    else if (v is float f) arr.Add((double)f);
                    else if (v is long l) arr.Add((double)l);
                    else if (v is int ii) arr.Add((double)ii);
                    else if (v is short sh) arr.Add((double)sh);
                    else if (v is byte b) arr.Add((double)b);
                    else arr.Add(v);
                }

                rows.Add(arr);

                decimal baseAmt = 0m;
                if (idxTVAmt >= 0 && !rdr.IsDBNull(idxTVAmt))
                {
                    var v = rdr.GetValue(idxTVAmt);
                    if (v is decimal dv) baseAmt = dv;
                    else if (v is double dd) baseAmt = (decimal)dd;
                    else if (v is float ff) baseAmt = (decimal)ff;
                }

                decimal? conAmtRow = null;
                if (idxConAmt >= 0 && !rdr.IsDBNull(idxConAmt))
                {
                    var v = rdr.GetValue(idxConAmt);
                    if (v is decimal dv) conAmtRow = dv;
                    else if (v is double dd) conAmtRow = (decimal)dd;
                    else if (v is float ff) conAmtRow = (decimal)ff;
                }

                totals.AfterCombinedAmt += conAmtRow ?? baseAmt;

            }

            if (groupSet != null) totals.GroupCount = groupSet.Count;

            var table = new TablePayload { Columns = columns, Headers = headers, Rows = rows };
            return (table, totals);
        }

        private async Task<string> ExecArcRefreshRatesAsync(string? consignment, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetArcStoredProcName();  // fin.nsp_FinConsolidation_AR_Test
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@consignment", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(consignment) ? DBNull.Value : consignment;

            cmd.Parameters.Add("@action", SqlDbType.Int).Value = 2; // REFRESH for AR
            var retParam = cmd.Parameters.Add("@return_value", SqlDbType.Int);
            retParam.Direction = ParameterDirection.ReturnValue;

            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            string? status = null;
            using (var rdr = await cmd.ExecuteReaderAsync(ct))
            {
                if (await rdr.ReadAsync(ct))
                {
                    if (!rdr.IsDBNull(0))
                        status = rdr.GetValue(0)?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                var code = (retParam.Value is int i) ? i : 0;
                status = code == 0 ? "AR: Rates refreshed successfully." : $"AR: Completed (code {code}).";
            }
            return status!;
        }


        private static int FindColIndex(List<string> columns, params string[] names)
        {
            foreach (var n in names)
            {
                var idx = columns.FindIndex(c => c.Equals(n, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) return idx;
            }
            return -1;
        }

        private async Task<(TablePayload table, TableTotals totals)> ExecArTestStrictWithTotalsAsync(
            string? consignment,
            int action,
            CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetArcStoredProcName();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@consignment", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(consignment) ? DBNull.Value : consignment;
            cmd.Parameters.Add("@action", SqlDbType.Int).Value = action; // 0 for preview
            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            using var rdr = await cmd.ExecuteReaderAsync(ct);

            var columns = new List<string>(rdr.FieldCount);
            var headers = new List<string>(rdr.FieldCount);
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                var col = rdr.GetName(i);
                columns.Add(col);
                headers.Add(FriendlyHeaderMap.TryGetValue(col, out var label) ? label : col);
            }

            // === AR column indices ===
            int idxTVAmt = FindColIndex(columns, "SVAmt", "TVAmt"); // AR uses SVAmt
            int idxConAmt = FindColIndex(columns, "ConAmt");
            int idxCalcPass = FindColIndex(columns, "CalcPass");
            int idxCalcAmt = FindColIndex(columns, "CalcAmt", "CalcTotAmt"); // AR uses CalcAmt
            int idxClient = FindColIndex(columns, "Client");
            int idxOrigin = FindColIndex(columns, "Origin"); // may not exist in AR
            int idxDest = FindColIndex(columns, "Destination");// may not exist in AR
            int idxConPass = FindColIndex(columns, "ConPass");     // NEW: uniform Y/N across dataset

            var totals = new TableTotals();
            var rows = new List<List<object?>>(256);
            HashSet<(string? o, string? d)>? groupSet =
                (idxOrigin >= 0 && idxDest >= 0) ? new() : null;

            while (await rdr.ReadAsync(ct))
            {
                totals.RowCount++;

                // === Entity label for AR: use Client (single value for dataset) ===
                if (idxClient >= 0 && totals.Carrier == null && !rdr.IsDBNull(idxClient))
                {
                    var client = rdr.GetValue(idxClient)?.ToString();
                    if (!string.IsNullOrWhiteSpace(client)) totals.Carrier = client; // reuse property
                }

                // NEW: ConPass is uniform for AR; capture once from the first row
                if (idxConPass >= 0 && totals.ConPass == null && !rdr.IsDBNull(idxConPass))
                {
                    var cp = rdr.GetValue(idxConPass)?.ToString();
                    if (!string.IsNullOrWhiteSpace(cp)) totals.ConPass = cp;
                }

                if (idxCalcPass >= 0 && totals.CalcPass == null && !rdr.IsDBNull(idxCalcPass))
                {
                    var calc = rdr.GetValue(idxCalcPass)?.ToString();
                    if (!string.IsNullOrWhiteSpace(calc)) totals.CalcPass = calc;
                }

                if (groupSet != null)
                {
                    string? o = idxOrigin >= 0 && !rdr.IsDBNull(idxOrigin) ? rdr.GetValue(idxOrigin)?.ToString() : null;
                    string? d = idxDest >= 0 && !rdr.IsDBNull(idxDest) ? rdr.GetValue(idxDest)?.ToString() : null;
                    groupSet.Add((o, d));
                }

                var arr = new List<object?>(rdr.FieldCount);
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    if (rdr.IsDBNull(i)) { arr.Add(null); continue; }
                    var v = rdr.GetValue(i);

                    // Totals for finance
                    if (i == idxTVAmt)
                    {
                        if (v is decimal dv) totals.TVAmt += dv;
                        else if (v is double dd) totals.TVAmt += (decimal)dd;
                        else if (v is float ff) totals.TVAmt += (decimal)ff;
                    }
                    else if (i == idxConAmt)
                    {
                        totals.HasConAmt = true;

                        if (v is decimal dv) totals.ConAmt += dv;
                        else if (v is double dd) totals.ConAmt += (decimal)dd;
                        else if (v is float ff) totals.ConAmt += (decimal)ff;
                    }
                    else if (i == idxCalcAmt)
                    {
                        if (v is decimal dv) totals.CalcAmt += dv;
                        else if (v is double dd) totals.CalcAmt += (decimal)dd;
                        else if (v is float ff) totals.CalcAmt += (decimal)ff;
                    }

                    // Normalize to JS-friendly values
                    if (v is DateTime dt) arr.Add(dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                    else if (v is decimal dec) arr.Add((double)dec);
                    else if (v is float f) arr.Add((double)f);
                    else if (v is long l) arr.Add((double)l);
                    else if (v is int ii) arr.Add((double)ii);
                    else if (v is short sh) arr.Add((double)sh);
                    else if (v is byte b) arr.Add((double)b);
                    else arr.Add(v);
                }
                rows.Add(arr);

                decimal baseAmt = 0m;
                if (idxTVAmt >= 0 && !rdr.IsDBNull(idxTVAmt))
                {
                    var v = rdr.GetValue(idxTVAmt);
                    if (v is decimal dv) baseAmt = dv;
                    else if (v is double dd) baseAmt = (decimal)dd;
                    else if (v is float ff) baseAmt = (decimal)ff;
                }

                decimal? conAmtRow = null;
                if (idxConAmt >= 0 && !rdr.IsDBNull(idxConAmt))
                {
                    var v = rdr.GetValue(idxConAmt);
                    if (v is decimal dv) conAmtRow = dv;
                    else if (v is double dd) conAmtRow = (decimal)dd;
                    else if (v is float ff) conAmtRow = (decimal)ff;
                }

                totals.AfterCombinedAmt += conAmtRow ?? baseAmt;

            }

            if (groupSet != null) totals.GroupCount = groupSet.Count;
            var table = new TablePayload { Columns = columns, Headers = headers, Rows = rows };
            return (table, totals);
        }

        private async Task RenderStrictAsync(TablePayload payload)
        {
            var json = JsonSerializer.Serialize(new
            { 
                columns = payload.Columns, 
                headers = payload.Headers, 
                rows = payload.Rows 
            });

            // Wait up to ~1s for app.js to load if needed
            for (int i = 0; i < 10; i++)
            {
                var ok = await WebView.CoreWebView2.ExecuteScriptAsync("typeof window.renderResultsStrict");
                if (ok?.Trim('"') == "function") break;
                await Task.Delay(100);
            }

            await WebView.CoreWebView2.ExecuteScriptAsync($"window.renderResultsStrict({json});");
        }

        // Enables/disables based on totals flags
        private void UpdateActionButtons(TableTotals t)
        {
            bool enableApply = string.Equals(t.ConPass, "Y", StringComparison.OrdinalIgnoreCase);
            bool enableRefresh = string.Equals(t.CalcPass, "Y", StringComparison.OrdinalIgnoreCase);

            void apply()
            {
                BtnApplyCon.Enabled = enableApply;
                BtnRefreshRates.Enabled = enableRefresh;
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        // Ensures both action buttons are off by default
        private void ResetActionButtons()
        {
            void apply()
            {
                BtnApplyCon.Enabled = false;
                BtnRefreshRates.Enabled = false;
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private async Task ClearPreviewUiAsync(bool showAwaiting = true)
        {
            // Clear labels + hide totals panel
            void clearLabels()
            {
                LblEntity.Text = UiDash;
                LblAmtBefore.Text = UiDash;
                LblAmtAfter.Text = UiDash;
                PnlTotals.Visible = false;
                LblStatus.Text = showAwaiting ? "Awaiting data…" : string.Empty;
                ResetActionButtons();
            }
            if (InvokeRequired) BeginInvoke((Action)clearLabels); else clearLabels();

            // Clear the app page DOM in-place (keeps app.js loaded)
            await EnsureCoreAsync();
            try
            {
                if (showAwaiting)
                {
                    // Existing behavior: inject spinner + "Awaiting data…"
                    await WebView.CoreWebView2.ExecuteScriptAsync(@"
                (function(){
                  var thead = document.getElementById('thead');
                  var tbody = document.getElementById('tbody');
                  var status = document.getElementById('status');
                  if (thead) thead.innerHTML = '';
                  if (tbody) {
                    tbody.innerHTML =
                      '<tr>' +
                        '<td colspan=""100"" style=""padding:1rem;"">' +
                          '<div id=""fc-loading"" style=""display:flex;align-items:center;gap:.6rem;color:var(--muted,#6e7781);"">' +
                            '<span class=""fc-spinner"" aria-hidden=""true"" ' +
                            'style=""width:16px;height:16px;display:inline-block;border-radius:50%;' +
                            'border:2px solid color-mix(in srgb, var(--fg,#1b1f23) 15%, transparent);' +
                            'border-top-color: color-mix(in srgb, var(--fg,#1b1f23) 75%, transparent);' +
                            'animation: fc-spin .8s linear infinite;""></span>' +
                            '<span>Awaiting data…</span>' +
                          '</div>' +
                        '</td>' +
                      '</tr>';
                    if (!document.getElementById('fc-loading-style')) {
                      var css = '@keyframes fc-spin { from { transform: rotate(0deg);} to { transform: rotate(360deg);} }';
                      var style = document.createElement('style');
                      style.id = 'fc-loading-style';
                      style.textContent = css;
                      document.head.appendChild(style);
                    }
                  }
                  if (status) status.textContent = 'Awaiting data…';
                })();
            ");
                }
                else
                {
                    // New behavior: make the preview truly blank (no spinner, no caption)
                    await WebView.CoreWebView2.ExecuteScriptAsync(@"
                (function(){
                  var thead = document.getElementById('thead');
                  var tbody = document.getElementById('tbody');
                  var status = document.getElementById('status');
                  if (thead) thead.innerHTML = '';
                  if (tbody) tbody.innerHTML = '';
                  if (status) status.textContent = '';
                })();
            ");
                }
            }
            catch
            {
                // Ignore if not on app page yet
            }
        }

        private async Task<string> ExecApcApplyAsync(string? tripCode, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetApcStoredProcName();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@tripCode", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(tripCode) ? DBNull.Value : tripCode;

            cmd.Parameters.Add("@action", SqlDbType.Int).Value = 1; // APPLY

            // Optional return code (works even if SP doesn't set it)
            var retParam = cmd.Parameters.Add("@return_value", SqlDbType.Int);
            retParam.Direction = ParameterDirection.ReturnValue;

            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            string? status = null;

            using (var rdr = await cmd.ExecuteReaderAsync(ct))
            {
                if (await rdr.ReadAsync(ct))
                {
                    if (!rdr.IsDBNull(0))
                        status = rdr.GetValue(0)?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                var code = (retParam.Value is int i) ? i : 0;
                status = code == 0 ? "APC applied successfully." : $"Completed (code {code}).";
            }

            return status!;
        }

        private async Task<string> ExecApcRefreshRatesAsync(string? tripCode, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetApcStoredProcName();
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@tripCode", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(tripCode) ? DBNull.Value : tripCode;

            cmd.Parameters.Add("@action", SqlDbType.Int).Value = 2; // REFRESH RATES
            var retParam = cmd.Parameters.Add("@return_value", SqlDbType.Int);
            retParam.Direction = ParameterDirection.ReturnValue;

            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            string? status = null;
            using (var rdr = await cmd.ExecuteReaderAsync(ct))
            {
                if (await rdr.ReadAsync(ct))
                {
                    if (!rdr.IsDBNull(0))
                        status = rdr.GetValue(0)?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                var code = (retParam.Value is int i) ? i : 0;
                status = code == 0 ? "Rates refreshed successfully." : $"Completed (code {code}).";
            }

            return status!;
        }


        private async Task<string> ExecArcApplyAsync(string? consignment, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildConnectionString());
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = GetArcStoredProcName();      // fin.nsp_FinConsolidation_AR_Test
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@consignment", SqlDbType.VarChar, -1).Value =
                string.IsNullOrWhiteSpace(consignment) ? DBNull.Value : consignment;

            cmd.Parameters.Add("@action", SqlDbType.Int).Value = 1; // APPLY for AR

            // Optional return code
            var retParam = cmd.Parameters.Add("@return_value", SqlDbType.Int);
            retParam.Direction = ParameterDirection.ReturnValue;

            cmd.CommandTimeout = GetSqlTimeoutSeconds();

            string? status = null;
            using (var rdr = await cmd.ExecuteReaderAsync(ct))
            {
                if (await rdr.ReadAsync(ct))
                {
                    if (!rdr.IsDBNull(0))
                        status = rdr.GetValue(0)?.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                var code = (retParam.Value is int i) ? i : 0;
                status = code == 0 ? "AR: Applied successfully." : $"AR: Completed (code {code}).";
            }
            return status!;
        }


        private void SetActionButtonsEnabled(bool enabled)
        {
            void apply()
            {
                // Safety: only enable what should be enabled;

                // Navigation/actions
                BtnFinChoiceAP.Enabled = enabled && BtnFinChoiceAP.Enabled;
                BtnFinChoiceAR.Enabled = enabled && BtnFinChoiceAR.Enabled;

                // In-grid actions
                BtnApplyCon.Enabled = enabled && BtnApplyCon.Enabled;
                BtnRefreshRates.Enabled = enabled && BtnRefreshRates.Enabled;

            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }

        private void UpdateEntityCaption()
        {
            void apply()
            {
                // When AP is selected → show "Carrier"; when AR → "Client"
                var isAR = string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase);
                LblEntity_Lbl.Text = isAR ? "Client:" : "Carrier:"; // add a small label next to LblEntity if you have one
                var tt = new ToolTip();
                tt.SetToolTip(LblEntity, isAR ? "Client for this dataset" : "Carrier for this dataset");
            }
            if (InvokeRequired) BeginInvoke((Action)apply); else apply();
        }


        // Outcome detector for refresh/apply messages returned by the stored proc
        private static (MessageBoxIcon icon, string caption, string statusText) ClassifyOutcome(string? message)
        {
            var m = (message ?? string.Empty).Trim();
            var lower = m.ToLowerInvariant();

            if (lower.Contains("success"))
                return (MessageBoxIcon.Information, "Success", "✅ Success");

            if (lower.Contains("fail")) // matches fail/failed/failure
                return (MessageBoxIcon.Error, "Failed", "❌ Failed");

            return (MessageBoxIcon.Warning, "Completed with exceptions", "⚠️ Completed with exceptions");
        }


        #region EventHandlers

        private async void FrmMain_ShownAsync(object? sender, EventArgs e)
        {
            ApplyEnvironmentToConnectionFields();
            await TriggerEnvConnectionCheckAsync();
            SetEnvActionButtons();
            await InitializeWebViewAsync();
        }

        private async void BtnFinChoice_Click(object? sender, EventArgs e)
        {
            if (sender is not Button _btn)
            {
                MessageBox.Show("Unexpected error: Sender is not a Button.");
                return;
            }

            SetBusy(true);

            try
            {
                if (!_envConnected) return;

                if (_btn.Tag is not EnumActionBtn _action)
                    return;

                switch (_action)
                {
                    case EnumActionBtn.BtnFinChoiceAP:
                        {
                            appMode = "AP";
                            LblSelInd1.BackColor = SystemColors.ActiveCaption;
                            LblSelInd2.BackColor = SystemColors.Control;
                            LblAppTitle.Text = $"{appTitle} - {appMode}";
                            LblItemSearch_Lbl.Text = "Trip Code:";
                            UpdateEntityCaption();
                            PnlItemSearch.Visible = true;
                            PnlModRates.Visible = true;
                            ResetActionButtons();
                            ResetUiForModeChange();

                            await ResetThenNavigateAsync(targetUrl: "https://app.local/index.html");
                            break;
                        }

                    case EnumActionBtn.BtnFinChoiceAR:
                        {

                            appMode = "AR";
                            LblSelInd2.BackColor = SystemColors.ActiveCaption;
                            LblSelInd1.BackColor = SystemColors.Control;
                            LblAppTitle.Text = $"{appTitle} - {appMode}";
                            LblItemSearch_Lbl.Text = "Consignment:";
                            UpdateEntityCaption();
                            PnlItemSearch.Visible = true;
                            PnlModRates.Visible = true;
                            PnlTotals.Visible = false;
                            ResetActionButtons();
                            ResetUiForModeChange();

                            await ResetThenNavigateAsync(targetUrl: "https://app.local/index.html");
                            break;
                        }
                }
            }
            finally
            {
                SetBusy(false); 
            }
        }

        private async void BtnItemSearch_Click(object? sender, EventArgs e)
        {
            if (_isRunning) return;
            SetBusy(true);
            await EnsureCoreAsync();

            var href = await WebView.CoreWebView2.ExecuteScriptAsync("location.href");
            if (href == null || !href.Contains("app.local", StringComparison.OrdinalIgnoreCase))
            {
                await ResetThenNavigateAsync(targetUrl: "https://app.local/index.html");
            }

            try
            {
                await ClearPreviewUiAsync();
                if (!_webViewReady)
                    await InitializeWebViewAsync();

                bool connected = await WithConnectivityCheckAsync();
                if (!connected) return;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
                string? input = TxtItemSearch?.Text?.Trim();  // Trip Code (AP) or Consignment (AR)

                // === NEW: branch by appMode ===
                (TablePayload table, TableTotals totals) result;

                if (string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase))
                {
                    LblStatus.Text = "Running AR preview…";
                    result = await ExecArTestStrictWithTotalsAsync(input, action: 0, cts.Token);
                }
                else // default AP
                {
                    LblStatus.Text = "Running AP preview…";
                    result = await ExecApTestStrictWithTotalsAsync(input, action: 0, cts.Token);
                }

                var (table, totals) = result;

                var renderWait = AwaitNextRenderCompleteAsync();
                await RenderStrictAsync(table);             // populate WebView
                await renderWait;                           // wait for render-complete ping

                UpdateAmountLabels(totals);                 // LblAmtBefore, LblAmtAfter, LblCalcAmt
                UpdateEntityLabel(totals.Carrier);         // LblCarrier
                UpdateConPassLabel(totals.ConPass);         // LblConPass
                UpdateActionButtons(totals);                // enables Apply if ConPass == "Y"

                LblStatus.Text = $"✅ Done — {table.Rows.Count} rows in {totals.GroupCount} groups";
                PnlTotals.Visible = true;
            }
            catch (OperationCanceledException)
            {
                LblStatus.Text = "Operation cancelled.";
            }
            catch (SqlException ex)
            {
                LblStatus.Text = "❌ SQL error";
                MessageBox.Show($"SQL error ({ex.Number}): {ex.Message}", "SQL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LblStatus.Text = "❌ Unexpected error";
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_renderCompleteTcs != null)
                    await AwaitNextRenderCompleteAsync();

                SetBusy(false);
            }
        }

        private async void BtnApplyCon_Click(object? sender, EventArgs e)
        {
            if (_isRunning) return; // keep single-run semantics

            var input = TxtItemSearch?.Text?.Trim();
            var isAR = string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(input))
            {
                var prompt = isAR
                    ? PromptEnterCons
                    : PromptEnterTrip;
                MessageBox.Show(prompt, "Apply", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true);
            try
            {
                LblStatus.Text = isAR ? "Applying ARC…" : "Applying APC…";

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                string status;

                if (isAR)
                    status = await ExecArcApplyAsync(input, cts.Token);  // @consignment, action=1
                else
                    status = await ExecApcApplyAsync(input, cts.Token);  // @tripCode,   action=1

                // Outcome → icon, caption, status text
                var outcome = ClassifyOutcome(status);
                LblStatus.Text = outcome.statusText;
                MessageBox.Show(status, outcome.caption, MessageBoxButtons.OK, outcome.icon);

                // === Auto-refresh preview (action: 0) ===
                LblStatus.Text = "Refreshing preview…";
                if (!_webViewReady)
                    await InitializeWebViewAsync(); // typically already ready after first search

                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                TablePayload table;
                TableTotals totals;

                if (isAR)
                    (table, totals) = await ExecArTestStrictWithTotalsAsync(input, action: 0, refreshCts.Token);
                else
                    (table, totals) = await ExecApTestStrictWithTotalsAsync(input, action: 0, refreshCts.Token);

                var renderWait = AwaitNextRenderCompleteAsync();
                await RenderStrictAsync(table);
                await renderWait;

                UpdateAmountLabels(totals);
                UpdateEntityLabel(totals.Carrier);
                UpdateConPassLabel(totals.ConPass);
                UpdateActionButtons(totals);      // enables Apply if ConPass == "Y"
                PnlTotals.Visible = true;

                LblStatus.Text = $"✅ Done — {table.Rows.Count} rows in {totals.GroupCount} groups";
            }
            catch (OperationCanceledException)
            {
                LblStatus.Text = "Operation cancelled.";
            }
            catch (SqlException ex)
            {
                LblStatus.Text = "❌ SQL error";
                MessageBox.Show($"SQL error ({ex.Number}): {ex.Message}", "SQL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LblStatus.Text = "❌ Unexpected error";
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_renderCompleteTcs != null)
                    await AwaitNextRenderCompleteAsync();
                SetBusy(false);
            }
        }

        private async void BtnRefreshRates_Click(object? sender, EventArgs e)
        {
            if (_isRunning) return;

            var input = TxtItemSearch?.Text?.Trim(); // Trip Code (AP) or Consignment (AR)
            if (string.IsNullOrWhiteSpace(input))
            {
                // Make the prompt match the current mode
                var prompt = string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase)
                    ? PromptEnterConsRef
                    : PromptEnterTripRef;
                MessageBox.Show(prompt, "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true);
            try
            {
                LblStatus.Text = "Refreshing rates…";
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                // Branch the REFRESH call by mode
                string status;
                if (string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase))
                {
                    status = await ExecArcRefreshRatesAsync(input, cts.Token);   // @consignment, action=2
                }
                else
                {
                    status = await ExecApcRefreshRatesAsync(input, cts.Token);   // @tripCode,  action=2
                }

                // Classify outcome (Success / Failed / Exceptions) and show appropriate icon
                var outcome = ClassifyOutcome(status);
                LblStatus.Text = outcome.statusText;
                MessageBox.Show(status, outcome.caption, MessageBoxButtons.OK, outcome.icon);

                // === Auto-refresh preview (action: 0) ===
                LblStatus.Text = "Refreshing preview…";
                if (!_webViewReady)
                    await InitializeWebViewAsync();

                using var refreshCts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

                // Branch the PREVIEW call by mode
                TablePayload table;
                TableTotals totals;
                if (string.Equals(appMode, "AR", StringComparison.OrdinalIgnoreCase))
                {
                    (table, totals) = await ExecArTestStrictWithTotalsAsync(input, action: 0, refreshCts.Token);
                }
                else
                {
                    (table, totals) = await ExecApTestStrictWithTotalsAsync(input, action: 0, refreshCts.Token);
                }

                var renderWait = AwaitNextRenderCompleteAsync();
                await RenderStrictAsync(table);
                await renderWait;

                UpdateAmountLabels(totals);
                UpdateEntityLabel(totals.Carrier);
                UpdateConPassLabel(totals.ConPass);
                UpdateActionButtons(totals);
                PnlTotals.Visible = true;

                BtnRefreshRates.Enabled = true; // keep it available after a successful cycle
                LblStatus.Text = $"✅ Done — {table.Rows.Count} rows in {totals.GroupCount} groups";
            }
            catch (OperationCanceledException)
            {
                LblStatus.Text = "Operation cancelled.";
            }
            catch (SqlException ex)
            {
                LblStatus.Text = "❌ SQL error";
                MessageBox.Show($"SQL error ({ex.Number}): {ex.Message}",
                    "SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LblStatus.Text = "❌ Unexpected error";
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (_renderCompleteTcs != null)
                    await AwaitNextRenderCompleteAsync();
                SetBusy(false);
            }
        }


        private async void EnvRadio_CheckedChanged(object? sender, EventArgs e)
        {

            if (_suppressEnvHandler) return;

            if (sender is RadioButton rb && rb.Checked)
            {
                _env = ReferenceEquals(rb, RbEnvLive) ? Env.Live : Env.Uat;
                ApplyEnvironmentToConnectionFields();       // updates LblServer etc.

                // Clear UI WITHOUT spinner
                //await ClearPreviewUiAsync(showAwaiting: false);


                await EnsureCoreAsync();
                await ResetThenNavigateAsync(targetHtml: "<!doctype html><html><head><meta charset='utf-8'></head><body></body></html>");


                TxtItemSearch.Text = string.Empty;
                PnlItemSearch.Visible = false;
                PnlModRates.Visible = false;
                PnlTotals.Visible = false;
                LblSelInd1.BackColor = SystemColors.Control;
                LblSelInd2.BackColor = SystemColors.Control;
                LblAppTitle.Text = appTitle;

                _envConnected = false;
                SetEnvActionButtons();

                // Kick new env connectivity
                _ = TriggerEnvConnectionCheckAsync();
            }

        }


        #endregion EventHandlers

    }
}
