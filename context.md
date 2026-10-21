# 🧠 Contexto Técnico Integral para Contribuidores y Agentes (`context.md`)

> **PROPÓSITO DE ESTE DOCUMENTO**: Este archivo es la fuente de verdad técnica interna del repositorio **MathTutor**. Está diseñado para que cualquier desarrollador humano o agente de código (Antigravity, OpenCode, Claude Code, etc.) comprenda **de un solo vistazo** toda la arquitectura, las decisiones de diseño, el flujo de datos y el detalle **archivo por archivo** de lo realizado en el proyecto, sin necesidad de explorar a ciegas el código fuente.
>
> **REGLA DE MANTENIMIENTO**: Este archivo forma parte del repositorio Git (como contexto interno de ingeniería, separado del `README.md` público) y **debe actualizarse obligatoriamente** cada vez que se agregue, modifique o elimine un archivo, modelo, servicio, vista o prueba.

---

## 1. Visión General y Arquitectura de la Solución

**MathTutor Accesible** es una plataforma educativa e inclusiva desarrollada en **C# (.NET 10 / WinUI 3)**, 100% conforme a **WCAG 2.2 (Niveles AA y AAA)** y **Diseño Universal para el Aprendizaje (DUA)**. Cubre un currículo completo de **45 estaciones de aprendizaje (16 objetivos pedagógicos)** desde **Jardín de Niños (3 años)**, **Primaria (1º a 6º)**, **Secundaria (1º a 3º)** y **Preparatoria / Bachillerato (1º a 3º)** hasta **Pensamiento Computacional y Programación Inclusiva (Módulos 1, 2 y 3)**.

### Diagrama de Arquitectura Modular

```
MathTutor.slnx
│
├── MathTutor.Core (net10.0 - Biblioteca de Dominio Pura, sin dependencias de UI)
│   ├── Models/
│   │   ├── Enums.cs               (DisabilityType, DisabilitySeverity, AcademicLevel, RegionalContext, HighContrastTheme, FontSizeLevel, AnswerResult)
│   │   ├── UserProfile.cs         (AccessibilityPreferences + UserProfile + propiedades culturales LATAM + ApplyRecommendedAccessibilityDefaults)
│   │   ├── Exercise.cs            (GamePlayType [53 mecánicas], ManipulableItem, ExerciseOption, Exercise)
│   │   ├── PedagogicalTopic.cs    (Modelo de lección: narrativa, concepto, principios, lista de ejercicios)
│   │   └── Badge.cs               (Badge y RealmStation para el Mapa del Reino Matemágico)
│   ├── Engine/
│   │   ├── AdaptiveEngine.cs      (Validación Edad vs. Nivel Escolar, adaptación de lenguaje, reducción cognitiva y estrellas/racha)
│   │   ├── CurriculumService.cs   (Catálogo de las 45 lecciones, filtrado por edad GetTopicsForAge, GetNextTopic y generación adaptativa)
│   │   └── ExerciseGenerator.cs   (Generador procedimental parametrizado para las 45 estaciones con distractores diagnósticos)
│   └── Services/
│       ├── ProfileStorageService.cs (Persistencia JSON en %LocalAppData%/MathTutor/user_profile.json)
│       └── RewardService.cs       (46 insignias y 45 estaciones conectadas al progreso del alumno)
│
├── MathTutor.WinUI (net10.0-windows10.0.19041.0 - Cliente Escritorio WinUI 3 Accesible)
│   ├── App.xaml / App.xaml.cs     (Diccionario de recursos dinámicos WCAG, estilos de foco, botones y controles accesibles)
│   ├── MainWindow.xaml / .cs      (Shell principal, barra global F1-F6/Esc, foco inicial, diálogo posmisión, regiones de notificación UIA y alto contraste del sistema)
│   ├── Navigation/NavigationService.cs (Frame de navegación y eventos de flujo entre vistas)
│   ├── Services/
│   │   ├── AccessibilityManager.cs (Singleton coordinador de perfil, canales F2 vs. lector, temas, escala tipográfica y anuncios)
│   │   ├── SpeechService.cs       (Puente nativo hacia NVDA Controller Client, JAWS COM API y notificaciones UIA)
│   │   └── AccessibleAudioCueService.cs (Sintetizador procedural PCM en memoria: notas, éxito, error y navegación)
│   └── Views/
│       ├── ProfilePage.xaml / .cs       (Caracterización del alumno con validación dinámica Edad/Discapacidad vs. Nivel Escolar)
│       ├── MenuPage.xaml / .cs          (Menú principal con ListView navegable por flechas y filtrado por edad)
│       ├── LessonPage.xaml / .cs        (Lección con Cosmo, secreto matemático y ejemplo guiado paso a paso)
│       ├── GamePage.xaml / .cs          (Motor interactivo de ejercicios, conteo, pistas y retroalimentación)
│       └── MapPage.xaml / .cs           (Mapa del Reino con estaciones e insignias navegables)
│
└── MathTutor.Tests (net10.0 - Suite Automatizada xUnit con 114 pruebas)
    ├── ExerciseGeneratorTests.cs         (59 pruebas de generación procedimental, aleatorización de opciones y no-colisión)
    ├── PedagogyAndAccessibilityTests.cs  (51 pruebas de cobertura curricular, comorbilidad, stepper de lección y formato de anuncios)
    ├── ProfileStorageTests.cs            (2 pruebas de serialización/deserialización JSON de perfiles con comorbilidad)
    └── RewardServiceTests.cs             (2 pruebas integrales de las 45 estaciones y desbloqueo de las 46 insignias)
```

---

### Estructura de Directorios

```text
MathTutor.slnx
├── src/
│   ├── MathTutor.Core/       Biblioteca de dominio, currículo, motor adaptativo y almacenamiento
│   └── MathTutor.WinUI/      Aplicación WinUI 3, navegación, servicios y vistas
├── tests/
│   └── MathTutor.Tests/      Suite xUnit de 114 pruebas
├── scripts/
│   └── publish-winui.ps1     Publicación y empaquetado de la release
├── artifacts/                Salidas de compilación y releases (ignorado por Git)
├── Directory.Build.props     Salidas centralizadas en artifacts/
├── MathTutor.sln / .slnx     Soluciones de Visual Studio / .NET
├── LEEME.txt                 Guía para familias, docentes y estudiantes
└── README.md, context.md, correcciones mat tutor.md, CLAUDE.md
```

## 2. Inventario Detallado Archivo por Archivo

### 2.1. Archivos Raíz del Repositorio

