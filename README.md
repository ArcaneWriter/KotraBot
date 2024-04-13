# Kotra Dice Roller 

C# Application that runs a discord bot to make dice rolls for the TTRPG Knights of the Round: Academy (Kotr:a)
Made with .NET 8.0

## Usage 
### Windows

To publish a build, go to the .cspojs folder and run on the cli

```ps1
dotnet publish -c release --runtime win-x64 --self-contained false

```
the .NET 8 sdk is needed to run this command, also the .NET 8 sdk or runtime is needed on the machine where you need to run the application.

after this you can run it as a normal executable.

### Linux

To publish a build, go to the .cspojs folder and run on the cli

```ps1
dotnet publish -c release --runtime linux-x64 --self-contained false

```

put the files published except the `.pdb` on the linux machine, where the .NET 8 sdk or runtime needs to be installed

then run it using the command 

```bash
dotnet run KotraBot.dll
```

## Important
For the applcation to work you need to put a secrets.json at root level of the installation of the app, which need to be as the template one in the `templates` folder 
