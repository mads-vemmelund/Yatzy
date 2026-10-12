# Yatzy

Et dansksproget Yatzy-spil til én spiller, bygget med C#, .NET 10 og ASP.NET Core Blazor. Spillet har fem terninger, 15 klassiske kategorier, automatisk pointberegning og en responsiv kasino-inspireret brugerflade. Spillogikken er adskilt fra webappen og dækket af xUnit-tests.

## Kør appen

```powershell
dotnet run --project src/Yatzy.Web/Yatzy.Web.csproj
```

## Kør tests

```powershell
dotnet test Yatzy.sln
```

Se [spec.md](spec.md) for de komplette spilleregler, krav og arkitektur.