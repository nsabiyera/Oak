Start-Service winrm
$nlm = [Activator]::CreateInstance([Type]::GetTypeFromCLSID([Guid]"{DCB00C01-570F-4A9B-8D69-199FDBA5723B}"))
$connections = $nlm.getnetworkconnections()
$connections |foreach {
    if ($_.getnetwork().getcategory() -eq 0)
    {
        $_.getnetwork().setcategory(1)
    }
}
Enable-PSRemoting -force
set-item wsman:localhost\Shell\MaxMemoryPerShellMB 2048
icm -computer localhost { C:\development\oak\sample%20apps\borrowedgames\borrowedgames.ui.tests\bin\debug\borrowedgames.ui.tests.exe }

