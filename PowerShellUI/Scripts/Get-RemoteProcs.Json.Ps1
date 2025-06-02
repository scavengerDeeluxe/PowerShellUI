$remotepc = $1

function Get-RemoteProcs {

get-wmiobject -class win32_process -computername $remotepc 

}
get-remoteprocs 
