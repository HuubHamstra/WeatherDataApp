﻿namespace WeatherDataApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;

        MainPage = new NavigationPage(new Views.MainPage());
    }
}