| Archivo | Propósito y Contenido Realizado |
|---|---|
| `MathTutor.slnx` | Archivo de solución XML moderno de .NET que agrupa los tres proyectos: `MathTutor.Core`, `MathTutor.WinUI` y `MathTutor.Tests`. |
| `Directory.Build.props` | Centraliza las salidas de todos los proyectos en `artifacts/` mediante `UseArtifactsOutput` y `ArtifactsPath`; evita `bin/` y `obj/` dentro de cada proyecto. |
| `scripts/publish-winui.ps1` | Publica WinUI 3 en un solo ejecutable para `win-x64`, `win-x86` o `win-arm64`, verifica `MathTutor.WinUI.pri`, integra `LEEME.txt`, arma la carpeta `MathTutor-v$Version` y genera `artifacts/releases/MathTutor-v$Version-Windows.zip`. |
| `artifacts/` | Directorio ignorado por Git con salidas centralizadas: `bin/`, `obj/`, `publish/`, `package/` y `releases/`. |
| `CLAUDE.md` | Directrices de ingeniería de software basadas en los principios de Andrej Karpathy: (1) Pensar antes de codificar, (2) Simplicidad primero (sin sobreingeniería), (3) Cambios quirúrgicos y (4) Ejecución orientada a objetivos verificables con pruebas. |
| `README.md` | Documentación pública del proyecto orientada a usuarios finales y educadores: características de accesibilidad WCAG 2.2, integración nativa con lectores de pantalla (NVDA, JAWS, Narrador), desglose curricular de los 16 objetivos / 45 estaciones, tabla de atajos de teclado (`F1`–`F6`, `Esc`, `1`–`3`, `Enter`) e instrucciones de compilación y ejecución. |
| `LEEME.txt` | Guía en texto plano para familias, docentes y estudiantes: instalación, primeros pasos, teclas, accesibilidad, privacidad, consejos y solución de problemas. Se integra en el paquete de release junto al ejecutable. |
| `correcciones mat tutor.md` | Registro histórico de correcciones, especificaciones de accesibilidad y reglas de negocio, con secciones por release hasta `v1.7.0`. |
| `context.md` | **(Este archivo)** Documento vivo de contexto técnico archivo por archivo para contribuidores y agentes de código. |
| `.gitignore` | Reglas de exclusión de Git para artefactos de compilación (`artifacts/`, `bin/`, `obj/`, `.vs/`, paquetes temporales). |
| `.engram/config.json` | Configuración del proyecto `mathtutor` para la memoria persistente Engram. |

---

### 2.2. Proyecto `MathTutor.Core` (Dominio, Currículo y Motor Adaptativo)

#### `src/MathTutor.Core/MathTutor.Core.csproj`
- **TargetFramework**: `net10.0`, `ImplicitUsings=enable`, `Nullable=enable`.
- **Dependencias**: Cero paquetes NuGet externos (100% BCL estándar de .NET para máxima portabilidad y velocidad de compilación).

#### `src/MathTutor.Core/Models/Enums.cs` (89 líneas)
Define las enumeraciones fundamentales del dominio:
1. **`DisabilityType`**:
   - `None = 0` (Neurotípico)
   - `Visual = 1` (Ceguera o Baja Visión)
   - `Hearing = 2` (Hipoacusia o Sordera)
   - `Motor = 3` (Dificultad motora fina / movilidad reducida)
   - `Intellectual = 4` (Discapacidad intelectual / cognitiva)
   - `Neurodivergent = 5` (TDAH / Espectro Autista)
2. **`DisabilitySeverity`**: `Mild = 1` (Leve), `Moderate = 2` (Moderada), `Severe = 3` (Severa).
3. **`AcademicLevel`**:
   - `Kindergarten = 1` (3 a 5 años)
   - `EarlyElementary = 2` (1º y 2º Primaria — 6 a 8 años)
   - `MiddleElementary = 3` (3º y 4º Primaria — 9 a 10 años)
   - `UpperElementary = 4` (5º y 6º Primaria — 11 a 12 años)
   - `ProgrammingFoundations = 5` (Fundamentos de Programación y Algoritmos)
   - `SecondaryFirstGrade = 6` (1º Secundaria — 12 a 13 años)
   - `SecondarySecondGrade = 7` (2º Secundaria — 13 a 14 años)
   - `SecondaryThirdGrade = 8` (3º Secundaria — 14 a 15 años)
   - `HighSchoolFirstGrade = 9` (1º Preparatoria — 15 a 16 años)
   - `HighSchoolSecondGrade = 10` (2º Preparatoria — 16 a 17 años)
   - `HighSchoolThirdGrade = 11` (3º Preparatoria — 17 a 18 años)
4. **`RegionalContext`**: `Mexico = 1`, `Colombia = 2`, `Argentina = 3`, `Peru = 4`, `Chile = 5`.
5. **`HighContrastTheme`**: `DefaultPlayful = 0`, `HighContrastDark = 1` (Negro/Amarillo > 15:1), `HighContrastLight = 2` (Blanco/Negro > 21:1), `LowSensory = 3` (Pastel suave TEA/TDAH).
6. **`FontSizeLevel`**: `Normal = 0` (18pt), `Large = 1` (23-24pt), `ExtraLarge = 2` (28-30pt), `Huge = 3` (34-36pt).
7. **`AnswerResult`**: `None = 0`, `Correct = 1`, `NeedsGuidance = 2`, `Assisted = 3`.

#### `src/MathTutor.Core/Models/UserProfile.cs` (253 líneas)
Contiene dos clases clave:
1. **`AccessibilityPreferences`**:
   - Propiedades persistibles: `EnableVoiceSynthesis`, `ScreenReaderAnnouncements`, `Theme`, `FontSize`, `SpeechRate`, `SpeechVolume`, `PlayAudioCues`, `SimplifyLanguage`, `ReduceOptionsCount`, `SimplifiedKeyboardNavigation`, `UnlimitedTime`.
2. **`UserProfile`**:
   - Datos del alumno: `Id`, `Name`, `Age`, `Disability`, `Severity`, `CurrentLevel`, `Region`, `Preferences`.
   - Propiedades calculadas de localización cultural latinoamericana:
     - `CurrencyName`: `"soles"` para Perú; `"pesos"` para México, Colombia, Argentina y Chile.
     - `CurrencyFractionName`: `"céntimos"` para Perú; `"centavos"` para el resto.
     - `CurrencySymbol`: `"S/"` para Perú; `"$"` para el resto.
     - `StoreName`: `"tiendita"` (México), `"tienda de barrio"` (Colombia), `"kiosco"` (Argentina), `"bodega"` (Perú), `"almacén"` (Chile).
   - Gamificación: `StarsTotal`, `PointsTotal`, `CurrentStreak`, `CompletedLessonIds`, `CompletedExerciseIds`.
   - Método **`ApplyRecommendedAccessibilityDefaults()`**: Configura automáticamente el tema de contraste, tamaño de fuente, simplificación de lenguaje y reducción de opciones según `Disability`, `Severity` y `Age`.

