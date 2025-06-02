$remotepc = $target.text

$results = get-wmiobject -class win32_process -computername $remotepc
foreach($line in $results){
$logbox.appendtext($line)
}
