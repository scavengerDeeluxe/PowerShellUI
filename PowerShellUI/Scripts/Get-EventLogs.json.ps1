

function Get-wmieventsondate
{
	param (
		[string]$ComputerName,
		[int[]]$EventID,
		[datetime]$Date,
  		[int]$HoursToGet
	)

if($date){
	$dayStart = $Date.Date
	$dayEnd = $dayStart.AddDays(1).AddSeconds(-1)
	
	# Format into WMI time format
	$startWMI = $dayStart.ToString("yyyyMMddHHmmss.000000-000")
	$endWMI = $dayEnd.ToString("yyyyMMddHHmmss.000000-000")
 
try{    

$results = Get-WinEvent -computername $target -FilterHashtable @{
    LogName = 'Security,System,Application'
    ID = $EventID.ID
    StartTime = $start
    EndTime = $end
}

}
	catch
	{
		$richtextbox1.appendtext("Failed to query $ComputerName")
	}
 }
 elseif($hourstoget){
 try{    # Get the WMI-formatted time string for 24 hours ago
	# $start = (Get-Date).AddDays(-5)
$results = Get-WinEvent -computername $target -FilterHashtable @{
    LogName = 'Security,System,Application'
    ID = $EventID.ID
    Hours = $(get-date).addHours($HoursToGet * -1)
}
 }
catch{}

}
 }



Write-Host "Computer: $ComputerName"
Write-Host "Events: $EventIDs"
Write-Host "Time: $date"

$output = get-wmieventsondate  @inputvalues

