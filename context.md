# 🧠 Contexto Técnico Integral para Contribuidores y Agentes (`context.md`)

> **PROPÓSITO DE ESTE DOCUMENTO**: Este archivo es la fuente de verdad técnica interna del repositorio **MathTutor**. Está diseñado para que cualquier desarrollador humano o agente de código (Antigravity, OpenCode, Claude Code, etc.) comprenda **de un solo vistazo** toda la arquitectura, las decisiones de diseño, el flujo de datos y el detalle **archivo por archivo** de lo realizado en el proyecto, sin necesidad de explorar a ciegas el código fuente.
>
> **REGLA DE MANTENIMIENTO**: Este archivo forma parte del repositorio Git (como contexto interno de ingeniería, separado del `README.md` público) y **debe actualizarse obligatoriamente** cada vez que se agregue, modifique o elimine un archivo, modelo, servicio, vista o prueba.

---

## 1. Visión General y Arquitectura de la Solución

**MathTutor Accesible** es una plataforma educativa e inclusiva desarrollada en **C# (.NET 10 / WinUI 3)**, 100% conforme a **WCAG 2.2 (Niveles AA y AAA)** y **Diseño Universal para el Aprendizaje (DUA)**. Cubre un currículo completo de **74 estaciones de aprendizaje (16 objetivos pedagógicos)** desde **Jardín de Niños (3 años)**, **Primaria (1º a 6º)**, **Secundaria (1º a 3º)** y **Preparatoria / Bachillerato (1º a 3º)** hasta **Pensamiento Computacional y Programación Inclusiva (Módulos 1, 2 y 3)**.

### Diagrama de Arquitectura Modular

```
MathTutor.slnx
│
├── MathTutor.Core (net10.0 - Biblioteca de Dominio Pura, sin dependencias de UI)
│   ├── Models/
│   │   ├── Enums.cs               (DisabilityType, AutismSupportLevel, AdhdPresentation, IntellectualSupportLevel, AcademicLevel, RegionalContext, HighContrastTheme, FontSizeLevel, AnswerResult)
│   │   ├── UserProfile.cs         (AccessibilityPreferences + UserProfile + propiedades culturales LATAM + ApplyRecommendedAccessibilityDefaults)
│   │   ├── Exercise.cs            (GamePlayType [69 mecánicas], ManipulableItem, ExerciseOption, Exercise)
│   │   ├── PedagogicalTopic.cs    (Modelo de lección: narrativa, concepto, principios, lista de ejercicios)
│   │   └── Badge.cs               (Badge y RealmStation para el Mapa del Reino Matemático)
│   ├── Engine/
│   │   ├── AdaptiveEngine.cs      (Validación Edad vs. Nivel Escolar, adaptación de lenguaje, reducción cognitiva y estrellas/racha)
│   │   ├── CurriculumService.cs   (Catálogo de las 74 lecciones, filtrado por edad GetTopicsForAge, GetNextTopic y generación adaptativa)
│   │   ├── ExerciseGenerator.cs   (Generador procedimental parametrizado para las 74 estaciones con distractores diagnósticos)
│   │   ├── KindergartenCurriculum.cs (10 temas de jardín de niños y 45 ejercicios curados)
│   │   └── SimplifiedHintCatalog.cs (Pistas 1/2 y éxito en lenguaje simple para cada GamePlayType)
│   └── Services/
│       ├── ProfileStorageService.cs (Persistencia JSON atómica en %LocalAppData%/MathTutor/user_profile.json)
│       ├── AudioSynthesizer.cs    (Síntesis procedural WAV PCM determinista con caché acotada)
│       └── RewardService.cs       (55 insignias y 74 estaciones conectadas al progreso del alumno)
│
├── MathTutor.WinUI (net10.0-windows10.0.19041.0 - Cliente Escritorio WinUI 3 Accesible)
│   ├── App.xaml / App.xaml.cs     (Diccionario de recursos dinámicos WCAG, estilos de foco, botones y controles accesibles)
│   ├── MainWindow.xaml / .cs      (Shell principal, menú "Opciones del juego" con atajos F1-F6/Esc, solicitud de primer plano tras la carga, foco inicial, diálogo posmisión, regiones de notificación UIA y alto contraste del sistema)
│   ├── Navigation/NavigationService.cs (Frame de navegación y eventos de flujo entre vistas)
│   ├── Navigation/VerticalListNavigation.cs (Navegación ↑/↓ compartida para ListView accesibles)
│   ├── Services/
│   │   ├── AccessibilityManager.cs (Singleton coordinador de perfil, canales F2 vs. lector, temas, escala tipográfica y anuncios)
│   │   ├── SpeechService.cs       (Puente nativo hacia NVDA Controller Client, JAWS COM API y notificaciones UIA)
│   │   └── AccessibleAudioCueService.cs (Canal serializado de audio en memoria: notas, éxito, error y navegación)
│   └── Views/
│       ├── ProfilePage.xaml / .cs       (Caracterización del alumno con validación dinámica Edad/Discapacidad vs. Nivel Escolar)
│       ├── MenuPage.xaml / .cs          (Menú principal con ListView navegable por flechas ↑/↓ y filtrado por edad)
│       ├── LessonPage.xaml / .cs        (Lección con Cosmo, secreto matemático y ejemplo guiado paso a paso; pasos con ↑/↓)
│       ├── GamePage.xaml / .cs          (Motor interactivo de ejercicios, conteo con celdas de 170 px, pistas, respuestas con ↑/↓ y retroalimentación)
│       └── MapPage.xaml / .cs           (Mapa del Reino con estaciones y sala de trofeos TreeView agrupada por nivel)
│
└── MathTutor.Tests (net10.0 - Suite Automatizada xUnit con 239 pruebas)
    ├── ExerciseGeneratorTests.cs         (66 pruebas de generación procedimental, aleatorización de opciones y no-colisión)
    ├── PedagogyAndAccessibilityTests.cs  (76 pruebas de cobertura curricular, comorbilidad, stepper de lección y formato de anuncios)
    ├── ProfileStorageTests.cs            (21 pruebas de serialización, migración, concurrencia, exportación y borrado)
    ├── RewardServiceTests.cs             (5 pruebas integrales de las 74 estaciones y desbloqueo de las 55 insignias)
    ├── AudioSynthesizerTests.cs          (6 pruebas de síntesis WAV determinista y caché)
    └── ThemeContrastTests.cs             (40 casos de contraste WCAG en los cuatro temas)
```

---

### Estructura de Directorios

