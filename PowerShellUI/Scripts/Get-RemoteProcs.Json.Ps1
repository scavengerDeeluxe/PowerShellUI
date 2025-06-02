$remotepc = $1

get-wmiobject -class win32_process -computername $remotepc 
