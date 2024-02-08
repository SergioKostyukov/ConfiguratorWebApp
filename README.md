# ConfigurationWebApp

This project implemented a web application on asp.net core 8 mvc.

Its functionality allows:
+ **Reading** the configuration file *(.json, .txt)*
+ **Saving** data in the database *(MSSQL)*
+ **Displaying** the configuration on the page in the form of a hierarchical tree
+ Configuration **navigation** via URL

Simple logging of the main processes was also implemented, which allows to analyze the operation of the application and check the status of objects at various stages of execution.

Unit tests were not refined, but the function of saving the configuration from the database to a file *(output.json)* is present.
So you can "manually" compare the values and test the correctness of error handling.

## Getting started:
+ filling out appsettings.json
  to connect to the database:
  ```json
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConfigurationApp": "Server=.;Database=ConfigurationAppDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
  ```
  specifying the file path to read:
  ```json
  "ConfigurationFilePath": "Data/ConfigExamples/config1.json"
  ```
+ database and table generation. Use comand in Package Manager Console:
  ```
    Update-Database
  ```

## Usage:
1. **Specify** the path to the configuration file *(in appsettings.json)*
2. **Start** the project. The complete configuration will be displayed on the screen
3. **Navigate** using the URL *(example: .../keyA/keyB)*
4. **Compare** the input and output files to check the correctness of the application
