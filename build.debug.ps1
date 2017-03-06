$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$CAKE_DIR = Join-Path $TOOLS_DIR "Cake"
$CAKE_EXE = Join-Path $CAKE_DIR "Cake.exe"

if (-Not (Test-Path $CAKE_EXE)) {
	Write-Host "Cake is not installed. Running normal build";
	& ".\build.ps1"
}

Write-Host "Debugging Cake in Visual Studio under Administration Priviliges with opened 'build.cake' file ..."	 
Push-Location $CAKE_DIR
.\Cake.exe ..\..\build.cake -target=Default --debug
Pop-Location
exit $LASTEXITCODE
