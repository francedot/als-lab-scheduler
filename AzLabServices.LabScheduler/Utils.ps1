$azLsModuleSource = "https://raw.githubusercontent.com/Azure/azure-devtestlab/master/samples/ClassroomLabs/Modules/Library/Az.LabServices.psm1"

function Import-AzLsModule {
    return Import-RemoteModule -Source $azLsModuleSource -ModuleName "Az.LabServices.psm1"
}

function Import-RemoteModule {
    param(
        [ValidateNotNullOrEmpty()]
        [string] $Source,
        [ValidateNotNullOrEmpty()]
        [string] $ModuleName
    )
  
    $modulePath = Join-Path -Path (Resolve-Path ./) -ChildPath $ModuleName
  
    if (Test-Path -Path $modulePath) {
        # if the file exists, delete it - just in case there's a newer version, we always download the latest
        Remove-Item -Path $modulePath
    }
  
    $WebClient = New-Object System.Net.WebClient
    $WebClient.DownloadFile($Source, $modulePath)
    Import-Module $modulePath
  
    return $modulePath
}

function Import-AzLsLabSchedulerAssembly {
    Add-Type -Path ".\AzLabServices.LabScheduler\bin\Release\AzLabServices.LabScheduler.dll"
}

# Import (with . syntax) this at the start of each begin block
function BeginPreamble {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseDeclaredVarsMoreThanAssignments", "", Scope = "Function")]
    param()
    Get-CallerPreference -Cmdlet $PSCmdlet -SessionState $ExecutionContext.SessionState
    $callerEA = $ErrorActionPreference
    $ErrorActionPreference = 'Stop'
}

# TODO: consider reducing function below to just get ErrorActionPreference
# Taken from https://gallery.technet.microsoft.com/scriptcenter/Inherit-Preference-82343b9d
function Get-CallerPreference {
    [CmdletBinding(DefaultParameterSetName = 'AllVariables')]
    param (
        [Parameter(Mandatory = $true)]
        [ValidateScript( { $_.GetType().FullName -eq 'System.Management.Automation.PSScriptCmdlet' })]
        $Cmdlet,

        [Parameter(Mandatory = $true)]
        [System.Management.Automation.SessionState]
        $SessionState,

        [Parameter(ParameterSetName = 'Filtered', ValueFromPipeline = $true)]
        [string[]]
        $Name
    )

    begin {
        $filterHash = @{ }
    }

    process {
        if ($null -ne $Name) {
            foreach ($string in $Name) {
                $filterHash[$string] = $true
            }
        }
    }

    end {
        # List of preference variables taken from the about_Preference_Variables help file in PowerShell version 4.0

        $vars = @{
            'ErrorView'                     = $null
            'FormatEnumerationLimit'        = $null
            'LogCommandHealthEvent'         = $null
            'LogCommandLifecycleEvent'      = $null
            'LogEngineHealthEvent'          = $null
            'LogEngineLifecycleEvent'       = $null
            'LogProviderHealthEvent'        = $null
            'LogProviderLifecycleEvent'     = $null
            'MaximumAliasCount'             = $null
            'MaximumDriveCount'             = $null
            'MaximumErrorCount'             = $null
            'MaximumFunctionCount'          = $null
            'MaximumHistoryCount'           = $null
            'MaximumVariableCount'          = $null
            'OFS'                           = $null
            'OutputEncoding'                = $null
            'ProgressPreference'            = $null
            'PSDefaultParameterValues'      = $null
            'PSEmailServer'                 = $null
            'PSModuleAutoLoadingPreference' = $null
            'PSSessionApplicationName'      = $null
            'PSSessionConfigurationName'    = $null
            'PSSessionOption'               = $null

            'ErrorActionPreference'         = 'ErrorAction'
            'DebugPreference'               = 'Debug'
            'ConfirmPreference'             = 'Confirm'
            'WhatIfPreference'              = 'WhatIf'
            'VerbosePreference'             = 'Verbose'
            'WarningPreference'             = 'WarningAction'
        }


        foreach ($entry in $vars.GetEnumerator()) {
            if (([string]::IsNullOrEmpty($entry.Value) -or -not $Cmdlet.MyInvocation.BoundParameters.ContainsKey($entry.Value)) -and
                ($PSCmdlet.ParameterSetName -eq 'AllVariables' -or $filterHash.ContainsKey($entry.Name))) {
                $variable = $Cmdlet.SessionState.PSVariable.Get($entry.Key)

                if ($null -ne $variable) {
                    if ($SessionState -eq $ExecutionContext.SessionState) {
                        Set-Variable -Scope 1 -Name $variable.Name -Value $variable.Value -Force -Confirm:$false -WhatIf:$false
                    }
                    else {
                        $SessionState.PSVariable.Set($variable.Name, $variable.Value)
                    }
                }
            }
        }

        if ($PSCmdlet.ParameterSetName -eq 'Filtered') {
            foreach ($varName in $filterHash.Keys) {
                if (-not $vars.ContainsKey($varName)) {
                    $variable = $Cmdlet.SessionState.PSVariable.Get($varName)

                    if ($null -ne $variable) {
                        if ($SessionState -eq $ExecutionContext.SessionState) {
                            Set-Variable -Scope 1 -Name $variable.Name -Value $variable.Value -Force -Confirm:$false -WhatIf:$false
                        }
                        else {
                            $SessionState.PSVariable.Set($variable.Name, $variable.Value)
                        }
                    }
                }
            }
        }

    } # end

} # function Get-CallerPreference

