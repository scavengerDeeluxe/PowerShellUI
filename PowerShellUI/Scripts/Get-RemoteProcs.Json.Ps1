$1 = $targetpc

function Get-RemoteProcs {

get-wmiobject -class win32_process -computername $remotepc | select-object name,processid,commandline | FT

}
get-remoteprocs 
