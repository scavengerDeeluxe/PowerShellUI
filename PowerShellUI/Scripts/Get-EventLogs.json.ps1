
function Get-WMIEventsOnDate
{
	param (
		[string]$ComputerName = $target,
		[int[]]$EventIDs = $eventIdsHere,
		[datetime]$Date
	)
	
	# Calculate start and end of the selected day
	$dayStart = $Date.Date
	$dayEnd = $dayStart.AddDays(1).AddSeconds(-1)
	
	# Format into WMI time format
	$startWMI = $dayStart.ToString("yyyyMMddHHmmss.000000-000")
	$endWMI = $dayEnd.ToString("yyyyMMddHHmmss.000000-000")
	
	# Build event code filter
	$eventFilter = ($EventIDs | ForEach-Object { "EventCode = $_" }) -join " OR "
	
	# Final WMI filter
	$filter = "LogFile = '$LogName' AND TimeGenerated >= '$startWMI' AND TimeGenerated <= '$endWMI'"
	if ($EventIDs.Count -gt 0)
	{
		$filter += " AND ($eventFilter)"
	}
	
	try
	{
		$item = Get-WmiObject -Class Win32_NTLogEvent -ComputerName $ComputerName -Filter $filter -ErrorAction Stop |
		Select-Object TimeGenerated, EventCode, SourceName, Type, Message
		$results.APpendText($item)
	}
	catch
	{
		Write-Warning "Failed to query $ComputerName"
	}
}

function Get-WMILast24HoursEvents
{
	param (
		[string]$ComputerName = $target,
		[int[]]$EventIDs = $eventIdsHere, # Default: system startup/shutdown
		[int]$HoursToGet = $hours
	)
	
	# Get the WMI-formatted time string for 24 hours ago
	$startTime = (Get-Date).AddHours(-24).ToString("yyyyMMddHHmmss.000000-000")
	
	# Build the EventCode filter (e.g., "EventCode = 6005 OR EventCode = 6006")
	$eventFilter = ($EventIDs | ForEach-Object { "EventCode = $_" }) -join " OR "
	
	# Full WMI filter string
	$filter = "LogFile = '$LogName' AND TimeGenerated >= '$startTime'"
	if ($eventIDs.Count -gt 0)
	{
		$filter += " AND ($eventFilter)"
	}
	
	try
	{
		$results = Get-WmiObject -Class Win32_NTLogEvent -ComputerName $ComputerName -Filter $filter -ErrorAction Stop
		$results | Select-Object TimeGenerated, EventCode, SourceName, Type, Message
		$output.appendText $results
	}
	catch
	{
		Write-Warning "Failed to query $ComputerName"
	}
}

$eventIdsHere = $checkedListBox_EventIDs.CheckedItems

$target = $computername.text
if ($ComboBox.SelectedItem -eq 'On This Date') { $dates = $selection.text | get-date -Format 'dd/MM/yyyy'
	Get-WMIEventsOnDate -Date $dates -ComputerName $target -EventIDs $eventIdsHere}
elseif ($combobox.selectedItem -eq "Last X Hours")
{
	Get-WMILast24HoursEvents -HoursToGet $hours.text -ComputerName $target -EventIDs $eventIdsHere
}
    
