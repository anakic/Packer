﻿# run this script from the output directory, e.g. C:\Projects\Packer\Packer2.PS\bin\Debug\net6.0

& "C:\tools\nuget.exe" pack .\Packer2.PS.nuspec -OutputFileNamesWithoutVersion  -Properties NoWarn=NU5100
& "C:\tools\nuget.exe" push .\Packer2.PS.nupkg -source Packer2RepoPackageSource -ApiKey "<anystring>"