function ConvertToUri($resource) {
    "https://management.azure.com" + $resource.Id
}

function InvokeRest($Uri, $Method, $Body, $params) {
    $authHeaders = GetHeaderWithAuthToken

    $fullUri = $Uri + '?' + $ApiVersion

    if ($params) { $fullUri += '&' + $params }

    if ($body) { Write-Verbose $body }    
    $result = Invoke-WebRequest -Uri $FullUri -Method $Method -Headers $authHeaders -Body $Body -UseBasicParsing
    $resObj = $result.Content | ConvertFrom-Json
    
    # Happens with Post commands ...
    if (-not $resObj) { return $resObj }

    if (Get-Member -inputobject $resObj -name "Value" -Membertype Properties) {
        return $resObj.Value | Enrich
    }
    else {
        return $resObj | Enrich
    }
}

function Get-AzureRmCachedAccessToken() {
    $ErrorActionPreference = 'Stop'
    Set-StrictMode -Off

    $azureRmProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
    if (-not $azureRmProfile.Accounts.Count) {
        Write-Error "Ensure you have logged in before calling this function."
    }

    $currentAzureContext = Get-AzContext
    $profileClient = New-Object Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient($azureRmProfile)
    Write-Debug ("Getting access token for tenant" + $currentAzureContext.Subscription.TenantId)
    $token = $profileClient.AcquireAccessToken($currentAzureContext.Subscription.TenantId)
    return $token.AccessToken
}

function GetHeaderWithAuthToken {

    $authToken = Get-AzureRmCachedAccessToken
    Write-Debug $authToken

    $header = @{
        'Content-Type'  = 'application/json'
        "Authorization" = "Bearer " + $authToken
        "Accept"        = "application/json;odata=fullmetadata"
    }

    return $header
}

$ApiVersion = 'api-version=2019-01-01-preview'

