<#
.SYNOPSIS
    Sets up (or tears down) everything the pscp based unit tests need on a dev machine.

.DESCRIPTION
    The Category("NetworkTest") tests in SuperPuttyUnitTests drive a real pscp.exe against a real
    ssh server.  A listener on port 22 is not enough, they need all four of:

      1. sshd listening on port 22, reachable from Windows under SuperPuTTY.ScpTests.KnownHost;
      2. the SuperPuTTY.ScpTests.UserName account on that server, accepting the
         SuperPuTTY.ScpTests.Password as a password (pscp is invoked with "-pw");
      3. the server's host key cached in HKCU\Software\SimonTatham\PuTTY\SshHostKeys for KnownHost,
         and deliberately NOT cached for SuperPuTTY.ScpTests.UnKnownHost - PscpClient aborts on an
         uncached key and ListDirHostNoKey asserts exactly that;
      4. a non-empty remote home directory (ListDirSuccess asserts Files.Count > 0), plus
         /home/<user> for LocalToRemote.

    PscpClient only passes "-pw" when the SuperPuTTY AllowPlainTextPuttyPasswordArg setting is on.
    Inside VSTest that setting is read from %APPDATA%\testhost\testhost.settings (the test host's
    product name is "testhost"), so this script writes that file too.

    The server here is the sshd of a WSL distro.  GitHub hosted Windows runners have no WSL2, so
    the CI job keeps these tests skipped; see .github/workflows/ci.yml.

.PARAMETER Distro
    WSL distro to use, the first installed one when omitted.

.PARAMETER SetPassword
    Also set the password of an account that already exists.  Off by default so that a pre-existing
    account (e.g. your own WSL user) is never touched; set it yourself with "wsl -u root passwd".

.PARAMETER KeepUnknownHostKey
    Do not delete the cached host key of UnKnownHost.  Leaving it there makes ListDirHostNoKey
    assert against a stale registry entry instead of an uncached server.

.PARAMETER Teardown
    Undo what this script did: stop sshd, drop the marker file, remove the cached host keys and,
    with -RemoveUser, the test account.

.PARAMETER RemoveUser
    With -Teardown, also delete the account created for the tests.

.EXAMPLE
    .\tools\scp-test-server.ps1
    Set everything up and verify it with the same command the tests use.

.EXAMPLE
    dotnet test SuperPuttyUnitTests/SuperPuttyUnitTests.csproj -p:BuildPuttyEmbed=false
    Run the tests once the script above reports success.
#>
[CmdletBinding()]
param(
    [string]$Distro,
    [switch]$SetPassword,
    [switch]$KeepUnknownHostKey,
    [switch]$Teardown,
    [switch]$RemoveUser
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$TestConfig = Join-Path $RepoRoot "SuperPuttyUnitTests\app.config"
$Kh2Reg = Join-Path $RepoRoot "putty\contrib\kh2reg.py"
$HostKeyPath = "HKCU:\Software\SimonTatham\PuTTY\SshHostKeys"
$MarkerName = "superputty-test.txt"
$Port = 22   # the pscp client tests build their SessionData with Port = 22

function Write-Step($message) { Write-Host "-> $message" -ForegroundColor Cyan }
function Write-Done($message) { Write-Host "   $message" -ForegroundColor Green }
function Write-Warn($message) { Write-Host "   $message" -ForegroundColor Yellow }

function Get-WslDistros {
    # wsl.exe writes its own messages (the -l list) as UTF-16, which powershell 5.1 mangles unless
    # the console output encoding is switched for that call
    $previous = [Console]::OutputEncoding
    try {
        [Console]::OutputEncoding = [System.Text.Encoding]::Unicode
        & wsl.exe -l -q
    } finally {
        [Console]::OutputEncoding = $previous
    }
}

function Invoke-WslRoot([string[]]$lines) {
    # powershell puts a CR on every line it pipes to a native program, tr removes them again
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $lines -join "`n" | & wsl.exe -d $Distro -u root -- bash -c "tr -d '\r' | bash -s"
        $code = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previous
    }
    if ($code -ne 0) { throw "the setup script failed inside $Distro (exit $code)" }
}

function Test-Port($hostName, $port) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        return $client.ConnectAsync($hostName, $port).Wait(3000) -and $client.Connected
    } catch {
        return $false
    } finally {
        $client.Close()
    }
}

function Remove-HostKeys($hostName) {
    if (-not (Test-Path $HostKeyPath)) { return }
    $suffix = "@${Port}:$hostName"
    foreach ($name in (Get-Item $HostKeyPath).GetValueNames()) {
        if ($name.EndsWith($suffix)) {
            Write-Warn "removing cached host key $name"
            Remove-ItemProperty -Path $HostKeyPath -Name $name
        }
    }
}

