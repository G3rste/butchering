#!/bin/bash
version=$(grep -o '"version":\s*"\([0-9]\.*\)*"' resources/modinfo.json | grep -o '\([0-9]\.*\)*')
releasefile='bin/butchering_v'$version'.zip'
dotnet build -c release
mv bin/butchering.zip $releasefile
gh release create --generate-notes 'v'$version $releasefile 