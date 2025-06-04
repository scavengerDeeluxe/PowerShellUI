$remotepc = $target

get-wmiobject -class win32_process -computername $remotepc 