# ---- the settings the tests use, app.config is the single source of truth -----------------------
if (-not (Test-Path $TestConfig)) { throw "no test config file at $TestConfig" }
$config = [xml](Get-Content $TestConfig -Raw)
$settings = @{}
foreach ($entry in $config.configuration.appSettings.add) { $settings[$entry.key] = $entry.value }

$user = $settings["SuperPuTTY.ScpTests.UserName"]
$pass = $settings["SuperPuTTY.ScpTests.Password"]
$knownHost = $settings["SuperPuTTY.ScpTests.KnownHost"]
$unknownHost = $settings["SuperPuTTY.ScpTests.UnKnownHost"]

$configuredPscp = $settings["SuperPuTTY.ScpTests.PscpLocation"]
$pscp = $configuredPscp
if (-not [System.IO.Path]::IsPathRooted($pscp)) {
    # same resolution as ScpConfig.ResolvePscpLocation, plus a fallback that finds any pscp.exe
    $candidate = Join-Path $RepoRoot $pscp
    if (-not (Test-Path $candidate)) {
        $candidate = (Get-ChildItem (Join-Path $RepoRoot "putty\build") -Recurse -Filter "pscp.exe" -ErrorAction SilentlyContinue |
            Select-Object -First 1).FullName
    }
    $pscp = $candidate
}
if (-not $pscp -or -not (Test-Path $pscp)) {
    throw "pscp.exe not found ('$configuredPscp'). Configure putty with cmake and build the pscp target."
}

if (-not $Distro) {
    $Distro = (Get-WslDistros | Where-Object { $_ } | Select-Object -First 1)
    if (-not $Distro) { throw "no WSL distro installed; run 'wsl --install -d Ubuntu' first" }
}

Write-Host "distro=$Distro user=$user knownHost=$knownHost unknownHost=$unknownHost port=$Port"
Write-Host "pscp=$pscp"

# ---- teardown ----------------------------------------------------------------------------------
if ($Teardown) {
    Write-Step "stopping ssh and removing the marker file"
    $lines = @(
        "rm -f /home/$user/$MarkerName",
        # ssh.socket holds port 22 and wakes sshd back up, so it has to go too
        "systemctl stop ssh ssh.socket >/dev/null 2>&1 || true",
        "service ssh stop >/dev/null 2>&1 || true",
        "pkill -x sshd >/dev/null 2>&1 || true")
    if ($RemoveUser) {
        $lines += "userdel -r -f $user >/dev/null 2>&1 && echo 'user $user removed' || true"
    }
    Invoke-WslRoot $lines
    Write-Step "removing cached host keys"
    Remove-HostKeys $knownHost
    Remove-HostKeys $unknownHost
    Write-Done "done, the NetworkTest cases skip again"
    exit 0
}

# ---- 1. sshd on port 22, the account, a non-empty home ------------------------------------------
Write-Step "installing openssh-server, the $user account and sshd in $Distro"
$lines = @(
    "set -e",
    "if ! dpkg -s openssh-server >/dev/null 2>&1; then",
    "  export DEBIAN_FRONTEND=noninteractive",
    "  apt-get update -qq >/dev/null && apt-get install -y -qq openssh-server >/dev/null",
    "fi",
    "created=0",
    "if ! id $user >/dev/null 2>&1; then useradd -m -s /bin/bash $user; created=1; fi",
    "usermod -e '' $user >/dev/null 2>&1 || true",   # sshd rejects password auth on an expired account
    "if ! grep -qrs '^PasswordAuthentication yes' /etc/ssh/sshd_config /etc/ssh/sshd_config.d; then",
    "  echo 'PasswordAuthentication yes' > /etc/ssh/sshd_config.d/99-superputty-tests.conf",
    "fi",
    "systemctl start ssh.socket >/dev/null 2>&1 || true",
    "systemctl restart ssh >/dev/null 2>&1 || service ssh restart >/dev/null 2>&1 || { pkill -x sshd; /usr/sbin/sshd; }",
    "mkdir -p /home/$user",
    "echo written by tools/scp-test-server.ps1 > /home/$user/$MarkerName",
    "chown -R $user /home/$user")

if ($SetPassword) {
    $lines += 'echo "' + $user + ':' + $pass + '" | chpasswd'
} else {
    # only an account created here gets the password from app.config, an existing one is left alone
    $lines += 'if [ "$created" = 1 ]; then echo "' + $user + ':' + $pass + '" | chpasswd; fi'
}
Invoke-WslRoot $lines

$wslIp = (& wsl.exe -d $Distro hostname -I)
Write-Done "wsl ip:$wslIp"