#### `src/MathTutor.Core/Models/Exercise.cs` (123 líneas)
- **`GamePlayType` (Enum con 53 valores)**: Identifica la mecánica exacta de cada reto (desde `InteractiveCounting`, `OrderSequence`, `Comparison` hasta `CalculusDerivativeRate`, `CalculusIntegralArea`, `ProgrammingVariablesState`, `ProgrammingConditionalsIfElse`, `ProgrammingBooleanLogic`, `ProgrammingLoopsForWhile`, `ProgrammingArraysZeroIndex`, `ProgrammingFunctionsReturn`, `ProgrammingAlgorithmsSearch`, `ProgrammingGameCoordinates2D`, `ProgrammingAssistiveAIData`).
- **`ManipulableItem`**: Objeto interactivo contable o explorable en pantalla (`Id`, `Name`, `EmojiSymbol`, `AccessibleDescription`, `OrderIndex`, `IsCounted`, `MusicalPitchHz`).
- **`ExerciseOption`**: Opción de respuesta (`Id`, `Label`, `Value`, `IsCorrect`, `SpeechFeedback` con diagnóstico formativo del error o acierto, y `AccessibleKeyShortcut` `"1"`, `"2"`, `"3"`).
- **`Exercise`**: Reto completo con instrucciones duales (`StandardInstruction` y `SimplifiedInstruction`), descripción de escena (`ScreenReaderDescription`, ya no se anuncia para no revelar el resultado), colecciones `ItemsGroupA` e `ItemsGroupB`, lista `Options`, pistas escalonadas (`FirstHint`, `SecondHintDetailed`), `ExplanationOfSuccess`, `ExpectedResult`, `TargetAgeMin/Max` y `Level`.

#### `src/MathTutor.Core/Models/PedagogicalTopic.cs` (30 líneas)
- Representa una misión o lección completa: `Id`, `Title`, `Level`, `CharacterName` (`"Cosmo el Búho Sabio"`), `NarrativeIntro`, `NarrativeIntroSimplified`, `CoreConceptExplanation`, `CoreConceptSimplified`, `PrinciplesAddressed` (`List<string>`), `Exercises` (`List<Exercise>`) y `CongratulationMessage`.

#### `src/MathTutor.Core/Models/Badge.cs` (34 líneas)
- **`Badge`**: Trofeo desbloqueable (`Id`, `Title`, `Description`, `IconEmoji`, `AssociatedTopicId`, `RequiredStars`, `IsUnlocked`, `UnlockedAt`).
- **`RealmStation`**: Estación del sendero en el Mapa del Reino (`Id`, `Name`, `Description`, `LandmarkEmoji`, `TopicId`, `OrderIndex`, `IsCompleted`, `IsCurrentStation`, `StarsEarned`).

#### `src/MathTutor.Core/Engine/AdaptiveEngine.cs` (243 líneas)
Motor de adaptación pedagógica y reglas de negocio por edad y condición:
- **`GetAllowedLevelsForAge(int age, DisabilityType disability = DisabilityType.None)`** (Tarea 6 y Tarea 12): Devuelve la lista de niveles académicos válidos para la edad y condición del alumno:
  - `3 a 5 años`: `Kindergarten`
  - `6 a 11 años`: `EarlyElementary`, `MiddleElementary`, `UpperElementary`
  - `12 años`: `EarlyElementary`, `MiddleElementary`, `UpperElementary`, `SecondaryFirstGrade`
  - `13 a 15 años`: `SecondaryFirstGrade`, `SecondarySecondGrade`, `SecondaryThirdGrade`
  - `16 años o más`: Todos los niveles, incluyendo Preparatoria (`HighSchoolFirstGrade`–`HighSchoolThirdGrade`) y `ProgrammingFoundations`.
  - Si `disability == DisabilityType.Intellectual` entre los 6 y 15 años, incluye siempre los niveles base de refuerzo (`Kindergarten`, `EarlyElementary`, `MiddleElementary`, `UpperElementary`).
- **`GetRecommendedLevelForAge(int age)`** (Tarea 6): Sugiere el nivel académico inicial óptimo dentro de los niveles permitidos para la edad seleccionada.
- **`GetAdaptedInstruction(Exercise, UserProfile)`**: Devuelve `SimplifiedInstruction` si `SimplifyLanguage` es `true` o si `Disability == DisabilityType.Intellectual`; de lo contrario devuelve `StandardInstruction`.
- **`GetAdaptedTopicIntro(PedagogicalTopic, UserProfile)`** y **`GetAdaptedConceptExplanation(PedagogicalTopic, UserProfile)`**: Conmutan entre la versión estándar y la versión simplificada de la narrativa y del concepto matemático.
- **`GetAdaptedOptions(Exercise, UserProfile)`**: Si `ReduceOptionsCount` o `Disability == Intellectual` está activo, filtra la lista de opciones dejando **1 respuesta correcta y 1 distractor** en su orden relativo original y reasigna los atajos `"1"` y `"2"`.
- **`RecordAttempt(UserProfile, Exercise, bool isCorrect, int attemptCount)`**:
  - Si `isCorrect == true`: incrementa `CurrentStreak`, otorga **3 estrellas** (primer intento), **2 estrellas** (segundo intento) o **1 estrella** (3+ intentos, premiando siempre la perseverancia), suma `starsEarned * 10` a `PointsTotal` y registra el `exercise.Id` en `CompletedExerciseIds`.
  - Si `isCorrect == false`: reduce suavemente `CurrentStreak` en 1 (sin castigo punitivo a cero).

#### `src/MathTutor.Core/Engine/CurriculumService.cs` (2,799 líneas)
Contiene el catálogo pedagógico completo de las **45 misiones** (`GetAllTopics()`), el filtrado adaptativo por edad/condición y el orquestador de ejercicios:
- **`GetTopicsForAge(int age, DisabilityType disability = DisabilityType.None)`** (Tarea 6 y Tarea 12): Filtra `GetAllTopics()` devolviendo únicamente las misiones cuyos niveles pertenecen a `AdaptiveEngine.GetAllowedLevelsForAge(age, disability)`.
- **`GetNextTopic(string currentTopicId, UserProfile? profile = null)`** (Tarea 7 y Tarea 10): Obtiene la siguiente misión secuencial o pendiente dentro del rango de edad del alumno tras completar `currentTopicId`, devolviendo `null` cuando el alumno ha completado todas las misiones de su etapa.
- **`GenerateAdaptiveExercises(PedagogicalTopic topic, UserProfile profile, int count = 3)`**:
  - Si el alumno tiene `DisabilityType.Intellectual` o es un tema monetario con región distinta de México, genera los `count` ejercicios procedimentalmente con `ExerciseGenerator` para garantizar el rango numérico reducido y la moneda local (`soles`, `pesos`, `tiendita`, `kiosco`, `bodega`, `almacén`).
  - En caso estándar, conserva como **Reto 1** el ejercicio curado de la lección (`templateTopic.Exercises[0]`, que conecta directamente con el Ejemplo Guiado Paso a Paso) y genera procedimentalmente los retos restantes con `_generator.GenerateExercisesForTopic(topic.Id, profile, remaining)`.

