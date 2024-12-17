Describe "Find-AffectedProjects" {
    BeforeAll {
        . $PSScriptRoot/Find-AffectedProjects.ps1
    }


    Context "When no hosts are affected" {

        It 'should return nothing' {
            $affectedProjectsJson = "[]" 

            $hostNamesJson = "{
                'foo': 'Bar.csproj',
                'bar': 'FooBar.csproj'
              }"

            $expectedResult = "{
                'foo': '',
                'bar': ''
              }" | ConvertFrom-Json | ConvertTo-Json -Compress

            $affectedHosts = Find-AffectedHosts -AffectedProjects $affectedProjectsJson -HostNames $hostNamesJson
            $affectedHosts | Should -Be $expectedResult
        }
    }

    Context "When one host is affected by changes" {

        It 'should return nothing' {
            $affectedProjectsJson = "[
            {
                'Name': 'Bar',
                'Filepath': 'source/somePath/Bar.csproj'
            },
            {
                'Name': 'BarBaz',
                'Filepath': 'source/somePath/BarBaz.csproj'
            }         
            ]" 

            $hostNamesJson = "{
                'foo': 'Bar.csproj',
                'bar': 'FooBar.csproj'
              }"

            $expectedResult = "{
                'foo': 'true',
                'bar': ''
              }" | ConvertFrom-Json | ConvertTo-Json -Compress

            $affectedHosts = Find-AffectedHosts -AffectedProjects $affectedProjectsJson -HostNames $hostNamesJson
            $affectedHosts | Should -Be $expectedResult
        }
    }    
}