Write-Step "waiting for ${knownHost}:$Port to answer from Windows"
$ready = $false
for ($try = 0; $try -lt 10 -and -not $ready; $try++) {
    $ready = Test-Port $knownHost $Port
    if (-not $ready) { Start-Sleep -Milliseconds 500 }
}
if (-not $ready) {
    Write-Warn "nothing answers on ${knownHost}:$Port from Windows"
    Write-Warn "WSL2 forwards localhost into the distro; if SuperPuTTY.ScpTests.KnownHost is an old"
    Write-Warn "WSL ip, it has changed - put the wsl ip printed above into $TestConfig and re-run"
    exit 1
}
Write-Done "port $Port reachable"

# ---- 2/3. host key cached for KnownHost, not cached for UnKnownHost ------------------------------
Write-Step "caching the host key of $knownHost for pscp"
$previous = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try { $pubKeys = & wsl.exe -d $Distro -u root -- bash -c "cat /etc/ssh/ssh_host_*_key.pub" }
finally { $ErrorActionPreference = $previous }
if (-not $pubKeys) { throw "could not read the host keys of $Distro" }

$knownHostsFile = Join-Path $env:TEMP "scp-test-known_hosts"
$regFile = Join-Path $env:TEMP "scp-test-hostkeys.reg"
Set-Content -Path $knownHostsFile -Encoding ascii -Value ($pubKeys | ForEach-Object { "$knownHost $_" })

$python = (Get-Command python -ErrorAction SilentlyContinue)
if (-not (Test-Path $Kh2Reg) -or -not $python) {
    if (-not (Test-Path $Kh2Reg)) { $why = "$Kh2Reg is missing (git submodule update --init putty)" }
    else { $why = "no python on PATH" }
    Write-Warn $why
    Write-Warn "accept the key once by hand instead, without -batch:"
    Write-Warn "  & `"$pscp`" -ls -P $Port `"$user@${knownHost}:.`""
} else {
    & $python.Source $Kh2Reg --win $knownHostsFile -o $regFile
    if (-not (Test-Path $regFile)) { throw "kh2reg.py did not produce $regFile" }
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { reg import $regFile 2>&1 | Out-Null } finally { $ErrorActionPreference = $previous }
    $count = ($pubKeys | Measure-Object).Count
    Write-Done "cached $count host key(s) for $knownHost under $HostKeyPath"
}

if ($KeepUnknownHostKey) {
    Write-Warn "keeping any host key cached for $unknownHost, ListDirHostNoKey may mislead you"
} else {
    Remove-HostKeys $unknownHost
}

# ---- PscpClient only sends -pw when this setting is on ------------------------------------------
Write-Step "turning on AllowPlainTextPuttyPasswordArg for the test host"
$settingsFile = Join-Path $env:APPDATA "testhost\testhost.settings"
if (Test-Path $settingsFile) {
    $xml = [xml](Get-Content $settingsFile -Raw)
    $node = $xml.SelectSingleNode("Settings/AllowPlainTextPuttyPasswordArg")
    if (-not $node) {
        $node = $xml.CreateElement("AllowPlainTextPuttyPasswordArg")
        $xml.SelectSingleNode("Settings").AppendChild($node) | Out-Null
    }
    $node.InnerText = "True"
    $xml.Save($settingsFile)
} else {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $settingsFile) | Out-Null
    Set-Content -Path $settingsFile -Encoding utf8 -Value @(
        '<?xml version="1.0" encoding="utf-8"?>',
        '<Settings>',
        '  <AllowPlainTextPuttyPasswordArg>True</AllowPlainTextPuttyPasswordArg>',
        '</Settings>')
}
Write-Done "AllowPlainTextPuttyPasswordArg=True in $settingsFile"

# ---- 4. check it the way the tests do it ---------------------------------------------------------
Write-Step "listing a directory through pscp"
# pscp writes its normal chatter ("Server refused our key", progress) to stderr, which powershell
# turns into an error record when $ErrorActionPreference is Stop
$previous = $ErrorActionPreference
$ErrorActionPreference = "Continue"
$listing = & $pscp -batch -ls -pw $pass -P $Port "$user@${knownHost}:." 2>&1
$exitCode = $LASTEXITCODE
$ErrorActionPreference = $previous
if ($exitCode -ne 0) {
    Write-Warn "pscp failed (exit $exitCode):"
    $listing | ForEach-Object { Write-Host "   $_" }
    if ("$listing" -match "password|Access denied") {
        Write-Warn "password auth rejected - set it with 'wsl -u root passwd $user' or re-run with -SetPassword"
    }
    exit 1
}
$count = ($listing | Measure-Object).Count
Write-Done "pscp listed the home of $user ($count lines)"

Write-Host ""
Write-Host "run the tests with:"
Write-Host "  dotnet test SuperPuttyUnitTests/SuperPuttyUnitTests.csproj -p:BuildPuttyEmbed=false"
Write-Host "undo this with:"
Write-Host "  .\tools\scp-test-server.ps1 -Teardown"