#### `src/MathTutor.Core/Engine/ExerciseGenerator.cs` (2,705 líneas)
Generador procedimental parametrizado que produce ejercicios infinitos y libres de colisiones para las **45 estaciones**:
- **`GetTargetRange(UserProfile)`**: Ajusta el rango numérico a `(1, 3)` para discapacidad intelectual severa, `(1, 5)` para moderada/leve o Jardín de Niños, y `(1, 10)` para primaria inicial.
- **46 métodos públicos de generación** (`GenerateCountingExercise`, `GenerateStableOrderExercise`, `GenerateAdditionCombineExercise`, ..., `GenerateProgrammingVariablesExercise`, `GenerateProgrammingConditionalsExercise`, `GenerateProgrammingBooleanLogicExercise`, `GenerateProgrammingLoopsExercise`, `GenerateProgrammingArraysExercise`, `GenerateProgrammingFunctionsExercise`, `GenerateProgrammingAlgorithmsSearchExercise`, `GenerateProgrammingGameCoordinatesExercise`, `GenerateProgrammingAssistiveAIExercise`) conectados en el despachador `GenerateExercisesForTopic(string topicId, UserProfile profile, int count = 3, Random? rng = null)`.
- **`BuildOptions(...)`**: Construye las opciones de respuesta garantizando valores únicos, barajado determinista/aleatorio sin sesgo de posición y **retroalimentación diagnóstica específica (`SpeechFeedback`)** en cada distractor (indicando si el número elegido es mayor o menor que el resultado correcto y qué paso revisar).

#### `src/MathTutor.Core/Services/ProfileStorageService.cs` (81 líneas)
- Gestiona la persistencia local del perfil en `%LocalAppData%\MathTutor\user_profile.json` usando `System.Text.Json` con `JsonStringEnumConverter`.
- Permite inyectar una ruta personalizada en el constructor (`ProfileStorageService(string? customPath = null)`) para pruebas unitarias aisladas en archivos temporales.
- Métodos: `Save(UserProfile)`, `Load()`, `Exists()`.

#### `src/MathTutor.Core/Services/RewardService.cs` (889 líneas)
- **`GetAllBadges(UserProfile)`**: Define y evalúa el estado de desbloqueo (`IsUnlocked`) de las **46 insignias** (45 insignias asociadas 1-a-1 a cada `TopicId` + 1 insignia especial `"badge_star_collector"` al acumular $\ge 15$ estrellas).
- **`GetRealmStations(UserProfile)`**: Devuelve las **45 estaciones** del Mapa del Reino (`station_1` a `station_45`), marcando `IsCompleted = true` para las lecciones en `CompletedLessonIds` y asignando `IsCurrentStation = true` a la primera estación pendiente del sendero.

---

### 2.3. Proyecto `MathTutor.WinUI` (Interfaz WinUI 3 y Servicios Nativos)

#### `src/MathTutor.WinUI/MathTutor.WinUI.csproj`
- **TargetFramework**: `net10.0-windows10.0.19041.0`, `TargetPlatformMinVersion=10.0.17763.0`, `UseWinUI=true`, `Nullable=enable`, `ImplicitUsings=enable`.
- Aplicación de escritorio sin empaquetar (`WindowsPackageType=None`, `EnableMsixTooling=false`) con `WindowsAppSDKSelfContained=true`.
- Referencia de proyecto únicamente a `MathTutor.Core.csproj`; icono `Assets/AppIcon.ico` y manifiesto `app.manifest`.
- El target `CopyPriToPublishDirectory` copia `MathTutor.WinUI.pri` al directorio de publicación; sin él, el ejecutable publicado falla al iniciar.
- La publicación de release usa un solo ejecutable (`PublishSingleFile`, extracción de bibliotecas nativas y compresión) desde `scripts/publish-winui.ps1`; el paquete final conserva únicamente el ejecutable, `.pri` y `LEEME.txt`.

#### `src/MathTutor.WinUI/App.xaml` y `App.xaml.cs`
Define los recursos globales dinámicos y estilos accesibles WCAG 2.2:
- **Brushes dinámicos**: `AppBackgroundBrush`, `AppSurfaceBrush`, `AppCardBrush`, `AppTextPrimaryBrush`, `AppTextSecondaryBrush`, `AppAccentBrush`, `AppFocusBorderBrush`, `AppBorderBrush`, `AppSuccessBrush`, `AppOnAccentBrush`, `AppOnSuccessBrush` y `AppErrorBrush`.
- **Escalas tipográficas dinámicas**: `AppBaseFontSize` (18), `AppSubHeaderFontSize` (22), `AppHeaderFontSize` (28) y `AppTitleFontSize` (36).
- **Estilos**: `AccessibleActionButton`, `AccessibleGameOptionButton`, `AccessibleCard` y `AccessibleReadOnlyTextBox` (texto de solo lectura navegable con el cursor del lector de pantalla).
- `App.xaml.cs` crea y activa `MainWindow`; expone `App.Navigation` para que las páginas soliciten navegación.

#### `src/MathTutor.WinUI/MainWindow.xaml` y `MainWindow.xaml.cs`
Ventana principal y shell de navegación:
- **Barra superior**: `CommandBar` con Menú (`Esc`), Mapa (`F6`), Perfil (`F5`), Contraste (`F3`), Letra (`F4`), Sonidos (`F2`), Repetir (`F1`) y contador de estrellas (`TxtTopStars`). El botón de desbordamiento se renombra a “Más opciones de navegación y accesibilidad”.
- **Host central**: `Frame` (`RootFrame`) gestionado por `NavigationService` para las páginas de perfil, menú, lección, minijuego y mapa.
- **Atajos globales**: `KeyDown` con `handledEventsToo` para `F1`–`F6`, `Esc` y respuestas `1`–`4` cuando el minijuego está activo.
- **Anuncios**: `RaiseScreenReaderAnnouncement` usa `RaiseNotificationEvent` de UI Automation (sin `LiveSetting` duplicado) y actualiza `TxtLiveAnnouncement` como texto de apoyo.
- **Alto contraste del sistema**: escucha `AccessibilitySettings.HighContrastChanged`, aplica el tema correspondiente y conserva la preferencia del usuario al desactivarlo.
- **Diálogo de misión completada**: calcula la siguiente misión, muestra el diálogo modal accesible y navega según la elección.
- **Tamaño inicial**: ventana de 1100x820 lógicos, mínimo de 800x600, escalada por DPI y limitada al área de trabajo.

#### `src/MathTutor.WinUI/Navigation/NavigationService.cs`
Centraliza el `Frame` y los eventos del flujo: `TopicSelected`, `MapRequested`, `MenuRequested`, `ProfileCompleted`, `ProfileCancelled`, `StartGameRequested`, `BackToMenuRequested`, `BackToLessonRequested` y `MissionCompleted`.

#### `src/MathTutor.WinUI/Services/SpeechService.cs`
Puente nativo con el lector de pantalla del sistema (sin SAPI5):
- `UiAutomationAnnouncer` conecta los anuncios con la ventana principal.
- Cascada de salida: NVDA Controller Client (`nvdaControllerClient64.dll`), JAWS COM (`FreedomScientific.JawsApi`) y notificaciones de Microsoft UI Automation.
- Separa el audio interno del juego de la integración con el lector; `F2` solo conmuta el audio interno.

