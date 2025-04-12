# MeteoInsight
The repository "WeatherDataApp" contains all necessary files for the application of MeteoInsight.

## Repository contents
```WeatherDataApp.sln``` is the Visual Studio 2022 project.

The code files are spread over multiple folders:
1. ```Models``` contains ```WeatherData.cs```, which is a structural class for the weather data gathered through the [Open Meteo API](https://open-meteo.com/).
2. ```Services``` countains two files. ```WeatherService.cs``` is the main file containing the logic for asynchronously fetching the weather data from the Open Meteo API. ```FileService.cs``` contains the logic for asynchronously exporting the weather data to JSON files.
3. ```ViewModels``` contains the file ```MainPageViewModel.cs```, which contains all logic necessary to prepare the weather data for the GUI. It handles all calculations for the weather data, loads the data into variables bounded to the GUI, and has a method for picking the folder for writing the JSON files.
4. Lastly, ```Views``` contains a XAML-file ```MainPage.xaml```, which contains a description for the GUI.

In addition to these files, there are additional files created by the .NET MAUI project to support running the application.

## Running the application
### Prerequisites
1. A Windows device.
2. The files in this repository.
3. [Visual Studio 2022](https://visualstudio.microsoft.com/)

### Steps
1. Clone the repository to your device.
2. When you're running this on Windows, you'll need to enable developer mode. For this you need to go to settings and type *"Developer Settings"* into the search bar. At the top will be the setting for enabling developer mode.
3. Once developer mode is enabled, open the solution ```WeatherDataApp.sln``` in Visual Studio.
4. At the top, there will be a button to run the application.

## Code conventions
For the code, the standard .NET code conventions have been used. These conventions can be found [here](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#language-guidelines).