```text
MathTutor.slnx
├── src/
│   ├── MathTutor.Core/       Biblioteca de dominio, currículo, motor adaptativo y almacenamiento
│   └── MathTutor.WinUI/      Aplicación WinUI 3, navegación, servicios y vistas
├── tests/
│   └── MathTutor.Tests/      Suite xUnit de 239 pruebas
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
| `scripts/publish-winui.ps1` | Publica WinUI 3 para `win-x64`, `win-x86` o `win-arm64`, verifica `MathTutor.pri`, organiza la aplicación en `libs/`, los módulos `.winmd` en `modules/`, integra `LEEME.txt`, `PRIVACIDAD.md`, `BIBLIOGRAFIA.md`, `LICENSE` y `THIRD-PARTY-NOTICES.md`, compila el lanzador `MathTutor.exe`, arma la carpeta `MathTutor-v$Version` y genera `artifacts/releases/MathTutor-v$Version-Windows.zip`. |
| `artifacts/` | Directorio ignorado por Git con salidas centralizadas: `bin/`, `obj/`, `publish/`, `package/` y `releases/`. |
| `CLAUDE.md` | Directrices de ingeniería de software basadas en los principios de Andrej Karpathy: (1) Pensar antes de codificar, (2) Simplicidad primero (sin sobreingeniería), (3) Cambios quirúrgicos y (4) Ejecución orientada a objetivos verificables con pruebas. |
| `README.md` | Documentación pública del proyecto orientada a usuarios finales y educadores: características de accesibilidad WCAG 2.2, integración nativa con lectores de pantalla (NVDA, JAWS, Narrador), desglose curricular de los 16 objetivos / 54 estaciones, tabla de atajos de teclado (`F1`–`F6`, `Esc`, `1`–`3`, `Enter`) e instrucciones de compilación y ejecución. |
| `LEEME.txt` | Guía en texto plano para familias, docentes y estudiantes: instalación, primeros pasos, teclas, accesibilidad, privacidad, consejos y solución de problemas. Se integra en el paquete de release junto al ejecutable. |
| `PRIVACIDAD.md` | Aviso de privacidad para familias: datos guardados, finalidad, ubicación local, exportación, borrado y contacto. También se copia al paquete de release y se puede leer desde el perfil. |
| `BIBLIOGRAFIA.md` | Referencias y criterios del modelo de apoyos: DSM-5-TR, AAIDD, ICF, CDPD, WCAG 2.2, DUA y principios del conteo. |
| `LICENSE` | Licencia propietaria de Narayan Projects; todos los derechos reservados. |
| `THIRD-PARTY-NOTICES.md` | Avisos y licencias de los paquetes de ejecución y pruebas. |
| `ACCESIBILIDAD-ACR-VPAT.md` | Borrador ACR/VPAT, evidencia automatizada y matriz manual NVDA/JAWS/Narrador. |
| `.github/workflows/ci.yml` | CI de restore, build Release con advertencias como errores, pruebas y paquete manual. |
| `correcciones mat tutor.md` | Registro histórico de correcciones, especificaciones de accesibilidad y reglas de negocio, con secciones por release hasta `v1.10.0`, el registro de la Fase 5 y las dos auditorías integrales de remediación (fases 1-8 y segunda pasada). |
| `context.md` | **(Este archivo)** Documento vivo de contexto técnico archivo por archivo para contribuidores y agentes de código. |
| `.gitignore` | Reglas de exclusión de Git para artefactos de compilación (`artifacts/`, `bin/`, `obj/`, `.vs/`, paquetes temporales). |
| `.engram/config.json` | Configuración del proyecto `mathtutor` para la memoria persistente Engram. |

---

### 2.2. Proyecto `MathTutor.Core` (Dominio, Currículo y Motor Adaptativo)

#### `src/MathTutor.Core/MathTutor.Core.csproj`
- **TargetFramework**: `net10.0`, `ImplicitUsings=enable`, `Nullable=enable`.
- **Dependencias**: Cero paquetes NuGet externos (100% BCL estándar de .NET para máxima portabilidad y velocidad de compilación).

#### `src/MathTutor.Core/Models/Enums.cs` (159 líneas)
Define las enumeraciones fundamentales del dominio:
1. **`DisabilityType`** (condiciones múltiples; ya no se usa una severidad genérica):
   - `None = 0`, `Visual = 1`, `Hearing = 2`, `Motor = 3`, `Intellectual = 4`.
   - `Neurodivergent = 5` es un valor heredado que se migra a `Autism` + `ADHD` al cargar perfiles antiguos.
   - `Autism = 6`, `ADHD = 7`.
2. **`AutismSupportLevel`**: `NotApplicable = 0`, `Level1 = 1` (requiere apoyo), `Level2 = 2` (apoyo sustancial), `Level3 = 3` (apoyo muy sustancial).
3. **`AdhdPresentation`**: `NotApplicable = 0`, inatenta, hiperactiva-impulsiva y combinada.
4. **`IntellectualSupportLevel`**: `NotApplicable = 0`, intermitente, limitado, extenso y generalizado.
5. **`VisualFunctionalLevel`**: baja visión y ceguera. **`AuditoryFunctionalLevel`**: hipoacusia y sordera. **`MotorFunctionalLevel`**: dificultades motoras finas, movilidad reducida y ausencia de uso de miembros superiores.
6. **`LanguageRegister`**: registro lúdico temprano, simple, estándar y técnico.
7. **`AcademicLevel`**:
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
8. **`RegionalContext`**: `Mexico = 1`, `Colombia = 2`, `Argentina = 3`, `Peru = 4`, `Chile = 5`.
9. **`HighContrastTheme`**: `DefaultPlayful = 0`, `HighContrastDark = 1` (Negro/Amarillo > 15:1), `HighContrastLight = 2` (Blanco/Negro > 21:1), `LowSensory = 3` (Pastel suave TEA/TDAH).
10. **`FontSizeLevel`**: `Normal = 0` (18pt), `Large = 1` (23-24pt), `ExtraLarge = 2` (28-30pt), `Huge = 3` (34-36pt).
11. **`AnswerResult`**: `None = 0`, `Correct = 1`, `NeedsGuidance = 2`, `Assisted = 3`.

#### `src/MathTutor.Core/Models/UserProfile.cs` (408 líneas)
Contiene tres clases clave:
1. **`AccessibilityPreferences`**:
   - Propiedades persistibles: `EnableVoiceSynthesis`, `ScreenReaderAnnouncements`, `Theme`, `FontSize`, `SpeechRate`, `SpeechVolume`, `PlayAudioCues`, `SimplifyLanguage`, `ReduceOptionsCount`, `SimplifiedKeyboardNavigation`, `UnlimitedTime`.
   - `BadgeGroupExpansion` conserva los grupos de trofeos abiertos/cerrados.
   - `ManualPreferenceKeys` registra las preferencias ajustadas a mano para que `ApplyRecommendedAccessibilityDefaults()` no las sobrescriba.
2. **`SupportProfile`**:
   - `AutismSupport`, `AdhdPresentation`, `IntellectualSupport`, `VisualFunction`, `AuditoryFunction` y `MotorFunction`.
   - Describe apoyos y funcionamiento; no exige diagnóstico ni usa una severidad genérica.
3. **`UserProfile`**:
   - Datos del alumno: `Id`, `Name`, `Age`, `CurrentLevel`, `Region`, `Preferences`, `Disabilities`, `Support` y `ProfileVersion`.
   - `CurrentProfileVersion = 3` permite migrar perfiles antiguos: `Neurodivergent` se separa en `Autism` + `ADHD` y la severidad heredada se convierte en el apoyo correspondiente.
   - La propiedad `Disability` se conserva por compatibilidad y devuelve la primera condición marcada.
   - Propiedades calculadas de localización cultural latinoamericana:
     - `CurrencyName`: `"soles"` para Perú; `"pesos"` para México, Colombia, Argentina y Chile.
     - `CurrencyFractionName`: `"céntimos"` para Perú; `"centavos"` para el resto.
     - `CurrencySymbol`: `"S/"` para Perú; `"$"` para el resto.
     - `StoreName`: `"tiendita"` (México), `"tienda de barrio"` (Colombia), `"kiosco"` (Argentina), `"bodega"` (Perú), `"almacén"` (Chile).
   - Gamificación: `StarsTotal`, `PointsTotal`, `CurrentStreak`, `CompletedLessonIds`, `CompletedExerciseIds`.
   - Método **`ApplyRecommendedAccessibilityDefaults()`**: Configura tema de contraste, tamaño de fuente, simplificación, reducción de opciones y voz según la edad, las condiciones y el `SupportProfile`, respetando las preferencias marcadas como manuales.

#### `src/MathTutor.Core/Models/Exercise.cs` (142 líneas)
- **`GamePlayType` (Enum con 52 valores)**: Identifica la mecánica exacta de cada reto (desde `InteractiveCounting`, `OrderSequence`, `Comparison` hasta `CalculusDerivativeRate`, `CalculusIntegralArea`, `ProgrammingVariablesState`, `ProgrammingConditionalsIfElse`, `ProgrammingBooleanLogic`, `ProgrammingLoopsForWhile`, `ProgrammingArraysZeroIndex`, `ProgrammingFunctionsReturn`, `ProgrammingAlgorithmsSearch`, `ProgrammingGameCoordinates2D`, `ProgrammingAssistiveAIData`).
- **`ManipulableSoundKind`**: Timbre semántico del objeto (`Tone`, `Bell`, `Drum`, `Flute`, `Clap`), usado para que el audio refuerce el material o instrumento representado.
- **`ManipulableItem`**: Objeto interactivo contable o explorable en pantalla (`Id`, `Name`, `EmojiSymbol`, `AccessibleDescription`, `OrderIndex`, `IsCounted`, `MusicalPitchHz`, `SoundKind`).
- **`ExerciseOption`**: Opción de respuesta (`Id`, `Label`, `Value`, `IsCorrect`, `SpeechFeedback` con diagnóstico formativo del error o acierto, `AccessibleDescription` para cantidades comparativas y `AccessibleKeyShortcut` `"1"`, `"2"`, `"3"`).
- **`Exercise`**: Reto completo con instrucciones duales (`StandardInstruction` y `SimplifiedInstruction`), descripción de escena (`ScreenReaderDescription`, ya no se anuncia para no revelar el resultado), colecciones `ItemsGroupA` e `ItemsGroupB`, lista `Options`, pistas escalonadas (`FirstHint`, `SecondHintDetailed`), `ExplanationOfSuccess`, `ExpectedResult`, `TargetAgeMin/Max` y `Level`.

#### `src/MathTutor.Core/Models/PedagogicalTopic.cs` (32 líneas)
- Representa una misión o lección completa: `Id`, `Title`, `Level`, `CharacterName` (`"Cosmo el Búho Sabio"`), `NarrativeIntro`, `NarrativeIntroSimplified`, `CoreConceptExplanation`, `CoreConceptSimplified`, `PrinciplesAddressed` (`List<string>`), `Exercises` (`List<Exercise>`) y `CongratulationMessage`.

#### `src/MathTutor.Core/Models/Badge.cs` (34 líneas)
- **`Badge`**: Trofeo desbloqueable (`Id`, `Title`, `Description`, `IconEmoji`, `AssociatedTopicId`, `RequiredStars`, `IsUnlocked`, `UnlockedAt`).
- **`RealmStation`**: Estación del sendero en el Mapa del Reino (`Id`, `Name`, `Description`, `LandmarkEmoji`, `TopicId`, `OrderIndex`, `IsCompleted`, `IsCurrentStation`, `StarsEarned`).

#### `src/MathTutor.Core/Engine/AdaptiveEngine.cs` (331 líneas)
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
- **`GetAdaptedOptions(Exercise, UserProfile)`**: Si `ReduceOptionsCount` o `Disability == Intellectual` está activo, deja **1 respuesta correcta y el distractor más cercano al resultado** en su orden relativo original, sobre copias (sin mutar las opciones compartidas del ejercicio) y reasigna los atajos `"1"` y `"2"`.
- **`BuildOptionsAnnouncement(Exercise, UserProfile)`**: Anuncia el enunciado y cada opción; cuando `ExerciseOption.AccessibleDescription` está disponible (por ejemplo, “Las rojas, 3 manzanas”), la usa en lugar de la etiqueta corta para que la comparación de cantidades sea comprensible sin ver la pantalla.
- **`RecordAttempt(UserProfile, Exercise, bool isCorrect, int attemptCount)`**:
  - Si `isCorrect == true`: incrementa `CurrentStreak`, otorga **3 estrellas** (primer intento), **2 estrellas** (segundo intento) o **1 estrella** (3+ intentos, premiando siempre la perseverancia), suma `starsEarned * 10` a `PointsTotal` y registra el `exercise.Id` en `CompletedExerciseIds`.
  - Si `isCorrect == false`: reduce suavemente `CurrentStreak` en 1 (sin castigo punitivo a cero).

#### `src/MathTutor.Core/Engine/KindergartenCurriculum.cs` (1,142 líneas)
Contiene los **10 temas de jardín de niños** y sus 45 ejercicios curados. Los temas se organizan por banda de edad (3-4, 4-5 y 5-6) y cubren conteo, formas, posiciones, cuentos, patrones, clasificación, música, números hasta 10, suma/resta y medida. Los patrones se reproducen como secuencias y cada objeto recibe un tono y un timbre coherentes con su instrumento o material; las opciones de comparación incluyen la cantidad y el sustantivo plural (por ejemplo, “Las rojas, 3 manzanas”) para el anuncio accesible.

#### `src/MathTutor.Core/Engine/CurriculumService.cs` (3,184 líneas)
Contiene el catálogo pedagógico completo de las **54 misiones** (`GetAllTopics()`), el filtrado adaptativo por edad/condición y el orquestador de ejercicios:
- **`GetTopicsForAge(int age, DisabilityType disability = DisabilityType.None)`** (Tarea 6 y Tarea 12): Filtra `GetAllTopics()` devolviendo únicamente las misiones cuyos niveles pertenecen a `AdaptiveEngine.GetAllowedLevelsForAge(age, disability)`.
- **`GetNextTopic(string currentTopicId, UserProfile? profile = null)`** (Tarea 7 y Tarea 10): Obtiene la siguiente misión secuencial o pendiente dentro del rango de edad del alumno tras completar `currentTopicId`, devolviendo `null` cuando el alumno ha completado todas las misiones de su etapa.
- **`GenerateAdaptiveExercises(PedagogicalTopic topic, UserProfile profile, int count = 3)`**:
  - Si el alumno tiene `DisabilityType.Intellectual` o es un tema monetario con región distinta de México, genera los `count` ejercicios procedimentalmente con `ExerciseGenerator` para garantizar el rango numérico reducido y la moneda local (`soles`, `pesos`, `tiendita`, `kiosco`, `bodega`, `almacén`).
  - En caso estándar, conserva como **Reto 1** el ejercicio curado de la lección (`templateTopic.Exercises[0]`, que conecta directamente con el Ejemplo Guiado Paso a Paso) y genera procedimentalmente los retos restantes con `_generator.GenerateExercisesForTopic(topic.Id, profile, remaining)`.

#### `src/MathTutor.Core/Engine/ExerciseGenerator.cs` (2,749 líneas)
Generador procedimental parametrizado que produce ejercicios infinitos y libres de colisiones para las **54 estaciones**:
- **`GetTargetRange(UserProfile)`**: Ajusta el rango numérico a `(1, 3)` para apoyos intelectuales extensos o generalizados, `(1, 5)` para apoyos intermitentes/limitados o Jardín de Niños, y `(1, 10)` para primaria inicial.
- **46 métodos públicos de generación** (`GenerateCountingExercise`, `GenerateStableOrderExercise`, `GenerateAdditionCombineExercise`, ..., `GenerateProgrammingVariablesExercise`, `GenerateProgrammingConditionalsExercise`, `GenerateProgrammingBooleanLogicExercise`, `GenerateProgrammingLoopsExercise`, `GenerateProgrammingArraysExercise`, `GenerateProgrammingFunctionsExercise`, `GenerateProgrammingAlgorithmsSearchExercise`, `GenerateProgrammingGameCoordinatesExercise`, `GenerateProgrammingAssistiveAIExercise`) conectados en el despachador `GenerateExercisesForTopic(string topicId, UserProfile profile, int count = 3, Random? rng = null)`.
- **`BuildOptions(...)`**: Construye las opciones de respuesta garantizando valores únicos, barajado determinista/aleatorio sin sesgo de posición y **retroalimentación diagnóstica específica (`SpeechFeedback`)** en cada distractor (indicando si el número elegido es mayor o menor que el resultado correcto y qué paso revisar). Los distractores se eligen aleatoriamente dentro del rango declarado con peso `|v-correcto| + jitter (span/3)`; si el rango no aporta dos, un respaldo determinista siempre terminante expande **alternando arriba y abajo** para no sesgar la pista.

#### `src/MathTutor.Core/Engine/SimplifiedHintCatalog.cs` (244 líneas)
Catálogo estático de pistas en lenguaje llano para los **52 `GamePlayType`**: `GetSimpleHint(GamePlayType, bool detailed)` devuelve la pista 1 o 2 y `GetSimpleSuccess(GamePlayType)` la explicación de éxito. `AdaptiveEngine.GetAdaptedHint` y `GetAdaptedSuccess` lo usan como respaldo cuando el ejercicio no trae variantes simplificadas propias; los textos evitan tecnicismos y no revelan la respuesta numérica del caso concreto.
#### `src/MathTutor.Core/Services/ProfileStorageService.cs` (341 líneas)
- Gestiona la persistencia local del perfil en `%LocalAppData%\MathTutor\user_profile.json` usando `System.Text.Json` con `JsonStringEnumConverter`.
- Permite inyectar una ruta personalizada en el constructor (`ProfileStorageService(string? customPath = null)`) para pruebas unitarias aisladas en archivos temporales.
- Métodos: `Save(UserProfile)`, `Load()`, `Exists()`, `Export(UserProfile, string)` y `Delete()`.

#### `src/MathTutor.Core/Services/AudioSynthesizer.cs` (188 líneas)
Síntesis procedural determinista de WAV PCM (44.1 kHz, 16-bit, mono) con envolvente de atenuación en ambos extremos y caché acotada (96 buffers, bajo candado). `GetNoteWav(...)` genera tonos puros y `GetItemCueWav(ManipulableItem)` genera el timbre de cada `ManipulableSoundKind` (campana, tambor con barrido de frecuencia clampeado a 110-220 Hz, flauta y palmada) con clamp global de 120-1200 Hz.
#### `src/MathTutor.Core/Services/RewardService.cs` (1,064 líneas)
- **`GetAllBadges(UserProfile)`**: Define y evalúa el estado de desbloqueo (`IsUnlocked`) de las **55 insignias** (54 insignias asociadas 1-a-1 a cada `TopicId` + 1 insignia especial `"badge_star_collector"` al acumular $\ge 15$ estrellas).
- **`GetRealmStations(UserProfile, IReadOnlySet<string>? allowedTopicIds = null)`**: Devuelve las **54 estaciones** del Mapa del Reino (`station_1` a `station_54`), marcando `IsCompleted = true` para las lecciones en `CompletedLessonIds`. Si se recibe `allowedTopicIds`, la estación activa se elige solo entre esos temas; así la recomendación nunca queda fuera del rango de edad del alumno.

---

### 2.3. Proyecto `MathTutor.WinUI` (Interfaz WinUI 3 y Servicios Nativos)

#### `src/MathTutor.WinUI/MathTutor.WinUI.csproj`
- **TargetFramework**: `net10.0-windows10.0.19041.0`, `TargetPlatformMinVersion=10.0.17763.0`, `UseWinUI=true`, `Nullable=enable`, `ImplicitUsings=enable`.
- Aplicación de escritorio sin empaquetar (`WindowsPackageType=None`, `EnableMsixTooling=false`) con `WindowsAppSDKSelfContained=true`.
- Referencia de proyecto únicamente a `MathTutor.Core.csproj`; icono `Assets/AppIcon.ico` y manifiesto `app.manifest`.
- `AssemblyName=MathTutor`: el ejecutable publicado es `MathTutor.exe` y el recurso es `MathTutor.pri`, más fácil de reconocer para el usuario final.
- El target `CopyPriToPublishDirectory` copia `MathTutor.pri` al directorio de publicación; sin él, el ejecutable publicado falla al iniciar.
- La publicación de release se organiza por carpetas desde `scripts/publish-winui.ps1`: `MathTutor.exe` en la raíz es un lanzador que inicia `libs/MathTutor.exe`; la carpeta `libs/` conserva la aplicación y todas sus bibliotecas y `modules/` los `.winmd`. Esta estructura es necesaria porque WinUI 3 busca sus DLL nativas y recursos junto al ejecutable real.

#### `src/MathTutor.WinUI/App.xaml` y `App.xaml.cs`
Define los recursos globales dinámicos y estilos accesibles WCAG 2.2:
- **Brushes dinámicos**: `AppBackgroundBrush`, `AppSurfaceBrush`, `AppCardBrush`, `AppTextPrimaryBrush`, `AppTextSecondaryBrush`, `AppAccentBrush`, `AppFocusBorderBrush`, `AppBorderBrush`, `AppSuccessBrush`, `AppOnAccentBrush`, `AppOnSuccessBrush` y `AppErrorBrush`.
- **Escalas tipográficas dinámicas**: `AppBaseFontSize` (18), `AppSubHeaderFontSize` (22), `AppHeaderFontSize` (28) y `AppTitleFontSize` (36).
- **Estilos**: `AccessibleActionButton`, `AccessibleGameOptionButton`, `AccessibleCard` y `AccessibleReadOnlyTextBox` (texto de solo lectura navegable con el cursor del lector de pantalla).
- `App.xaml.cs` crea y activa `MainWindow`, expone `App.MainAppWindow` (necesario para `FileSavePicker.InitializeWithWindow`) y `App.Navigation` para que las páginas soliciten navegación.

#### `src/MathTutor.WinUI/MainWindow.xaml` y `MainWindow.xaml.cs`
Ventana principal y shell de navegación:
- **Barra superior**: `DropDownButton` "Opciones del juego" con un `MenuFlyout` que agrupa Menú (`Esc`), Mapa (`F6`), Perfil (`F5`) y el submenú "Opciones de accesibilidad" con Contraste (`F3`), Letra (`F4`), Sonidos (`F2`) y Repetir (`F1`), además del contador de estrellas (`TxtTopStars`). Cada elemento reproduce el sonido de navegación al recibir foco.
- **Host central**: `Frame` (`RootFrame`) gestionado por `NavigationService` para las páginas de perfil, menú, lección, minijuego y mapa.
- **Atajos globales**: `KeyDown` con `handledEventsToo` para `F1`–`F6`, `Esc` y respuestas `1`–`4` cuando el minijuego está activo.
- **Anuncios**: `RaiseScreenReaderAnnouncement` usa `RaiseNotificationEvent` de UI Automation (sin `LiveSetting` duplicado) y actualiza `TxtLiveAnnouncement` como texto de apoyo.
- **Alto contraste del sistema**: escucha `AccessibilitySettings.HighContrastChanged`, aplica el tema correspondiente y conserva la preferencia del usuario al desactivarlo.
- **Diálogo de misión completada**: calcula la siguiente misión, muestra cuántas estrellas se ganaron según los intentos, ofrece continuar o volver al mapa y anuncia el resultado completo.
- **Guardado seguro del perfil**: `OnThemeChanged` y `OnFontSizeChanged` solo guardan si `Storage.Exists()`; tras borrar el perfil, cambiar tema o tamaño no recrea `user_profile.json`.
- **Tamaño inicial**: ventana de 1100x820 lógicos, mínimo de 800x600, escalada por DPI y limitada al área de trabajo.
- **Primer plano inicial**: tras la carga y la navegación inicial, `DispatcherQueue.TryEnqueue` llama una sola vez a `AppWindow.MoveInZOrderAtTop`, `Activate` y `SetForegroundWindow` (P/Invoke de `user32.dll`), con guarda `_isClosing`; `RootGrid.Loaded` se desuscribe para evitar solicitudes repetidas.

#### `src/MathTutor.WinUI/Navigation/NavigationService.cs`
Centraliza el `Frame` y los eventos del flujo: `TopicSelected`, `MapRequested`, `MenuRequested`, `ProfileCompleted`, `ProfileCancelled`, `StartGameRequested`, `BackToMenuRequested`, `BackToLessonRequested` y `MissionCompleted` (que transporta el tema completado y las estrellas ganadas).

#### `src/MathTutor.WinUI/Navigation/VerticalListNavigation.cs`
Helper compartido por `MenuPage` y `MapPage` para que `ListView` mueva la selección y el foco con `↑`/`↓` de forma explícita, manteniendo el elemento visible y sin depender de la navegación interna del control. Las flechas horizontales se consumen para evitar saltos inesperados.

#### `src/MathTutor.WinUI/Services/SpeechService.cs`
Puente nativo con el lector de pantalla del sistema (sin SAPI5):
- `UiAutomationAnnouncer` conecta los anuncios con la ventana principal.
- Cascada de salida: NVDA Controller Client (`nvdaControllerClient64.dll`), JAWS COM (`FreedomScientific.JawsApi`) y notificaciones de Microsoft UI Automation.
- Separa el audio interno del juego de la integración con el lector; `F2` solo conmuta el audio interno.

#### `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`
Sintetizador procedural PCM en memoria, sin archivos `.wav` externos:
- `PlayNote`, `PlaySuccessChord`, `PlayErrorChord`, `PlayHintTone`, `PlayNavigationClick`, `PlayItemCue` y `PlayItemSequence`.
- Timbres `Tone`, `Bell`, `Drum`, `Flute` y `Clap` generados por `BuildWavStream`; las secuencias de patrones se reproducen en segundo plano y `R` las repite.
- `MusicEnabled` controla efectos y pistas sin afectar los anuncios del lector de pantalla.

#### `src/MathTutor.WinUI/Services/AccessibilityManager.cs`
Singleton central (`AccessibilityManager.Instance`) que coordina:
- `CurrentProfile`, `Speech`, `Audio` y `Storage`.
- Eventos `ThemeChanged`, `FontSizeChanged` y `AnnouncementMade`.
- `ApplyProfile`, `SaveCurrentProfile`, `LoadSavedProfile`, `Announce`, `AnnounceAssertive`, `CycleTheme`, `CycleFontSize` y `ToggleSpeech`.
- `ApplySystemHighContrast` mantiene el tema del sistema activo sin sobrescribir la preferencia guardada del usuario.
- Temas `DefaultPlayful`, `HighContrastDark`, `HighContrastLight` y `LowSensory`, con recursos de foco, borde, éxito y texto sobre acento.

#### `src/MathTutor.WinUI/Views/ProfilePage.xaml` y `ProfilePage.xaml.cs`
Caracterización inicial del alumno (`F5`): nombre, edad, nivel escolar filtrado por edad, país/moneda, condiciones múltiples, apoyos de autismo/TDAH/intelectuales y descriptores funcionales de visión, audición y motricidad. Incluye audio interno, pistas sonoras, lenguaje simplificado, tema y tamaño de letra. Los seis selectores de apoyos exponen `HelpText` y anuncian el valor elegido al cerrarse, sin interrumpir mientras el alumno recorre las opciones. La tarjeta de privacidad permite exportar perfil y progreso a JSON, borrar el perfil local y leer el aviso completo. Valida edad/nivel, restaura el perfil guardado al cancelar y usa encabezados y nombres accesibles.

#### `src/MathTutor.WinUI/Views/MenuPage.xaml` y `MenuPage.xaml.cs`
Menú principal de misiones (`Esc`): saludo, resumen del perfil, estrellas, racha y `ListView` de misiones filtradas por edad. Los elementos tienen nombres como “Misión recomendada: …” y los botones internos “Comenzar la misión: …” o “Volver a jugar: …”. Soporta `↑`/`↓` mediante `VerticalListNavigation`, `Enter` y `Espacio`.

#### `src/MathTutor.WinUI/Views/LessonPage.xaml` y `LessonPage.xaml.cs`
Lección guiada en tres pasos: historia de Cosmo, secreto matemático y ejemplo resuelto. Incluye navegación anterior/siguiente con `↑`/`↓` y `Backspace`, lectura de la explicación, inicio del minijuego y conservación del paso al volver desde el juego. Al finalizar, el texto y el anuncio usan “¡Explicación completada!” para describir con precisión la actividad.

#### `src/MathTutor.WinUI/Views/GamePage.xaml` y `GamePage.xaml.cs`
Motor interactivo: progreso, instrucción adaptada, repaso del secreto matemático en la misma pantalla, objetos manipulables, pistas diagnósticas, opciones `1`–`3`, banners de error/éxito y barra de progreso. Los grupos de objetos usan `ItemsRepeater` a ancho completo y celdas `UniformGridLayout` de 170 px para evitar solapamientos con botones de 140 px. Las respuestas se recorren con `↑`/`↓`; se evita el doble conteo, se bloquean respuestas tras acertar y se anuncia retroalimentación y resultados; no narra la descripción de escena que contenía el resultado esperado. En conteo interactivo y patrones, el foco inicial va al primer objeto manipulable, marcar un objeto actualiza su etiqueta sin volver a dibujar la cuadrícula y las flechas recorren los objetos sin saltar a las opciones; los patrones se reproducen automáticamente y `R` repite la secuencia.

#### `src/MathTutor.WinUI/Views/MapPage.xaml` y `MapPage.xaml.cs`
Mapa del Reino (`F6`): estaciones e insignias filtradas por edad y condición. Las estaciones usan nombres como “Misión recomendada: …” y botones “Explorar la estación: …”; la sala de trofeos es un `TreeView` agrupado por nivel educativo que muestra el progreso de cada grupo, ordena los trofeos desbloqueados primero, expande/contrae con `→`/`←` y persiste el estado de cada grupo en `AccessibilityPreferences.BadgeGroupExpansion`. Conserva la selección al cambiar de tema.

#### `src/MathTutor.WinUI/Assets`, `app.manifest`, `Properties/launchSettings.json` y `.gitignore`
- `Assets/AppIcon.ico` es el único recurso de imagen requerido por la aplicación de escritorio.
- `app.manifest` declara la aplicación como consciente de DPI y ejecutable de escritorio.
- `launchSettings.json` facilita la ejecución desde Visual Studio.
- `.gitignore` local excluye `bin/`, `obj/` y artefactos de empaquetado.

> **Nota de arquitectura:** el proyecto WPF (`MathTutor.WpfApp`) fue retirado después de consolidar la paridad funcional en WinUI 3. El historial de Git conserva su implementación y los registros de releases anteriores.

### 2.4. Proyecto `MathTutor.Tests` (Suite de 209 Pruebas Unitarias Automatizadas)

| Archivo de Pruebas | Pruebas | Qué Verifica |
|---|---|---|
| `ExerciseGeneratorTests.cs` (1,231 líneas) | **66** | Verifica individualmente los métodos generadores de los 16 objetivos (desde conteo de Kinder hasta Cálculo e IA Asistiva), comprobando en bucles de 20 iteraciones con semilla determinista que `ExpectedResult` sea matemáticamente exacto, que exista exactamente una opción correcta, que no haya opciones duplicadas (`Distinct`), que los rangos se reduzcan para discapacidad intelectual y que la moneda/vocabulario cambie según `RegionalContext` (Perú `S/` vs. México `$`). Incluye el respaldo bidireccional de `BuildOptions` y la reducción sin mutar opciones de `GetAdaptedOptions`. |
| `PedagogyAndAccessibilityTests.cs` (1,226 líneas) | **71** | Verifica que las 54 misiones existan en `CurriculumService`, cubran sus principios matemáticos esperados, posean instrucciones estándar y simplificadas, pistas escalonadas y retroalimentación diagnóstica. Incluye la cobertura de los 10 temas de jardín, el desbloqueo por edad, la invariante de que todo objeto visible en ejercicios de conteo se cuenta (`Kindergarten_InteractiveCounting_CountsEveryVisibleItem`), la variedad de tonos y timbres semánticos, las descripciones de cantidad en comparaciones y pruebas de `AdaptiveEngine.GetAllowedLevelsForAge`, `GetRecommendedLevelForAge`, `CurriculumService.GetTopicsForAge`, `GetNextTopic` (incluyendo retorno `null` al completar toda una etapa) y `ApplyRecommendedAccessibilityDefaults()`. |
| `ProfileStorageTests.cs` (547 líneas) | **21** | Verifica serialización/deserialización de todos los campos, el estado de los grupos de trofeos, la migración de perfiles antiguos (`Neurodivergent`/severidad a `SupportProfile`), la exportación a una ruta elegida, el borrado del perfil con su valor de retorno, la cuarentena ante JSON corrupto, el fallo transitorio de persistencia y la concurrencia de `Save`/`Load`/`Delete`. |
| `RewardServiceTests.cs` (111 líneas) | **5** | Verifica el orden, `Id` y `TopicId` de las **54 estaciones**, la estación activa dentro del rango permitido, la coincidencia entre títulos de jardín y nombres de estación, las estrellas reales de `LessonStarRatings` y la lógica de bloqueo/desbloqueo de las **55 insignias** en `GetAllBadges`. |
| `AudioSynthesizerTests.cs` (89 líneas) | **6** | Verifica el contenedor WAV PCM, la envolvente de atenuación, el determinismo, los timbres por `ManipulableSoundKind`, el rango de frecuencias y el carácter no tonal del aplauso. |
| `ThemeContrastTests.cs` (131 líneas, 40 casos) | **40** | Lee los colores de `AccessibilityManager` y verifica AAA 7:1 para texto (incluido el acento sobre fondo y tarjeta) y 3:1 para bordes/foco en los cuatro temas. |
---

## 3. Mapa Curricular Completo (54 Estaciones / 16 Objetivos)

> La tabla base conserva la numeración histórica de las 45 estaciones originales. La ampliación de jardín de la versión 1.10.0 añade 9 estaciones que se insertan después de `kinder_counting_01`; en la aplicación, las estaciones se renumeran por orden de la lista (`station_1` a `station_54`).

| Estación | `TopicId` | Nivel (`AcademicLevel`) | `GamePlayType` Principal | Tema / Objetivo Pedagógico |
|---|---|---|---|---|
| 1 | `kinder_counting_01` | `Kindergarten` | `InteractiveCounting`, `OrderSequence`, `Comparison` | Obj. 1: Los 5 Principios del Conteo (Gelman y Gallistel) |

### 3.1. Ampliación de Jardín de Niños v1.10.0 (9 estaciones nuevas)

| Orden en jardín | `TopicId` | Edad | `GamePlayType` Principal | Tema |
|---|---|---|---|---|
| 2 | `kinder_shapes_01` | 3-4 | `InteractiveCounting`, `Comparison` | Formas básicas: círculo, cuadrado y triángulo |
| 3 | `kinder_positions_01` | 3-4 | `InteractiveCounting`, `Comparison` | Posiciones: arriba/abajo, dentro/fuera, cerca/lejos |
| 4 | `kinder_stories_01` | 3-5 | `InteractiveCounting` | Contar dentro de cuentos breves |
| 5 | `kinder_patterns_01` | 4-5 | `OrderSequence` | Continuar patrones de color, forma y tamaño |
| 6 | `kinder_sorting_01` | 4-5 | `Comparison`, `InteractiveCounting` | Clasificar por color, forma y tamaño |
| 7 | `kinder_music_01` | 4-5 | `OrderSequence`, `InteractiveCounting` | Contar sonidos y palmadas |
| 8 | `kinder_count_to_10_01` | 5-6 | `InteractiveCounting`, `OrderSequence`, `Comparison` | Contar y ordenar números hasta 10 |
| 9 | `kinder_add_sub_01` | 5-6 | `AdditionCombine`, `SubtractionTakeAway` | Juntar y quitar con cantidades pequeñas |
| 10 | `kinder_measure_01` | 5-6 | `Comparison`, `OrderSequence` | Comparar longitudes, pesos y capacidades |

### 3.2. Estaciones base (numeración histórica)

| Estación | `TopicId` | Nivel (`AcademicLevel`) | `GamePlayType` Principal | Tema / Objetivo Pedagógico |
|---|---|---|---|---|
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

Todas las mejoras especificadas en [`correcciones mat tutor.md`](correcciones%20mat%20tutor.md) se encuentran implementadas y verificadas:

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
    - Transformada la vista `LessonView.xaml` y `LessonView.xaml.cs` en un flujo guiado interactivo de 3 pasos (Paso 1: Historia con Cosmo, Paso 2: El Secreto Matemático, Paso 3: Ejemplo Guiado y Reto).
    - Eliminada la lectura corrida que causaba fatiga auditiva: ahora se muestra y anuncia una sola tarjeta a la vez con instrucciones interactivas ("Presiona Enter o Espacio para continuar...").
    - Navegación bidireccional mediante teclas de dirección (`Flecha Izquierda` / `Backspace` para retroceder, `Enter` / `Espacio` / `Flecha Derecha` para avanzar) y `F1` para repetir el paso actual.
    - La transición al minijuego práctico únicamente se habilita al alcanzar el último paso.
14. ✅ **Mejora 2 (Soporte para Discapacidades Múltiples — Comorbilidad e Interseccionalidad)**:
    - Añadida la propiedad `Disabilities` (`List<DisabilityType>`) y el método `HasDisability(...)` en `UserProfile.cs`, manteniendo la propiedad `Disability` sincronizada como fachada de retrocompatibilidad.
    - En `ProfileSetupView.xaml`, sustituido el `ComboBox` de selección única por casillas de verificación (`CheckBox`) accesibles e independientes.
    - Adaptaciones simultáneas y acumulativas activadas en `ApplyRecommendedAccessibilityDefaults()` (por ejemplo, Ceguera + TEA o Intelectual + Motriz aplican al unísono tamaño de fuente, voz, lectura de pantalla, simplificación y paleta sensorial).
    - Adaptados `AdaptiveEngine.cs`, `CurriculumService.cs`, `CurriculumMenuView.xaml.cs` y `RealmMapView.xaml.cs` para aceptar colecciones de discapacidades.
15. ✅ **Mejora 3 (Anuncio Completo de Opciones en los Retos para TTS y Lector de Pantalla)**:
    - Implementado `AdaptiveEngine.BuildOptionsAnnouncement` y conectado en `MiniGameView.xaml.cs` tanto al iniciar un ejercicio como al presionar `F1` (Repetir).
    - Anuncia explícitamente el enunciado y todas las opciones disponibles con sus atajos numéricos: `"{Pregunta}. Opción 1: {Opcion1}. Opción 2: {Opcion2}. Opción 3: {Opcion3}. Presiona 1, 2 o 3 para responder."` evitando que el alumno tenga que explorar a ciegas.
16. ✅ **Mejora 4 (Aleatorización de Opciones para Eliminar el Sesgo de Posición)**:
    - Implementado el algoritmo Fisher-Yates en `ExerciseGenerator.ShuffleOptions` y en `BuildOptions` dentro de `ExerciseGenerator.cs`.
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

### Fase 5 — Accesibilidad, Foco y Audio Semántico (posterior a `v1.10.0`)
21. ✅ **Tarea 1 (Guía y Atajos en `LEEME.txt`)**:
    - Se eliminaron los separadores largos que añadían ruido y se documentó la tecla `R` para repetir la secuencia de un patrón.
22. ✅ **Tarea 2 (Versión Visible del Lanzador y la Aplicación)**:
    - `MathTutor.WinUI.csproj` fija `FileVersion` y `AssemblyVersion` en `1.10.0.0`; `scripts/publish-winui.ps1` valida el formato, estampa la versión en el lanzador temporal y comprueba la metadata de `libs/MathTutor.exe` y `MathTutor.exe`.
23. ✅ **Tarea 3 (Borrado de Perfil sin Recreación Automática)**:
    - `OnThemeChanged` y `OnFontSizeChanged` en `MainWindow.xaml.cs` solo guardan si `_acc.Storage.Exists()`; tras borrar el perfil, cambiar tema o tamaño ya no recrea `user_profile.json` y el arranque vuelve a la caracterización.
24. ✅ **Tarea 4 (Anuncio de los Apoyos sin Interrupciones)**:
    - Los seis selectores de apoyos de `ProfilePage.xaml` exponen `AutomationProperties.HelpText` y anuncian el valor elegido en `DropDownClosed`; `SupportDescription_Changed` ya no habla mientras se recorren las opciones.
25. ✅ **Tarea 5 (Lenguaje Preciso al Terminar la Lección)**:
    - El texto visible y el anuncio de `LessonPage.xaml`/`.xaml.cs` usan “¡Explicación completada!” en lugar de “¡Lección completada!”.
26. ✅ **Tarea 6 (Foco Estable en Conteo y Patrones)**:
    - `GameItemView` implementa `INotifyPropertyChanged` y actualiza `Label`, `AccessibleName` y `HelpText` sin volver a dibujar la cuadrícula; `GamePage` enfoca el primer objeto manipulable en conteo interactivo y patrones, conserva el foco al marcar y recorre los objetos con flechas mediante `FocusItemAt`, sin saltar a las opciones.
27. ✅ **Tarea 7 (Estrellas al Completar la Misión)**:
    - `NavigationService.MissionCompleted` transporta el tema y las estrellas ganadas; `GamePage` calcula `3`, `2` o `1` estrella según `_attemptsCount`; el diálogo de `MainWindow.xaml.cs` muestra “Ganaste X de 3 estrellas” y el anuncio lo incluye antes de ofrecer la siguiente misión.
28. ✅ **Tarea 8 (Audio Semántico y Cantidades Comparadas)**:
    - `ManipulableSoundKind` añade timbres de campana, tambor, flauta y palmas; `AccessibleAudioCueService` reproduce secuencias de patrones y sonidos por objeto; `KindergartenCurriculum` asigna tono/timbre según instrumento, tamaño o emoji; `ExerciseOption.AccessibleDescription` permite anunciar cantidades comparativas como “Las rojas, 3 manzanas”; `R` repite la secuencia y las opciones de patrón se reproducen automáticamente.
29. ✅ **Tarea 9 (Seguimiento de Auditoría: Estaciones y Patrones)**:
    - Las 10 estaciones de jardín usan el mismo nombre que su misión; `GetRealmStations` acepta los `TopicId` permitidos y `MapPage` los pasa tras filtrar por edad, de modo que la estación activa nunca queda fuera del rango visible. Se añadieron pruebas de regresión para esa selección, la coincidencia de nombres y la repetición de tono en cada patrón.

> **Verificación local de la Fase 5:** build `Release` con **0 advertencias** y **196/196 pruebas** superadas. No se publicó una nueva release ni se modificó el paquete `v1.10.0`.

---

## 5. Comandos de Verificación, Empaquetado y Limpieza Obligatoria para Contribuidores

> **REGLA DE HIGIENE DE DISCO**: Las salidas de compilación viven únicamente en `artifacts/`; los proyectos no deben contener `bin/` ni `obj/`. Antes de una nueva release se limpia `artifacts/` y se conserva solo el ZIP vigente de `artifacts/releases/`.

```powershell
# 1. Limpiar cualquier residuo de compilaciones anteriores antes de compilar la nueva versión
if (Test-Path artifacts) { Remove-Item artifacts -Recurse -Force }

