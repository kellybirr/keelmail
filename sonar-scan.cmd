dotnet sonarscanner begin /k:"kellybirr_keelmail" /o:"kellybirr" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="4731d33701decbbffbd648e09db078c62efba363"
dotnet build "Keelmail.sln"
dotnet sonarscanner end /d:sonar.login="4731d33701decbbffbd648e09db078c62efba363" 
