function Get-wmieventsondate
{
	param (
		[string]$ComputerName,
		[array[]]$EventIDs,
		[datetime]$Date,
  		[int]$TimeBranchInput
	)



if($date){

	$dayStart = $Date.Date
	$dayEnd = $dayStart.AddDays(1).AddSeconds(-1)
	
	# Format into WMI time format
	$startWMI = $dayStart.ToString("yyyyMMddHHmmss.000000-000")
	$endWMI = $dayEnd.ToString("yyyyMMddHHmmss.000000-000")
 $queryHashTable = @{
 LogName = 'System','Application'
ComputerName = $computername
Id = $eventIDs
StartTime = $startWMI
EndTime = $endWMI
 }
try{    

$results = Get-WinEvent -computername $computername -FilterHashtable $queryHashTable

}
	catch
	{
		$logbox.appendtext("Failed to query $ComputerName")
	}
 }
 elseif($TimeBranchInput){
     $Hours = $(get-date).addHours($TimeBranchInput * -1)
  $queryHashTable = @{
 LogName = 'System','Application'
ComputerName = $computername
Id = $eventIDs
StartTime = $startWMI
EndTime = $hours
 }
 try{    # Get the WMI-formatted time string for 24 hours ago
	# $start = (Get-Date).AddDays(-5)
$results = Get-WinEvent -computername $computername -FilterHashtable $queryHashTable
 }
catch{$logbox.appendtext('Failed to query')}

}
 }


write-host $results
write-host "HourstoGet: $TimeBranchInput"
Write-Host "Computer: $ComputerName"
Write-Host "Events: $EventIDs"
Write-Host "Time: $date"

$output = get-wmieventsondate  @inputvalues
