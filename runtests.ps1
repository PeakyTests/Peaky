param (
	[string] $apikey,
	[string] $environment,
	[string] $application,
	[string] $tag,
	[string] $host
)

$alltests = (Invoke-RestMethod https://$host/tests/$environment/$($application)?apikey=$apikey).Tests

$matchingTests = $alltests |
	where {
		if (test-path variable:tag) { ($_.Tags) -and $_.Tags -contains $tag } else { $true }
	}

$failingTests = @()

$matchingTests | foreach {
	$url = [Uri]$_.Url
	Write-Host -NoNewline "Running $($url.PathAndQuery)..."
	Try { 
		Invoke-WebRequest "$($url)?apikey=$apikey" | Out-Null
		Write-Host " passed"
	} Catch { 
		Write-Error " failed"
		$failingTests += $url.PathAndQuery
	}
}

exit $failingTests.Count
