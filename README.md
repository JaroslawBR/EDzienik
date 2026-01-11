# EDzienik

Krótki przewodnik uruchomienia i wstrzyknięcia przykładowych danych do bazy.

## Uruchamianie aplikacji

1. Otwórz rozwiązanie `EDzienik.sln` w Visual Studio 

2. Domyślny adres (z `Properties/launchSettings.json`):
   - HTTPS: `https://localhost:7099`

> Jeśli aplikacja uruchomi się na innym porcie, użyj tego portu w adresach poniżej.

## Wstrzyknięcie przykładowych danych (seed)

Aby wygenerować przykładowe dane w bazie (role, konta, klasy, przedmioty, przypisania, uczniowie, oceny):

1. Otwórz przeglądarkę i przejdź pod adres (przykłady):
   - `https://localhost:7099/Seed`

2. Po odwiedzeniu strony otrzymasz tekstowy raport z wygenerowanymi kontami i informacją o haśle:
   - Domyślne hasło dla wygenerowanych kont: `Haslo123!`

## Konfiguracja bazy danych

Plik z connection stringiem: `appsettings.json` (klucz `ConnectionStrings:DefaultConnection`).
Domyślnie używany jest SQLite: `Data Source=edzienik.db;Foreign Keys=True`.

Jeśli chcesz zastosować migracje ręcznie:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

## Gdzie edytować dane generowane przez seed?

- Logika seedowania znajduje się w `Controllers/SeedController.cs`.
- Możesz tam zmienić: adresy e‑mail, nazwy klas, przedmiotów, liczbę uczniów, domyślne hasło itp.

## Co generuje `SeedController`?
- Role: `Admin`, `Teacher`, `Student`
- Administratorów: `admin@edziennik.pl`, `dyrektor@edziennik.pl`
- Nauczycieli (kilka kont testowych)
- Klasy: `1A`, `2B`, `3C`
- Przedmioty: np. Matematyka, Język Polski, Angielski, Historia, Fizyka, Informatyka
- Przypisania przedmiotów do nauczycieli i klas
- 24 uczniów (uczen1..uczen24@edziennik.pl)
- Losowe oceny przypisane uczniom