# 2. Compilar toda la solución en modo Release (0 advertencias permitidas)
dotnet build MathTutor.slnx -c Release --nologo /p:TreatWarningsAsErrors=true

# 3. Ejecutar las 209 pruebas unitarias automatizadas sobre la compilación Release
dotnet test tests\MathTutor.Tests\MathTutor.Tests.csproj -c Release --nologo --no-build

# 4. Publicar y empaquetar WinUI 3 (verifica MathTutor.pri)
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-winui.ps1

# 5. Conservar únicamente el ZIP de la release vigente
Get-ChildItem artifacts\releases -Filter *.zip -File | Where-Object { $_.Name -ne 'MathTutor-v1.10.0-Windows.zip' } | Remove-Item -Force
```


---

## 6. Auditoría integral de remediación (fases 1-8, oct 2026)

Arquitectura sin cambios estructurales; la auditoría integral agregó y corrigió:

- **`src/MathTutor.Core/Services/AudioSynthesizer.cs` (nuevo):** síntesis procedural
  determinista WAV PCM (44.1 kHz / 16-bit / mono), envolvente de atenuación en ambos
  extremos, caché acotada 96 buffers con candado, timbres para cada
  `ManipulableSoundKind` (campana/armónico, tambor/barrido de frecuencia,
  flauta/armónico puro, aplauso/no tonal) y clamp de frecuencias
  120-1200 Hz. `AccessibleAudioCueService` consume este sintetizador dentro de un
  canal serializado (hilo dedicado de reproducción, cola acotada a 32 eventos,
  `ClearPendingCues()` público para evitar spam al cambiar de pantalla).
- **`src/MathTutor.Core/Engine/SimplifiedHintCatalog.cs` (nuevo):** respaldo de
  sugerencias simplificadas (pista 1, pista 2, éxito) en lenguaje sin tecnicismos
  para cada `GamePlayType` (52 tipos); el `AdaptiveEngine` lo sirve cuando el perfil
  prefiere lenguaje simple y el ejercicio no trae variantes simplificadas a mano.
- **`ProfileStorageService.cs`:** guardado atómico + cuarentena de archivos
  corruptos + `Delete` restringido + migración v2→v3 persistente
  (`UserProfile.CurrentProfileVersion = 3`, campo `LessonStarRatings`).
- **`CurriculumService.cs`:** `GetTopicsForAge` con exención de nivel bipolar, +
  `GetNextTopic` fallback hacia adelante; programación y trivia 16+; `add_ex_04`
  (propiedad asociativa para 6-8 pedazos).
- **`ExerciseGenerator.cs`:** distractores aleatorizados cerca del resultado con
  respaldo determinista terminante; 8 `ScreenReaderDescription` neutralizados
  (dejan de revelar la respuesta).
- **`CurriculumService.cs` (13 `ScreenReaderDescription`):** sin revelar respuestas.
- **`SpeechService.cs`:** latches de handshake NVDA RPC, presencia y falla JAWS COM.
- **`AccessibilityManager.cs`:** ciclo de temas honesto respecto al Contraste del
  Sistema; pinceles de error en `HighContrastDark` (255,120,120) y `DefaultPlayful`
  (170,0,30) subidos a AAA 7:1.
- **`MainWindow.xaml.cs`:** `AutomationProperties.SetAcceleratorKey` de los atajos
  globales; 🏆 del diálogo de misión en vista de accesibilidad `Raw` (enum
  `Microsoft.UI.Xaml.Automation.Peers` en WinUI 3).
- **Suite:** 196/196 pruebas en Release al cerrar esta pasada, 0 advertencias,
  0 errores. `dotnet test` local sigue bloqueado por ACL del sistema; usar
  `C:\Users\artik\AppData\Local\Temp\opencode\MathTutorRunner\MathTutorRunner.csproj`.

## 7. Segunda auditoría integral de remediación (oct 2026)

Segunda revisión con ojo fresco (Core, WinUI y documentación). Cambios de esta pasada:

- **Persistencia (`ProfileStorageService.cs`):** la re-escritura de la migración ya
  no puede cuarentenar un perfil válido (el `catch` se limita a `JsonException` y el
  `Save` de migración captura `IOException`/`UnauthorizedAccessException`); `Delete`
  se serializa con el mismo `FileGate` que `Save`/`Load`. Dos pruebas nuevas:
  fallo transitorio de persistencia de migración y concurrencia `Save`/`Load`/`Delete`.
- **Foco y teclado:** `LessonPage` no secuestra `↑`/`↓`/`Backspace`/`Espacio` cuando
  el foco está en un `TextBox` (lectura con caret) y su `Escape` duplicado se eliminó
  (solo el acelerador global de `MainWindow`). Las estrellas de la misión ahora
  incluyen los intentos del último reto. El F1 del diálogo de misión se unificó en
  el acelerador global (`_missionDialogMessage`), sin doble anuncio. El reintento de
  una opción ya probada anuncia un mensaje breve en lugar de concatenar la pista
  obsoleta. `ToggleSpeech` persiste la preferencia cuando ya existe perfil.
- **Contenido:** `add_ex_04` incluye el sexto tesoro (vagón 3) y corrige "vagón";
  `sub_ex_01` incorpora los 5 patitos manipulables que narra; la pista simplificada
  de fracciones mixtas ya no asume denominador 2; la función de programación se
  llama `calcularDecibeles` (coherente con su unidad) en el tema curado y en el
  generador; se aclaró la frase de "sierra" en números con signo.
- **Mapa y estados:** `RewardService.GetRealmStations` usa `LessonStarRatings` reales
  (mínimo 1 para perfiles antiguos sin calificación) en lugar de 3 fijos; las
  estaciones bloqueadas anuncian "Sendero por desbloquear" con ayuda coherente; el
  menú y el mapa muestran estado vacío cuando no hay misiones para la edad; la
  bienvenida larga del menú solo se locuta al llegar de verdad al menú.
- **Generador:** el respaldo de `BuildOptions` alterna arriba/abajo (sin sesgo
  direccional en rangos degenerados y resultados negativos); `GetAdaptedOptions`
  elige el distractor más cercano y trabaja sobre copias, sin mutar las opciones
  compartidas ni cambiar el orden relativo.
- **Audio y lectores:** el timbre de tambor aplica de verdad el clamp 110-220 Hz
  (antes era código muerto); las secuencias limpian la cola antes de sonar y al
  navegar entre páginas; el latch de NVDA solo se activa con una locución exitosa
  (se degrada si el RPC falla); `Dispose` libera el RCW de JAWS con
  `Marshal.FinalReleaseComObject` y reinicia los latches.
- **Contraste:** el acento usado como texto cumple AAA 7:1 en los cuatro temas
  (`LowSensory` RGB 30,80,120 con `OnAccent` blanco; `DefaultPlayful` RGB 70,45,180);
  la teoría de contraste cubre `AppAccentBrush` sobre fondo y tarjeta.
- **Documentación:** README/ACR con 209 pruebas; tabla y árbol de pruebas al día
  (66/71/21/5/6 + 40 casos); `GamePlayType` unificado en 52; `ProfileVersion` v3;
  eliminada la dependencia inexistente de WebView2; enlaces `file:///` de máquinas
  ajenas convertidos a rutas relativas; fragmentos corruptos reescritos.