#### `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`
Sintetizador procedural PCM en memoria, sin archivos `.wav` externos:
- `PlayNote`, `PlaySuccessChord`, `PlayErrorChord`, `PlayHintTone` y `PlayNavigationClick`.
- `MusicEnabled` controla efectos y pistas sin afectar los anuncios del lector de pantalla.

#### `src/MathTutor.WinUI/Services/AccessibilityManager.cs`
Singleton central (`AccessibilityManager.Instance`) que coordina:
- `CurrentProfile`, `Speech`, `Audio` y `Storage`.
- Eventos `ThemeChanged`, `FontSizeChanged` y `AnnouncementMade`.
- `ApplyProfile`, `SaveCurrentProfile`, `LoadSavedProfile`, `Announce`, `AnnounceAssertive`, `CycleTheme`, `CycleFontSize` y `ToggleSpeech`.
- `ApplySystemHighContrast` mantiene el tema del sistema activo sin sobrescribir la preferencia guardada del usuario.
- Temas `DefaultPlayful`, `HighContrastDark`, `HighContrastLight` y `LowSensory`, con recursos de foco, borde, éxito y texto sobre acento.

#### `src/MathTutor.WinUI/Views/ProfilePage.xaml` y `ProfilePage.xaml.cs`
Caracterización inicial del alumno (`F5`): nombre, edad, nivel escolar filtrado por edad, país/moneda, discapacidades múltiples, severidad, audio interno, pistas sonoras, lenguaje simplificado, tema y tamaño de letra. Valida edad/nivel, restaura el perfil guardado al cancelar y usa encabezados y nombres accesibles.

#### `src/MathTutor.WinUI/Views/MenuPage.xaml` y `MenuPage.xaml.cs`
Menú principal de misiones (`Esc`): saludo, resumen del perfil, estrellas, racha y `ListView` de misiones filtradas por edad. Los elementos tienen nombres como “Misión recomendada: …” y los botones internos “Comenzar la misión: …” o “Volver a jugar: …”. Soporta flechas, `Enter` y `Espacio`.

#### `src/MathTutor.WinUI/Views/LessonPage.xaml` y `LessonPage.xaml.cs`
Lección guiada en tres pasos: historia de Cosmo, secreto matemático y ejemplo resuelto. Incluye navegación anterior/siguiente, lectura de la explicación, inicio del minijuego y conservación del paso al volver desde el juego.

#### `src/MathTutor.WinUI/Views/GamePage.xaml` y `GamePage.xaml.cs`
Motor interactivo: progreso, instrucción adaptada, repaso del secreto matemático en la misma pantalla, objetos manipulables, pistas diagnósticas, opciones `1`–`3`, banners de error/éxito y barra de progreso. Evita doble conteo, bloquea respuestas tras acertar y anuncia retroalimentación y resultados; no narra la descripción de escena que contenía el resultado esperado.

#### `src/MathTutor.WinUI/Views/MapPage.xaml` y `MapPage.xaml.cs`
Mapa del Reino (`F6`): estaciones e insignias filtradas por edad y condición. Las estaciones usan nombres como “Misión recomendada: …” y botones “Explorar la estación: …”; la sala de trofeos usa nombres “Trofeo desbloqueado/bloqueado: …”. Conserva la selección al cambiar de tema.

#### `src/MathTutor.WinUI/Assets`, `app.manifest`, `Properties/launchSettings.json` y `.gitignore`
- `Assets/AppIcon.ico` es el único recurso de imagen requerido por la aplicación de escritorio.
- `app.manifest` declara la aplicación como consciente de DPI y ejecutable de escritorio.
- `launchSettings.json` facilita la ejecución desde Visual Studio.
- `.gitignore` local excluye `bin/`, `obj/` y artefactos de empaquetado.

> **Nota de arquitectura:** el proyecto WPF (`MathTutor.WpfApp`) fue retirado después de consolidar la paridad funcional en WinUI 3. El historial de Git conserva su implementación y los registros de releases anteriores.

### 2.4. Proyecto `MathTutor.Tests` (Suite de 114 Pruebas Unitarias Automatizadas)

| Archivo de Pruebas | Pruebas | Qué Verifica |
|---|---|---|
| `ExerciseGeneratorTests.cs` (1,053 líneas) | **59** | Verifica individualmente los métodos generadores de los 16 objetivos (desde conteo de Kinder hasta Cálculo e IA Asistiva), comprobando en bucles de 20 iteraciones con semilla determinista que `ExpectedResult` sea matemáticamente exacto, que exista exactamente una opción correcta, que no haya opciones duplicadas (`Distinct`), que los rangos se reduzcan para discapacidad intelectual y que la moneda/vocabulario cambie según `RegionalContext` (Perú `S/` vs. México `$`). |
| `PedagogyAndAccessibilityTests.cs` (740 líneas) | **51** | Verifica que las 45 misiones existan en `CurriculumService`, cubran sus principios matemáticos esperados, posean instrucciones estándar y simplificadas, pistas escalonadas y retroalimentación diagnóstica, además de probar `AdaptiveEngine.GetAllowedLevelsForAge` (incluyendo 12 años en 1º de Secundaria y refuerzo base en Discapacidad Intelectual), `GetRecommendedLevelForAge`, `CurriculumService.GetTopicsForAge`, `GetNextTopic` (incluyendo retorno `null` al completar toda una etapa) y `UserProfile.ApplyRecommendedAccessibilityDefaults()`. |
| `ProfileStorageTests.cs` (86 líneas) | **2** | Verifica que `ProfileStorageService` serialice y deserialice fielmente todos los campos de `UserProfile` y `AccessibilityPreferences` en disco usando archivos temporales aislados. |
| `RewardServiceTests.cs` (154 líneas) | **2** | Verifica el orden, `Id` y `TopicId` de las **45 estaciones** en `GetRealmStations` y la lógica de bloqueo/desbloqueo de las **46 insignias** en `GetAllBadges`. |

---

## 3. Mapa Curricular Completo (45 Estaciones / 16 Objetivos)

