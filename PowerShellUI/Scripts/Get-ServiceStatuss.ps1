{
  "script": {
    "ScriptID": 838389,
    "name": "Remove-GhostPNPDevices",
    "description": "Removes phantom devices using pnputil",
    "maker": {
      "name": "josh dahle",
      "contact": "josh@gmail.com"
    },
    "source_url": "https://raw.githubusercontent.com/scavengerDeeluxe/PowerShellUI/master/PowerShellUI/Scripts/Remove-GhostPNPDevices.ps1",
    "runs": 55,
    "rating": 5,
    "risk": 0,
    "command": "Get-PnpDevice | ForEach { pnputil /delete-device $_.InstanceId }"
  },
  "inputs": [
    {
      "name": "ComputerName",
      "label": "Target Computer",
      "type": "string",
      "required": true,
      "default": "r32-1991829"
    }
  ],
  "output": {
    "type": "console",
    "format": "text"
  },
  "conditions": {
    "success": {
      "keywords": ["deleted successfully", "operation completed"],
      "exit_code": 0
    },
    "failure": {
      "keywords": ["error", "failed"],
      "exit_code": [1]
    }
  },
  "executions": [
    {
      "ScriptID": 838389,
      "ExecutionTime": "2025-05-25T00:00:00",
      "Inputs": "r32-1991829",
      "Outputs": "placeholder",
      "Duration": 0,
      "Targets": "computername",
      "Domain": "pca.state.mn.us",
      "Details": "nil",
      "ExecutionDetails": [
        {
          "RunningID": 28,
          "ScriptName": "Remove-GhostPNPDevices",
          "Target": "r32-1991829",
          "Inputs": "0",
          "Outputs": "returnText",
          "Result": 1,
          "Duration": 25
        }
      ]
    }
  ]
}
