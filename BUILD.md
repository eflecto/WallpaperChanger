# 🔨 Инструкции по сборке WallpaperChanger

## Требования

### Для разработки:
- Windows 10/11
- Visual Studio 2022 (рекомендуется Community Edition или выше)
- .NET 8.0 SDK
- Workload "Разработка классических приложений .NET"

### Для компиляции через командную строку:
- .NET 8.0 SDK ([скачать](https://dotnet.microsoft.com/download/dotnet/8.0))

## Варианты сборки

### 1. Через Visual Studio (самый простой)

#### Открытие проекта:
```
1. Откройте Visual Studio 2022
2. File → Open → Project/Solution
3. Выберите WallpaperChanger.csproj
4. Дождитесь восстановления NuGet пакетов
```

#### Сборка Debug версии:
```
1. Build → Build Solution (или F6)
2. Результат: bin\Debug\net8.0-windows\WallpaperChanger.exe
```

#### Сборка Release версии:
```
1. В верхней панели измените "Debug" на "Release"
2. Build → Rebuild Solution
3. Результат: bin\Release\net8.0-windows\WallpaperChanger.exe
```

### 2. Через командную строку .NET CLI

#### Базовая сборка:
```bash
# Перейдите в папку с проектом
cd WallpaperChanger

# Debug версия
dotnet build

# Release версия (оптимизированная)
dotnet build -c Release
```

#### Запуск без сборки:
```bash
dotnet run
```

### 3. Создание standalone исполняемого файла

#### Single-file приложение (один EXE файл):
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Windows ARM64 (для Surface и других ARM устройств)
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true
```

Результат будет в: `bin\Release\net8.0-windows\win-x64\publish\`

#### Зависимый от framework (меньший размер):
```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

#### С ReadyToRun оптимизацией (быстрый запуск):
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true
```

### 4. Минимизация размера файла

#### Trim неиспользуемый код:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

⚠️ **Внимание**: Trimming может удалить необходимый код. Тестируйте тщательно!

#### Сжатие с помощью UPX:
```bash
# Скачайте UPX с https://upx.github.io/
# После сборки:
upx --best --lzma WallpaperChanger.exe
```

Это может уменьшить размер файла на 50-70%.

## Параметры MSBuild

Создайте файл `Directory.Build.props` в корневой папке для дополнительных настроек:

```xml
<Project>
  <PropertyGroup>
    <!-- Информация о версии -->
    <Version>1.0.0</Version>
    <Company>Your Company</Company>
    <Product>WallpaperChanger</Product>
    <Copyright>Copyright © 2026</Copyright>
    
    <!-- Оптимизация -->
    <DebugType>embedded</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <Optimize>true</Optimize>
    
    <!-- Single file -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    
    <!-- Дополнительные оптимизации -->
    <PublishReadyToRun>true</PublishReadyToRun>
    <PublishTrimmed>false</PublishTrimmed>
    <TieredCompilation>true</TieredCompilation>
    <TieredCompilationQuickJit>true</TieredCompilationQuickJit>
  </PropertyGroup>
</Project>
```

## Создание установщика

### С помощью Inno Setup:

1. Скачайте [Inno Setup](https://jrsoftware.org/isdl.php)
2. Создайте файл `installer.iss`:

```inno
[Setup]
AppName=WallpaperChanger
AppVersion=1.0.0
DefaultDirName={autopf}\WallpaperChanger
DefaultGroupName=WallpaperChanger
OutputDir=Output
OutputBaseFilename=WallpaperChanger-Setup
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\WallpaperChanger.exe"; DestDir: "{app}"

[Icons]
Name: "{group}\WallpaperChanger"; Filename: "{app}\WallpaperChanger.exe"
Name: "{autodesktop}\WallpaperChanger"; Filename: "{app}\WallpaperChanger.exe"

[Run]
Filename: "{app}\WallpaperChanger.exe"; Description: "Запустить WallpaperChanger"; Flags: postinstall nowait skipifsilent
```

3. Скомпилируйте установщик через Inno Setup Compiler

### С помощью WiX Toolset:

```bash
# Установите WiX
dotnet tool install --global wix

# Создайте MSI
wix build installer.wxs
```

## Подписание кода (Code Signing)

Для продакшн релиза рекомендуется подписать EXE файл:

```bash
# С помощью signtool из Windows SDK
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com WallpaperChanger.exe
```

## Continuous Integration (CI/CD)

### GitHub Actions пример:

Создайте `.github/workflows/build.yml`:

```yaml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release --no-restore
    
    - name: Publish
      run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: WallpaperChanger
        path: bin/Release/net8.0-windows/win-x64/publish/
```

## Решение проблем при сборке

### Ошибка: "SDK not found"
```bash
# Проверьте установку .NET SDK
dotnet --version

# Установите или обновите SDK
# Скачайте с https://dotnet.microsoft.com/download
```

### Ошибка: "Project file is incomplete"
```bash
# Восстановите проект
dotnet restore

# Очистите и пересоберите
dotnet clean
dotnet build
```

### Ошибка: "Windows Forms/WPF not found"
```bash
# Убедитесь, что установлен правильный SDK для Windows
# Должен быть net8.0-windows, не просто net8.0
```

### Ошибка при публикации single-file
```bash
# Убедитесь, что используете правильный runtime identifier
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Для списка всех доступных RID:
dotnet --info
```

## Оптимизация производительности

### Настройки Release сборки:

Добавьте в `.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <Optimize>true</Optimize>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <GenerateDocumentationFile>false</GenerateDocumentationFile>
</PropertyGroup>
```

### PGO (Profile-Guided Optimization):

```xml
<PropertyGroup>
  <TieredPGO>true</TieredPGO>
  <TieredCompilation>true</TieredCompilation>
</PropertyGroup>
```

## Тестирование сборки

После сборки обязательно проверьте:

1. ✅ Запуск от имени обычного пользователя
2. ✅ Запуск от имени администратора
3. ✅ Смена обоев работает
4. ✅ Смена экрана блокировки работает
5. ✅ Настройки сохраняются
6. ✅ Мониторинг процессов работает
7. ✅ Все кнопки и элементы UI работают
8. ✅ Нет утечек памяти (проверьте в Task Manager)

## Создание релиза

Чек-лист перед релизом:

- [ ] Обновлена версия в `.csproj`
- [ ] Обновлен `README.md`
- [ ] Создан CHANGELOG
- [ ] Протестирована сборка на чистой Windows 11
- [ ] Проверена работа на Windows 10
- [ ] Создан установщик (optional)
- [ ] Подписан код (для production)
- [ ] Создан tag в git
- [ ] Опубликован на GitHub Releases

## Дополнительные ресурсы

- [.NET Publishing Guide](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Windows Forms](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [Performance Tips](https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

---

**Успешной сборки! 🚀**
