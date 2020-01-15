# Azure Lab Services - Lab Scheduler

This is currently a proof of concept demonstrating how to schedule classes for a lab starting from a iCalendar (.ics) file.

## Usage

```powershell
. ".\Utils.ps1"
# Requires module Az.LabServices
Import-AzLsModule

$LabAccount = Get-AzLabAccount -ResourceGroupName "azlabservicestour-rg" -LabAccountName "AZ Lab Services Tour"
$Lab = Get-AzLab -LabAccount $LabAccount -LabName "Test"

$Lab | .\LabScheduler.ps1 -IcsFIlePath "<ICS_FILE_PATH>"
```

![Azure Lab Services - Scheduled Lab](/img/scheduled-lab.png)


## A few notes
- As of now, recurrent events are splitted into one-time-only events.
- We need the logic for specifying recurrence patterns (daily, weekly..) based on the parameters supported by Lab Services APIs.
- Also need support for no-end events. For now, I am limiting occurrences up to 20 years.