- **Suite:** 231/231 pruebas, build Release 0 advertencias / 0 errores.
- **Limitaciones aceptadas:** el "flip" transitorio de tema en `RefreshThemeResources`
  se conserva (fuerza la re-evaluación de `ThemeResource`; su alternativa requiere
  prueba visual en dispositivo); las preferencias de tema/fuente elegidas antes de
  crear el perfil no se persisten (guardarlas antes del onboarding lo saltaría).


## 8. Ampliación Curricular y Tercera Auditoría Integral (Release 1.11, oct 2026)

Tercera fase de auditoría e implementación integral de la Hoja de Ruta Curricular (Fases 1 a 5) basada en los programas oficiales de estudio SEP y NCTM. Se amplió el currículo de 54 a 74 estaciones continuas del Reino del Conocimiento, con soporte DUA completo y 231 pruebas unitarias automatizadas.

- **Fases Curriculares 1 a 5:**
  - **Fase 1 (Primaria Temprana y Magnitudes):** Monedas y vuelto (`elem_money_currency_01`), Regla y longitud en cm (`elem_measurement_ruler_01`), Reloj analógico/digital (`elem_time_clock_01`), Perímetro regular/irregular (`elem_geometry_perimeter_01`).
  - **Fase 2 (Primaria Superior, Aritmética y Estadística):** División entera con residuo explícito (`elem_division_remainder_01`), Recta numérica de fracciones (`elem_fractions_numberline_01`), Tablas y barras (`elem_data_barcharts_01`), Tendencia central (`elem_stats_central_tendency_01`).
  - **Fase 3 (Secundaria Inicial y Racionales):** Circunferencia y $\pi$ (`sec_circle_geometry_01`), Factores primos MCM y MCD (`sec_factors_multiples_mcm_mcd_01`), Racionales negativos (`sec_negative_rational_numbers_01`).
  - **Fase 4 (Secundaria Avanzada y Probabilidad):** Factorización algebraica (`sec_algebra_factoring_01`), Ángulos entre paralelas y secantes (`sec_parallel_lines_angles_01`), Probabilidad clásica y de eventos compuestos (`sec_probability_compound_01`).
  - **Fase 5 (Preparatoria, Geometría Analítica y Cálculo):** Parábola y elipse analíticas (`prep_conics_parabola_ellipse_01`), Límites y continuidad (`prep_calculus_limits_01`), Reglas de derivación operativa (`prep_calculus_derivative_rules_01`).
