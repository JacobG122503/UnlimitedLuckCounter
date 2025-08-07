# powershell -ExecutionPolicy Bypass -File "C:\Users\jgarcia\source\repos\UnlimitedLuckCounter\UnlimitedLuckCounter\AutoLaunch.ps1"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Add-Type @"
using System;
using System.Runtime.InteropServices;

public class ConsoleHelper
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    public const int SW_MAXIMIZE = 3;
    public const uint SWP_NOZORDER = 0x0004;
}
"@

$executablePath = Join-Path $scriptDir "bin\Debug\net8.0\UnlimitedLuckCounter.exe"

if (-not (Test-Path $executablePath)) {
    $executablePath = Join-Path $scriptDir "bin\Release\net8.0\UnlimitedLuckCounter.exe"
    
    if (-not (Test-Path $executablePath)) {
        Write-Host "Error: Could not find UnlimitedLuckCounter.exe in either bin\Debug\net8.0 or bin\Release\net8.0" -ForegroundColor Red
        Write-Host "Press any key to exit..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit
    }
}

Add-Type -AssemblyName System.Windows.Forms

$numberOfInstances = Read-Host "How many instances would you like to launch?"

if (-not [int]::TryParse($numberOfInstances, [ref]$null) -or [int]$numberOfInstances -le 0) {
    Write-Host "Please enter a positive number." -ForegroundColor Red
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

$numberOfInstances = [int]$numberOfInstances

function Start-LuckyCounterInstance {
    $process = Start-Process -FilePath $executablePath -PassThru
    Start-Sleep -Milliseconds 500
    
    $wshell = New-Object -ComObject wscript.shell
    $wshell.AppActivate($process.Id)
    Start-Sleep -Milliseconds 300
    
    $windowHandle = [ConsoleHelper]::GetForegroundWindow()
    
    [ConsoleHelper]::SetWindowPos($windowHandle, [IntPtr]::Zero, 100, 100, 1700, 175, [ConsoleHelper]::SWP_NOZORDER)
    
    Start-Sleep -Milliseconds 200
    
    [System.Windows.Forms.SendKeys]::SendWait("1")
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
    Start-Sleep -Milliseconds 500
    
    [System.Windows.Forms.SendKeys]::SendWait("0")
    Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
    
    return $process
}

Write-Host "Starting $numberOfInstances instances with lucky number 0..."
Write-Host "Found executable at: $executablePath"
$processes = @()

$screenWidth = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea.Width
$screenHeight = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea.Height

for ($i = 0; $i -lt $numberOfInstances; $i++) {
    Write-Host "Starting instance $($i+1)..."
    $process = Start-LuckyCounterInstance
    $processes += $process
    Start-Sleep -Seconds 1
}

Write-Host "All instances started! Press any key to terminate all instances..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

foreach ($process in $processes) {
    if (!$process.HasExited) {
        try {
            Write-Host "Terminating process $($process.Id)..."
            $process.Kill()
        } catch {
            Write-Host "Could not terminate process $($process.Id): $_" -ForegroundColor Yellow
        }
    }
}

Write-Host "Done. Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")