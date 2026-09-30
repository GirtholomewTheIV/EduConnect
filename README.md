# EduConnect

ASP.NET Core MVC (.NET 8) project for the EduConnect school communication platform.

Structure: Controllers, Models, ViewModels, Views, Data, Helpers.

## Run

1. Open `EduConnect.sln` in Visual Studio 2022.
2. Set **EduConnect** as the startup project and press F5.
3. Development URL: http://localhost:5268

The SQLite database (`educonnect.db`) is created automatically on first start and demo data is
seeded **in the Development environment only**. Demo accounts are created if they do not exist;
existing accounts are never overwritten.

If you change the models, run `ResetDatabase.bat` (or delete `educonnect.db*`) and start the app
again - `EnsureCreated` does not update the schema of an existing database.

## Demo accounts

| Role    | Email                     | Password   |
|---------|---------------------------|------------|
| Admin   | admin@educonnect.local    | Admin123!  |
| Teacher | teacher@educonnect.local  | Demo123!   |
| Parent  | parent@educonnect.local   | Demo123!   |
| Student | student@educonnect.local  | Demo123!   |

## Notes

- Report card PDFs are stored in `App_Data/report-cards` (not publicly reachable) and are served
  only through the authorised `ReportCards/Download` action.