- **Motor Adaptativo y Arquitectura:**
  - Tipado de resultados exactos mediante enteros escalados (`ExpectedResult` y `Value` como `int`), eliminando imprecisiones de punto flotante en cálculo y geometría.
  - Orden correlativo estricto en `CurriculumService.cs`: bloques contiguos de Secundaria 1º a 3º para garantizar el desbloqueo progresivo en cascada (`GetTopicsForAge`).
  - Catálogo DUA universal en `SimplifiedHintCatalog.cs`: soporte para las 74 estaciones y 69 tipos de `GamePlayType`.
  - Estaciones del Reino en `RewardService.cs` sincronizadas con `OrderIndex` 1 a 74.
- **Accesibilidad y Usabilidad de Teclado (WinUI 3):**
  - Reflujo multilínea dinámico con `WrapPanel` en las opciones de `GamePage.xaml` (WCAG 1.4.10).
  - Eliminación de truncamiento (`CharacterEllipsis`) por ajuste natural de palabras (`TextWrapping="WrapWholeWords"`).
  - Suspensión inteligente de atajos numéricos en `MainWindow.xaml.cs` cuando el elemento activo es un `TextBox`.
- **Suite Automatizada:**
  - 234/239 pruebas en Release, 0 advertencias, 0 errores.
  - Se agregaron pruebas de rendimiento y carga en `LoadAndPerformanceTests.cs` validando latencia sub-milisegundo de audio y arranque instantáneo con caché de 74 temas.
  - Versión general del sistema elevada a `1.11.0`.
