$remotepc = $target

$results = get-wmiobject -class win32_process -computername $remotepc  | select name

write-host $results

return $results
