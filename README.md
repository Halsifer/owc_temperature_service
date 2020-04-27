# owc_temperature_service
A temperature persisting web service

The Databases project is a database migration tool to create the MySQL table required by TemperatureService. Run this tool by executing:
```
cd ./TemperatureDatabases
dotnet run <connectionString>
```

Once databases are deployed, set the environment variable `ConnectionStrings__TemperatureConnection` to the connection string and run the TemperatureService by executing:

```
cd ./TemperatureService
dotnet run
```