# This function adds properties to the returned resource to make it more easily queryable and reportable
function Enrich {
    [CmdletBinding()]
    param([parameter(Mandatory = $true, ValueFromPipeline = $true)] $resource)

    begin { . BeginPreamble }

    process {
        foreach ($rs in $resource) {
            if ($rs.id) {
                $parts = $rs.id.Split('/')
                $len = $parts.Count

                # The id for a VM looks like this:
                # /subscriptions/SS/resourcegroups/RG/providers/microsoft.labservices/labaccounts/LA/labs/LAB/environmentsettings/default/environments/VM

                # The code below figures out the kind of resources by how deeep in the Id we are and add appropriate properties depending on the type
                if ($len -ge 4) { $rs | Add-Member -MemberType NoteProperty -Name ResourceGroupName -Value $parts[4] -Force }
                if ($len -ge 8) { $rs | Add-Member -MemberType NoteProperty -Name LabAccountName -Value $parts[8] -Force }
                if ($len -ge 10) { $rs | Add-Member -MemberType NoteProperty -Name LabName -Value $parts[10] -Force }
      
                if (($len -eq 15) -and ($parts[13] -eq 'Environments')) {
                    # it's a vm
                    if (Get-Member -inputobject $rs.properties -name "lastKnownPowerState" -Membertype Properties) {
                        $rs | Add-Member -MemberType NoteProperty -Name Status -Value $rs.properties.lastKnownPowerState -Force
                    }
                    else {
                        $rs | Add-Member -MemberType NoteProperty -Name Status -Value 'Unknown' -Force
                    }

                    if ($rs.properties.isClaimed -and $rs.properties.claimedByUserPrincipalId) {
                        $rs | Add-Member -MemberType NoteProperty -Name UserPrincipal -Value $rs.properties.claimedByUserPrincipalId -Force           
                    }
                    else {
                        $rs | Add-Member -MemberType NoteProperty -Name UserPrincipal -Value '' -Force           
                    }
                }
            }
            return $rs
        }
    }
    end { }
}

# A revised version of New-AzLabSchedule taking in a DateTime for 'FromDate' and 'ToDate' instead of string
function New-AzLabScheduleWithDate {
    param(
        [parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true, HelpMessage = "Lab to associate the schedule to.", ValueFromPipeline = $true)]
        [ValidateNotNullOrEmpty()]
        $Lab,

        [parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true, HelpMessage = "Start Date for the class.")]
        [ValidateNotNullOrEmpty()]
        [datetime] $FromDate = (Get-Date),

        [parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true, HelpMessage = "End Date for the class.")]
        [ValidateNotNullOrEmpty()]
        [datetime] $ToDate = (Get-Date).AddMonths(4),
    
        [parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true, HelpMessage = "The Windows time zone id associated with labVmStartup (E.g. UTC, Pacific Standard Time, Central Europe Standard Time).")]
        [ValidateLength(3, 40)]
        [string] $TimeZoneId = "W. Europe Standard Time",
    
        [parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true, HelpMessage = "Notes for the class meeting.")]
        $Notes = ""
    )
    begin { . BeginPreamble }
    process {
        try {
            foreach ($l in $Lab) {
                # TODO: ask for algo to generate schedule names
                $name = 'Default_' + (Get-Random -Minimum 10000 -Maximum 99999)

                $uri = (ConvertToUri -resource $Lab) + "/environmentsettings/default/schedules/$name"

                $fullStart = $FromDate.ToString('o')
                $fullEnd = $ToDate.ToString('o')

                # TODO: Consider checking parameters more instead of just plucking the ones I need
                $body = @{
                    properties = @{
                        enableState = 'Enabled'
                        start       = $fullStart
                        end         = $fullEnd
                        timeZoneId  = $TimeZoneId
                        startAction = @{
                            enableState = "Enabled"
                            actionType  = "Start"
                        }
                        endAction   = @{
                            enableState = "Enabled"
                            actionType  = "Stop"
                        }
                        notes       = $Notes
                    }
                } | ConvertTo-Json -depth 10

                Write-Verbose $body

                return InvokeRest -Uri $uri -Method 'Put' -Body $body
            }
        }
        catch {
            Write-Error -ErrorRecord $_ -EA $callerEA
        }
    }
    end { }
}