| Estación | `TopicId` | Nivel (`AcademicLevel`) | `GamePlayType` Principal | Tema / Objetivo Pedagógico |
|---|---|---|---|---|
| 1 | `kinder_counting_01` | `Kindergarten` | `InteractiveCounting`, `OrderSequence`, `Comparison` | Obj. 1: Los 5 Principios del Conteo (Gelman y Gallistel) |
| 2 | `elem_addition_01` | `EarlyElementary` | `AdditionCombine`, `CommutativePropertyDemo`, `AssociativePropertyDemo`, `ZeroNeutralProperty` | Obj. 2: Adición y sus Propiedades (1º y 2º Primaria) |
| 3 | `elem_subtraction_01` | `EarlyElementary` | `SubtractionTakeAway`, `InverseRelation` | Obj. 2: Sustracción y Relación Inversa |
| 4 | `elem_multiplication_01` | `EarlyElementary` | `MultiplicationArray` | Obj. 3: Multiplicación con Arreglos Rectangulares |
| 5 | `elem_division_01` | `MiddleElementary` | `DivisionFairSharing` | Obj. 3: División como Reparto Equitativo (3º Primaria) |
| 6 | `elem_fractions_01` | `MiddleElementary` | `FractionParts`, `FractionEquivalence` | Obj. 4: Fracciones Propias y Equivalentes (4º Primaria) |
| 7 | `elem_distributive_01` | `MiddleElementary` | `MultiplicationDistributive` | Obj. 4: Propiedad Distributiva por Descomposición |
| 8 | `elem_mixed_fractions_01` | `UpperElementary` | `MixedFractions` | Obj. 5: Fracciones Impropias y Números Mixtos (5º Primaria) |
| 9 | `elem_decimals_01` | `UpperElementary` | `DecimalsMoney` | Obj. 5: Números Decimales y Moneda Regional |
| 10 | `elem_powers_01` | `UpperElementary` | `PowersAndExponentGrowth` | Obj. 5: Potencias Iniciales (Cuadrados y Cubos) |
| 11 | `elem_proportion_01` | `UpperElementary` | `DirectProportion` | Obj. 6: Proporcionalidad Directa y Regla de Tres (6º Primaria) |
| 12 | `elem_algebra_balance_01` | `UpperElementary` | `AlgebraicBalance` | Obj. 6: Pre-Álgebra y Balanza de Ecuaciones ($x + a = b$) |
| 13 | `elem_square_root_01` | `UpperElementary` | `SquareRootPerfect` | Obj. 6: Raíz Cuadrada Exacta de Cuadrados Perfectos |
| 14 | `elem_cartesian_coordinates_01` | `UpperElementary` | `CartesianCoordinates` | Obj. 6: Plano Cartesiano y Pares Ordenados $(X, Y)$ |
| 15 | `sec_signed_numbers_01` | `SecondaryFirstGrade` | `SignedNumbersAddition` | Obj. 7: Números Enteros con Signo, Saldos y Deudas (1º Sec.) |
| 16 | `sec_order_operations_01` | `SecondaryFirstGrade` | `OrderOfOperationsPEMDAS` | Obj. 7: Jerarquía de Operaciones y Paréntesis (PEMDAS) |
| 17 | `sec_linear_equations_01` | `SecondaryFirstGrade` | `LinearEquationAXB` | Obj. 7: Ecuaciones Lineales de Primer Grado ($ax + b = c$) |
| 18 | `sec_percentages_01` | `SecondaryFirstGrade` | `PercentageCalculation` | Obj. 7: Porcentajes, Descuentos Comerciales e IVA |
| 19 | `sec_exponent_laws_01` | `SecondarySecondGrade` | `ExponentLaws` | Obj. 8: Leyes de los Exponentes ($a^m \cdot a^n$, $a^m / a^n$) (2º Sec.) |
| 20 | `sec_polynomials_like_terms_01` | `SecondarySecondGrade` | `PolynomialLikeTerms` | Obj. 8: Polinomios y Reducción de Términos Semejantes |
| 21 | `sec_linear_systems_2x2_01` | `SecondarySecondGrade` | `LinearSystem2x2` | Obj. 8: Sistemas de Ecuaciones Lineales $2 \times 2$ |
| 22 | `sec_pythagorean_theorem_01` | `SecondarySecondGrade` | `PythagoreanTheorem` | Obj. 8: Teorema de Pitágoras ($a^2 + b^2 = c^2$) |
| 23 | `sec_quadratic_equations_01` | `SecondaryThirdGrade` | `QuadraticEquations` | Obj. 9: Ecuaciones Cuadráticas ($ax^2 + c = d$) (3º Sec.) |
| 24 | `sec_thales_similarity_01` | `SecondaryThirdGrade` | `TriangleSimilarityThales` | Obj. 9: Semejanza de Triángulos y Teorema de Tales |
| 25 | `sec_trigonometry_ratios_01` | `SecondaryThirdGrade` | `TrigonometricRatios` | Obj. 9: Razones Trigonométricas ($\text{sen}, \cos, \tan$) |
| 26 | `sec_quadratic_functions_01` | `SecondaryThirdGrade` | `QuadraticFunctionCurve` | Obj. 9: Funciones Cuadráticas y Parábolas ($y = ax^2 + c$) |
| 27 | `prep_analytic_distance_01` | `HighSchoolFirstGrade` | `AnalyticDistance2D` | Obj. 10: Geometría Analítica — Distancia entre Dos Puntos (1º Prep.) |
| 28 | `prep_line_slope_01` | `HighSchoolFirstGrade` | `LineSlopeM` | Obj. 10: Pendiente de la Recta ($m = \Delta y / \Delta x$) |
| 29 | `prep_line_equation_ymxb_01` | `HighSchoolFirstGrade` | `LinearEquationYMXB` | Obj. 10: Ecuación Pendiente-Ordenada al Origen ($y = mx + b$) |
| 30 | `prep_circle_conic_01` | `HighSchoolSecondGrade` | `ConicCircleRadius` | Obj. 11: Secciones Cónicas — Circunferencia ($x^2 + y^2 = r^2$) (2º Prep.) |
| 31 | `prep_sine_wave_01` | `HighSchoolSecondGrade` | `SineWaveContinuous` | Obj. 11: Funciones Periódicas y Ondas Senoidales ($y = A\text{sen}(x)$) |
| 32 | `prep_logarithm_binary_01` | `HighSchoolSecondGrade` | `LogarithmBinary` | Obj. 11: Funciones Exponenciales y Logaritmo Base 2 ($\log_2(n)$) |
| 33 | `prep_calculus_derivative_01` | `HighSchoolThirdGrade` | `CalculusDerivativeRate` | Obj. 12: Cálculo Diferencial — Derivada Instantánea $f'(t)$ (3º Prep.) |
| 34 | `prep_calculus_integral_01` | `HighSchoolThirdGrade` | `CalculusIntegralArea` | Obj. 12: Cálculo Integral — Área bajo la Curva $\int_a^b f(t)\,dt$ |
| 35 | `prep_probability_stats_01` | `HighSchoolThirdGrade` | `ProbabilityAndStats` | Obj. 12: Probabilidad y Estadística Descriptiva ($\bar{x}$) |
| 36 | `prog_seed_01` | `ProgrammingFoundations` | `PatternAndLoopSeed` | Obj. 13: Semilla de Programación — Suma Repetida y Bucles |
| 37 | `prog_variables_state_01` | `ProgrammingFoundations` | `ProgrammingVariablesState` | Obj. 14 (Prog. Mód. 1): Variables, Asignación y Estado en Memoria |
| 38 | `prog_conditionals_ifelse_01` | `ProgrammingFoundations` | `ProgrammingConditionalsIfElse` | Obj. 14 (Prog. Mód. 1): Condicionales `SI / SI NO` (`if / else`) |
| 39 | `prog_boolean_logic_01` | `ProgrammingFoundations` | `ProgrammingBooleanLogic` | Obj. 14 (Prog. Mód. 1): Lógica Booleana (`AND`, `OR`, `NOT`) |
| 40 | `prog_loops_for_while_01` | `ProgrammingFoundations` | `ProgrammingLoopsForWhile` | Obj. 15 (Prog. Mód. 2): Ciclos `PARA` (`for`) y Acumuladores |
| 41 | `prog_arrays_lists_01` | `ProgrammingFoundations` | `ProgrammingArraysZeroIndex` | Obj. 15 (Prog. Mód. 2): Arreglos (`Arrays`) e Índice Base Cero `[0..n-1]` |
| 42 | `prog_functions_params_01` | `ProgrammingFoundations` | `ProgrammingFunctionsReturn` | Obj. 15 (Prog. Mód. 2): Funciones Modulares, Parámetros y `return` |
| 43 | `prog_algorithms_search_01` | `ProgrammingFoundations` | `ProgrammingAlgorithmsSearch` | Obj. 16 (Prog. Mód. 3): Algoritmos de Búsqueda del Máximo y Mínimo |
| 44 | `prog_game_coordinates_01` | `ProgrammingFoundations` | `ProgrammingGameCoordinates2D` | Obj. 16 (Prog. Mód. 3): Física y Coordenadas 2D en Videojuegos ($x = x_0 + v\cdot t$) |
| 45 | `prog_assistive_ai_data_01` | `ProgrammingFoundations` | `ProgrammingAssistiveAIData` | Obj. 16 (Prog. Mód. 3): Inteligencia Artificial Asistiva y Sensores |

