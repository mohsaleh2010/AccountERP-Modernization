# Read or change one encrypted setting row used to reveal hidden screen features for build/test.
# Encryption = char shift +28454 mod 65536 (same as SysConfigMenus.RazorPath). Connection comes from the dev appsettings (never printed).
#
#   General setting (SysConfig, KeysData = Common static field name):
#     powershell -NoProfile -File hidden-setting.ps1 -Key Show_Commision_Value_On_Bill_Monitor            # read
#     powershell -NoProfile -File hidden-setting.ps1 -Key Show_Commision_Value_On_Bill_Monitor -Set 1      # change (then reload, see below)
#   Per-user menu permission (SysConfigMenusPERMATIONS):
#     powershell -NoProfile -File hidden-setting.ps1 -MenuId 958 -UserId 1                                 # read
#     powershell -NoProfile -File hidden-setting.ps1 -MenuId 958 -UserId 1 -Set 1                          # change
# After a change to a general setting (SysConfig) reload it into the running dev server without restarting:
#     curl -X POST http://localhost:5256/Dev/ReloadSettings      (Development + localhost only; then refresh the browser)
# To restore, run again with the old value that was printed by the change.
param([string]$Key, [int]$MenuId, [long]$UserId, [string]$Set)
$cfg = Get-Content 'E:\RazorRun\appsettings.json' -Raw | ConvertFrom-Json
$cs = $null; $cfg.ConnectionStrings.PSObject.Properties | Select-Object -First 1 | ForEach-Object { $cs = $_.Value }
Add-Type -AssemblyName System.Data
function Enc([string]$s){ $sb = New-Object System.Text.StringBuilder; foreach($ch in $s.ToCharArray()){ [void]$sb.Append([char](([int]$ch + 28454) % 65536)) }; $sb.ToString() }
function Dec([string]$s){ if($null -eq $s){ return '' }; $sb = New-Object System.Text.StringBuilder; foreach($ch in $s.ToCharArray()){ [void]$sb.Append([char](([int]$ch - 28454 + 65536) % 65536)) }; $sb.ToString() }
$c = New-Object System.Data.SqlClient.SqlConnection $cs; $c.Open()
$tx = $c.BeginTransaction()
try {
  if ($Key) {
    $q = $c.CreateCommand(); $q.Transaction = $tx; $q.CommandText = "SELECT Id, [Value] FROM SysConfig WHERE KeysData=@k"; [void]$q.Parameters.AddWithValue('@k', (Enc $Key))
    $t = New-Object System.Data.DataTable; $t.Load($q.ExecuteReader())
    if ($t.Rows.Count -ne 1) { throw "expected exactly 1 SysConfig row for '$Key', found $($t.Rows.Count)" }
    $old = Dec ([string]$t.Rows[0].Value); "SysConfig $Key = $old"
    if ($PSBoundParameters.ContainsKey('Set')) {
      $u = $c.CreateCommand(); $u.Transaction = $tx; $u.CommandText = "UPDATE SysConfig SET [Value]=@v WHERE Id=@i"; [void]$u.Parameters.AddWithValue('@v', (Enc $Set)); [void]$u.Parameters.AddWithValue('@i', $t.Rows[0].Id); [void]$u.ExecuteNonQuery()
      $r = $c.CreateCommand(); $r.Transaction = $tx; $r.CommandText = "SELECT [Value] FROM SysConfig WHERE Id=@i"; [void]$r.Parameters.AddWithValue('@i', $t.Rows[0].Id)
      "changed: $old -> " + (Dec ([string]$r.ExecuteScalar())) + "  (now run: curl -X POST http://localhost:5256/Dev/ReloadSettings ; restore with -Set $old)"
    }
  } elseif ($MenuId -gt 0 -and $UserId -gt 0) {
    $q = $c.CreateCommand(); $q.Transaction = $tx; $q.CommandText = "SELECT Id, PERMATION FROM SysConfigMenusPERMATIONS WHERE Menu_ID=@m AND USER_ID=@u"; [void]$q.Parameters.AddWithValue('@m', $MenuId); [void]$q.Parameters.AddWithValue('@u', $UserId)
    $t = New-Object System.Data.DataTable; $t.Load($q.ExecuteReader())
    if ($t.Rows.Count -ne 1) { throw "expected exactly 1 permission row for menu $MenuId user $UserId, found $($t.Rows.Count)" }
    $old = Dec ([string]$t.Rows[0].PERMATION); "Permission menu=$MenuId user=$UserId = $old"
    if ($PSBoundParameters.ContainsKey('Set')) {
      $u = $c.CreateCommand(); $u.Transaction = $tx; $u.CommandText = "UPDATE SysConfigMenusPERMATIONS SET PERMATION=@v WHERE Id=@i"; [void]$u.Parameters.AddWithValue('@v', (Enc $Set)); [void]$u.Parameters.AddWithValue('@i', $t.Rows[0].Id); [void]$u.ExecuteNonQuery()
      $r = $c.CreateCommand(); $r.Transaction = $tx; $r.CommandText = "SELECT PERMATION FROM SysConfigMenusPERMATIONS WHERE Id=@i"; [void]$r.Parameters.AddWithValue('@i', $t.Rows[0].Id)
      "changed: $old -> " + (Dec ([string]$r.ExecuteScalar())) + "  (the user may need to sign in again; restore with -Set $old)"
    }
  } else { throw "give -Key <SysConfig key> or -MenuId and -UserId" }
  $tx.Commit()
} catch { $tx.Rollback(); "ERROR (rolled back): " + $_.Exception.Message }
$c.Close()
