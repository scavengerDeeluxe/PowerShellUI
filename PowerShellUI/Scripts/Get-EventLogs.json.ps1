$target = $computername.text
$EventIdsHere = $eventids.selectedItems
$hours = $lasthours.text
$dates = $specificdate.value
$logs = $logbox.text



function Get-WMIEventsOnDate
{
	param (
		[string]$ComputerName = $target,
		[int[]]$EventIDs = $eventIdsHere,
		[datetime]$Date
	)
    write-host "Grabbing date range"
	# Calculate start and end of the selected day
	$dayStart = $Date.Date
	$dayEnd = $dayStart.AddDays(1).AddSeconds(-1)
	
	# Format into WMI time format
	$startWMI = $dayStart.ToString("yyyyMMddHHmmss.000000-000")
	$endWMI = $dayEnd.ToString("yyyyMMddHHmmss.000000-000")
	
	# Build event code filter
#	$eventFilter = ($EventIDs | ForEach-Object { "EventCode = $_" }) -join " OR "
	
	# Final WMI filter
$start = $dates
$end = $dates.adddays(1).addseconds(-1)

    write-host "grabbing events from $start to $end"
    
    # Get the WMI-formatted time string for 24 hours ago
    # $start = (Get-Date).AddDays(-5)
    
    # Query WMI for events

try{    # Get the WMI-formatted time string for 24 hours ago
	# $start = (Get-Date).AddDays(-5)
$results = Get-WinEvent -computername $target -FilterHashtable @{
    LogName = 'Security'
    ID = $EventIdsHere
    StartTime = $start
    EndTime = $end
}
}
	catch
	{
		$logbox.appendtext("Failed to query $ComputerName")
	}
    $
}

function Get-WMILast24HoursEvents
{
	param (
		[string]$ComputerName = $target,
		[int[]]$EventIDs = $eventIdsHere, # Default: system startup/shutdown
		[int]$HoursToGet = $hours
	)
	write-host "grabbing last $hours hours"

try{    # Get the WMI-formatted time string for 24 hours ago
	# $start = (Get-Date).AddDays(-5)
$results = Get-WinEvent -computername $target -FilterHashtable @{
    LogName = 'Security'
    ID = $EventIdsHere
    Hours = $(get-date).addHours($hours * -1)
}
}
	catch
	{
		$logbox.appendtext("Failed to query $ComputerName")
	}
}

$eventIdsHere = $checkedListBox_EventIDs.CheckedItems


if($dates){
get-wmieventsondates 
}
elseif($hours){
get-wmilast24hoursevents
}
foreach($line in $results){

    

$logbox.appendtext($line)
    $logbox.appendtext("`n")


}