---

## 4. Registro de las 12 Correcciones Implementadas (`correcciones mat tutor.md` — Fase 1 y Fase 2)

> **Nota histórica:** las fases 1 a 4 describen la implementación original en WPF, retirada en `v1.7.0`. La interfaz vigente y sus equivalentes están en `src/MathTutor.WinUI`, descritos en las secciones 2.3 y 2.4.

Todas las mejoras especificadas en [`correcciones mat tutor.md`](file:///C:/Users/artik/Documents/antigravity/math%20tutor/correcciones%20mat%20tutor.md) se encuentran implementadas y verificadas:

### Fase 1 (Tareas 1 a 7)
1. ✅ **Tarea 1 (Foco Inicial Automático y Anuncio de Entrada al Iniciar)**:
   - Implementado posicionamiento automático de foco (`Focus()` + `Keyboard.Focus(...)`) en `MainWindow.xaml.cs`, `ProfileSetupView.xaml.cs`, `CurriculumMenuView.xaml.cs`, `RealmMapView.xaml.cs`, `LessonView.xaml.cs` y `MiniGameView.xaml.cs`.
2. ✅ **Tarea 2 (Navegación Direccional con Flechas en la Lista de Misiones y Mapa del Reino)**:
   - Sustituidos los `ItemsControl` por `ListBox` accesibles (`ItemsTopics` e `ItemsStations`) con `KeyboardNavigation.DirectionalNavigation="Cycle"`, `IsSynchronizedWithCurrentItem="True"`, borde de foco de 4px y activación directa con `Enter` o `Espacio`.
3. ✅ **Tarea 3 (Estandarización de Etiquetas `AutomationProperties` sin Emojis)**:
   - `TopicDisplayItem` y `RealmStationDisplayItem` exponen `AutomationProperties.Name` con el formato limpio `"{Estado}: {NombreMision}"` (cero emojis) y `AutomationProperties.HelpText = "Presiona Enter o Espacio para comenzar."`.
4. ✅ **Tarea 4 (Retroalimentación Sonora y Anuncio Asertivo en Respuestas Incorrectas)**:
   - Añadido `PlayErrorChord()` en `AccessibleAudioCueService.cs` y `AnnounceAssertive(...)` / `SpeakAssertive(...)` en `AccessibilityManager.cs` y `SpeechService.cs`, invocados en `MiniGameView.EvaluateAnswer` con el prefijo `"Incorrecto. {feedbackMessage}"`.
5. ✅ **Tarea 5 (Desacoplamiento de Audio Interno `F2` vs. Lector de Pantalla Externo)**:
   - Separados `IsInternalAudioEnabled` (controlado por `F2` para los efectos sonoros internos del juego) e `IsScreenReaderIntegrationEnabled` (siempre activo por defecto para NVDA, JAWS y Narrador).
6. ✅ **Tarea 6 (Validación Adaptativa de Edad vs. Nivel Escolar y Filtrado de Misiones)**:
   - Implementados `AdaptiveEngine.GetAllowedLevelsForAge`, `GetRecommendedLevelForAge` y `CurriculumService.GetTopicsForAge`, filtrando dinámicamente `CmbAcademicLevel` en `ProfileSetupView`, las misiones en `CurriculumMenuView` y las estaciones en `RealmMapView`.
7. ✅ **Tarea 7 (Diálogo Modal Accesible `MissionCompletedDialog` al Completar una Misión)**:
   - Creada la ventana modal `MissionCompletedDialog.xaml` / `.xaml.cs` conectada a `_gameView.MissionCompleted` en `MainWindow.xaml.cs`, con foco automático en **"Siguiente Misión: [Nombre]"** (`Enter`) y **"Volver al Mapa del Reino"** (`Escape`).

### Fase 2 — Auditoría Integral (Tareas 8 a 12)
8. ✅ **Tarea 8 (Prevención de Doble Refresco y Doble Anuncio al Navegar)**:
   - Eliminada la suscripción duplicada a `Loaded` en `CurriculumMenuView.xaml.cs` y `RealmMapView.xaml.cs` y la llamada redundante en `MainWindow_Loaded`, garantizando que `RefreshData()` y `RefreshMap()` se ejecuten exactamente una vez por transición.
9. ✅ **Tarea 9 (Alineación de Etiquetas XAML con el Desacoplamiento de `F2`)**:
   - Actualizadas las etiquetas visuales y `AutomationProperties.Name` de `BtnToggleSpeech` (`"🔊 Sonidos (F2)"`) en `MainWindow.xaml` y `ChkVoiceSynthesis` en `ProfileSetupView.xaml` para reflejar que controlan los sonidos internos del juego mientras el lector de pantalla permanece siempre activo.
10. ✅ **Tarea 10 (Resolución de Fin de Etapa en `CurriculumService.GetNextTopic`)**:
    - `GetNextTopic` devuelve `null` cuando el alumno completa todas las misiones disponibles en su etapa de edad, permitiendo que `MissionCompletedDialog` active el mensaje de etapa superada y el botón **"Volver al Mapa del Reino"**.
11. ✅ **Tarea 11 (Sala de Trofeos `ItemsBadges` Navegable por Teclado y Flechas)**:
    - Convertido `ItemsBadges` en `RealmMapView.xaml` / `.xaml.cs` en un `ListBox` accesible con `WrapPanel`, navegación con flechas, foco de 4px y etiqueta limpia `AccessibleBadgeName` (`"Trofeo desbloqueado: ..."` / `"Trofeo bloqueado: ..."`).
12. ✅ **Tarea 12 (Flexibilidad Pedagógica en `GetAllowedLevelsForAge` para 12 Años y Discapacidad Intelectual)**:
    - Habilitado `SecondaryFirstGrade` a partir de los 12 años y añadido el parámetro opcional `DisabilityType` en `GetAllowedLevelsForAge` y `GetTopicsForAge` para incluir siempre los niveles base de refuerzo (`Kindergarten` y Primaria) cuando el alumno tiene `DisabilityType.Intellectual`.

### Fase 3 — Feedback de Usuario y Pruebas de Usabilidad (Mejoras 1 a 4)
13. ✅ **Mejora 1 (Tutorial y Lección Interactiva Paso a Paso — Wizard/Stepper)**:
    - Transformada la vista [`LessonView.xaml`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/LessonView.xaml) y [`LessonView.xaml.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/LessonView.xaml.cs) en un flujo guiado interactivo de 3 pasos (Paso 1: Historia con Cosmo, Paso 2: El Secreto Matemático, Paso 3: Ejemplo Guiado y Reto).
    - Eliminada la lectura corrida que causaba fatiga auditiva: ahora se muestra y anuncia una sola tarjeta a la vez con instrucciones interactivas ("Presiona Enter o Espacio para continuar...").
    - Navegación bidireccional mediante teclas de dirección (`Flecha Izquierda` / `Backspace` para retroceder, `Enter` / `Espacio` / `Flecha Derecha` para avanzar) y `F1` para repetir el paso actual.
    - La transición al minijuego práctico únicamente se habilita al alcanzar el último paso.
14. ✅ **Mejora 2 (Soporte para Discapacidades Múltiples — Comorbilidad e Interseccionalidad)**:
    - Añadida la propiedad `Disabilities` (`List<DisabilityType>`) y el método `HasDisability(...)` en [`UserProfile.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/src/MathTutor.Core/Models/UserProfile.cs), manteniendo la propiedad `Disability` sincronizada como fachada de retrocompatibilidad.
    - En [`ProfileSetupView.xaml`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/ProfileSetupView.xaml), sustituido el `ComboBox` de selección única por casillas de verificación (`CheckBox`) accesibles e independientes.
    - Adaptaciones simultáneas y acumulativas activadas en `ApplyRecommendedAccessibilityDefaults()` (por ejemplo, Ceguera + TEA o Intelectual + Motriz aplican al unísono tamaño de fuente, voz, lectura de pantalla, simplificación y paleta sensorial).
    - Adaptados [`AdaptiveEngine.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/src/MathTutor.Core/Engine/AdaptiveEngine.cs), [`CurriculumService.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/src/MathTutor.Core/Engine/CurriculumService.cs), [`CurriculumMenuView.xaml.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs) y [`RealmMapView.xaml.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/RealmMapView.xaml.cs) para aceptar colecciones de discapacidades.
15. ✅ **Mejora 3 (Anuncio Completo de Opciones en los Retos para TTS y Lector de Pantalla)**:
    - Implementado `AdaptiveEngine.BuildOptionsAnnouncement` y conectado en [`MiniGameView.xaml.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/MathTutor.WpfApp/Views/MiniGameView.xaml.cs) tanto al iniciar un ejercicio como al presionar `F1` (Repetir).
    - Anuncia explícitamente el enunciado y todas las opciones disponibles con sus atajos numéricos: `"{Pregunta}. Opción 1: {Opcion1}. Opción 2: {Opcion2}. Opción 3: {Opcion3}. Presiona 1, 2 o 3 para responder."` evitando que el alumno tenga que explorar a ciegas.
16. ✅ **Mejora 4 (Aleatorización de Opciones para Eliminar el Sesgo de Posición)**:
    - Implementado el algoritmo Fisher-Yates en `ExerciseGenerator.ShuffleOptions` y en `BuildOptions` dentro de [`ExerciseGenerator.cs`](file:///c:/Users/maria/Desktop/programas%20descargados/desarrollo/MathTutor/src/MathTutor.Core/Engine/ExerciseGenerator.cs).
    - Reindexa los atajos accesibles del 1 al N tras barajar, eliminando el sesgo en el que la respuesta correcta caía siempre en la opción 2 (tecla 2) y distribuyéndola equitativamente entre las opciones 1, 2 y 3.

### Fase 4 — Ajustes de Accesibilidad y Pruebas de Campo
17. ✅ **Captura Global de Atajos Numéricos (`1`, `2`, `3`) en `MiniGameView`**:
    - Método `SelectOptionByIndex` y foco estabilizado con `DispatcherPriority.Loaded` en `MiniGameView.xaml.cs`.
    - En `MainWindow.xaml.cs`, interceptación y delegación global de `Key.D1..D4` y `Key.NumPad1..NumPad4` hacia `_gameView` si está activo, permitiendo responder retos inmediatamente sin tabular.
18. ✅ **Carga y Desbloqueo Continuo de la Lista de Misiones (`CurriculumService`)**:
    - Parámetro `completedLessonIds` integrado en `CurriculumService.GetTopicsForAge` y `GetNextTopic`.
    - Al completar las misiones base de Kínder (o del nivel actual), los alumnos de 5+ años desbloquean de forma automática las misiones de Primaria Inicial (`EarlyElementary`), avanzando sin bloqueos en el currículo.
19. ✅ **Navegación Direccional por Flechas en `CurriculumMenuView`**:
    - Método `FocusInitialElement()` y gestión asíncrona de generación de contenedores (`ItemContainerGenerator.StatusChanged`) para posicionar el foco físico directamente sobre el primer `ListBoxItem`, habilitando de inmediato la navegación nativa con flechas arriba/abajo.
20. ✅ **Aleatorización Completa en Retos Curados (`CurriculumService.GenerateAdaptiveExercises`)**:
    - Aplicación de `ShuffleOptions` a todos los ejercicios retornados, incluyendo el ejercicio modelo inicial de cada lección, garantizando la alternancia equitativa de la opción correcta en cualquier intento.

---

## 5. Comandos de Verificación, Empaquetado y Limpieza Obligatoria para Contribuidores

> **REGLA DE HIGIENE DE DISCO**: Las salidas de compilación viven únicamente en `artifacts/`; los proyectos no deben contener `bin/` ni `obj/`. Antes de una nueva release se limpia `artifacts/` y se conserva solo el ZIP vigente de `artifacts/releases/`.

```powershell
# 1. Limpiar cualquier residuo de compilaciones anteriores antes de compilar la nueva versión
if (Test-Path artifacts) { Remove-Item artifacts -Recurse -Force }

# 2. Compilar toda la solución en modo Release (0 advertencias permitidas)
dotnet build -c Release --nologo /p:TreatWarningsAsErrors=true

# 3. Ejecutar las 114 pruebas unitarias automatizadas sobre la compilación Release
dotnet test -c Release --nologo --no-build

# 4. Publicar y empaquetar WinUI 3 (verifica MathTutor.WinUI.pri)
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-winui.ps1

# 5. Conservar únicamente el ZIP de la release vigente
Get-ChildItem artifacts\releases -Filter *.zip -File | Where-Object { $_.Name -ne 'MathTutor-v1.7.0-Windows.zip' } | Remove-Item -Force
```

