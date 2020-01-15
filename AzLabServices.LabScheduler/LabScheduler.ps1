param(
    [parameter(Mandatory = $true, HelpMessage = "Lab to associate the schedule to.", ValueFromPipeline = $true)]
    [ValidateNotNullOrEmpty()]
    $Lab,
    [parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)]
    [string]
    $IcsFIlePath
)

. ".\Utils.ps1"

Import-AzLsLabSchedulerAssembly

$scheduleHelper = New-Object AzLabServices.LabScheduler.ScheduleHelper
$labschedules = $scheduleHelper.GetLabScheduleFromICalendar($IcsFIlePath)

$labschedules | ForEach-Object {

    # All-day iCalendar events have no timezone id
    if ([string]::IsNullOrWhiteSpace($_.TimeZoneId)) {
        $_.TimeZoneId = "W. Europe Standard Time"
    }

    New-AzLabScheduleWithDate -Lab $Lab -FromDate $_.FromDate -ToDate $_.ToDate -TimeZoneId $_.TimeZoneId -Notes $_.Summary
} | Out-Null