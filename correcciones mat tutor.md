# 🛠️ Especificación Técnica para Agentes de Código (Antigravity / OpenCode)
**Proyecto:** MathTutor (C# .NET 10 / WinUI 3)

**Objetivo:** Corrección de errores de accesibilidad WCAG 2.2 / DUA, usabilidad de teclado y reglas de negocio adaptativas.

**Nota histórica:** Las tareas y fases anteriores a `v1.6.0` describen la implementación original en WPF, retirada en `v1.7.0`. La aplicación vigente está en `src/MathTutor.WinUI`.

---

## 📌 Tarea 1: Foco Inicial Automático y Anuncio de Entrada al Iniciar
* **Componente / Archivo:** `MathTutor.WpfApp/MainWindow.xaml`, `MainWindow.xaml.cs` (vista principal inicial).
* **Comportamiento Actual:** Al abrir la aplicación, suena el audio de fondo, pero el foco no se posiciona en ningún elemento interactivo. Los lectores de pantalla (NVDA/JAWS/Narrador) no anuncian nada y las flechas no responden hasta presionar `Tab`.
* **Comportamiento Esperado:** Al finalizar el evento `Loaded`, el foco de teclado debe asignarse automáticamente al primer elemento interactivo y se debe emitir un anuncio de voz/UIA de bienvenida.
* **Instrucciones de Implementación:**
  1. En el manejador del evento `Loaded` de la ventana o vista inicial:
     ```csharp
     this.Loaded += (s, e) => 
     {
         // Asignar foco al primer botón u opción relevante
         PrimaryMenuButton.Focus();
         Keyboard.Focus(PrimaryMenuButton);

         // Disparar anuncio de accesibilidad
         AccessibilityManager.Instance.Announce("Bienvenido a MathTutor. Presiona Tab para navegar por las opciones.");
     };
     ```
  2. Verificar que `AutomationProperties.Name` del primer elemento lea claramente la instrucción de inicio.

---

## 📌 Tarea 2: Navegación Direccional (Flechas) y Maquetación de la Lista de Misiones
* **Componente / Archivo:** `MathTutor.WpfApp/Views/CurriculumMenuView.xaml`, `CurriculumMenuView.xaml.cs` (y `RealmMapView.xaml`).
* **Comportamiento Actual:** Las misiones se muestran en grupos pequeños de 2 o 3, requiriendo presionar la tecla `Tab` repetidamente. Las teclas de flecha no permiten cambiar de misión de manera fluida.
* **Comportamiento Esperado:** El listado de misiones debe comportarse como una lista accesible con navegación nativa por teclas de dirección (`Flecha Arriba` / `Flecha Abajo` / `Flecha Izquierda` / `Flecha Derecha`).
* **Instrucciones de Implementación:**
  1. Utilizar un `ListBox` o `ListView` WPF con `KeyboardNavigation.DirectionalNavigation="Cycle"` en lugar de contenedores estáticos o `ScrollViewer` aislados.
  2. Definir `ItemContainerStyle` para que la tarjeta seleccionada reciba el borde de foco de 4px (`FocusVisualStyle`).
  3. Asegurar que `IsSynchronizedWithCurrentItem="True"` para que mover el foco entre items con las flechas seleccione e informe la misión activa automáticamente.

---

## 📌 Tarea 3: Estandarización de Etiquetas Accesibles (`AutomationProperties`)
* **Componente / Archivo:** `MathTutor.WpfApp/Views/CurriculumMenuView.xaml` y `RealmMapView.xaml` (plantillas accesibles de misión y estación).
* **Comportamiento Actual:** El texto leído por los lectores es redundante y confuso (ejemplo: *"🚀 Entrar a Jugar: El Sendero del Retorno... 🧭 Misión Disponible para Explorar botón Presiona para iniciar esta lección lúdica"*).
* **Comportamiento Esperado:** El lector de pantalla debe enunciar un nombre corto y descriptivo del estado y título, dejando las instrucciones para el campo de ayuda.
* **Instrucciones de Implementación:**
  1. Refactorizar las propiedades de accesibilidad del botón o tarjeta:
     * **`AutomationProperties.Name`**: `"{Estado}: {NombreMision}"`  
       * *Ejemplo Disponible:* `"Misión disponible: El Sendero del Retorno: La Resta y sus Misterios"`
       * *Ejemplo Completada:* `"Misión completada: El Bosque Mágico: Los 5 Secretos del Conteo"`
     * **`AutomationProperties.HelpText`**: `"Presiona Enter o Espacio para comenzar."`
  2. Eliminar emojis o prefijos innecesarios de las cadenas que se envían a la API de lectura para evitar lecturas ruidosas.

---

## 📌 Tarea 4: Retroalimentación Sonora y Anuncio en Respuestas Incorrectas
* **Componente / Archivo:** `MathTutor.WpfApp/Views/MiniGameView.xaml.cs`, `MathTutor.WpfApp/Services/AccessibilityManager.cs`.
* **Comportamiento Actual:** Al seleccionar una opción incorrecta dentro de un ejercicio, el juego no emite ningún anuncio de voz para el lector de pantalla.
* **Comportamiento Esperado:** Ante una respuesta incorrecta, se debe notificar inmediatamente al usuario mediante voz y sonido diagnósticos sobre el error cometido y la orientación para corregirlo.
* **Instrucciones de Implementación:**
  1. En el método de evaluación de respuesta (`EvaluateAnswer` / `SubmitOption`):
     ```csharp
     if (!isCorrect)
     {
         string feedbackMessage = GetDiagnosticFeedback(userAnswer, expectedAnswer);
         
         // Anuncio asertivo prioritario
         AccessibilityService.AnnounceAssertive($"Incorrecto. {feedbackMessage}");
         
         // Efecto de audio procedural PCM
         AudioService.PlayErrorChord();
     }
     ```
  2. Garantizar que el método `AnnounceAssertive` use `LiveRegionChanged` o invocación directa a la API del lector con prioridad alta.

---

## 📌 Tarea 5: Desacoplamiento de la Síntesis Interna (`F2`) vs. Lectores de Pantalla Externos
* **Componente / Archivo:** `MathTutor.WpfApp/Services/AccessibilityManager.cs`, `MathTutor.WpfApp/Services/SpeechService.cs`.
* **Comportamiento Actual:** Desactivar la síntesis de voz del juego (tecla `F2`) interrumpe o apaga parcialmente los anuncios hacia NVDA, JAWS o Narrador.
* **Comportamiento Esperado:** La tecla `F2` debe conmutar **únicamente** la voz/efectos de audio internos del juego (Canal A). El canal de comunicación con los lectores de pantalla del sistema (Canal B) debe permanecer siempre activo.
* **Instrucciones de Implementación:**
  1. Separar el flag de configuración en dos canales independientes:
     * `bool IsInternalAudioEnabled` (Controlado por `F2`).
     * `bool IsScreenReaderIntegrationEnabled` (Siempre `true` por defecto).
  2. Actualizar la lógica de envío de anuncios:
     ```csharp
     public void Announce(string text)
     {
         // Canal B: SIEMPRE envía al lector de pantalla externo (NVDA/JAWS/UIA)
         ScreenReaderApi.Speak(text);

         // Canal A: Solo sintetiza con audio interno si F2 no lo ha desactivado
         if (IsInternalAudioEnabled)
         {
             InternalAudioSynthesizer.Speak(text);
         }
     }
     ```

---

## 📌 Tarea 6: Validación Adaptativa de Edad vs. Nivel Escolar y Misiones
* **Componente / Archivo:** `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.WpfApp/Views/ProfileSetupView.xaml.cs`.
* **Comportamiento Actual:** Un usuario de 3 o 5 años puede seleccionar o ver disponibles niveles avanzados como "Fundamentos de Programación y Algoritmos" o niveles de Secundaria/Preparatoria.
* **Comportamiento Esperado:** El nivel escolar y los objetivos de aprendizaje deben ajustarse automáticamente en función de la edad seleccionada en la caracterización.
* **Instrucciones de Implementación:**
  1. En `MathTutor.Core`, crear una regla de mapeo entre Edad y Niveles Escolares permitidos:
     ```csharp
     public static IEnumerable<SchoolLevel> GetAllowedLevelsForAge(int age) => age switch
     {
         >= 3 and <= 5 => new[] { SchoolLevel.JardinDeNinos },
         >= 6 and <= 12 => new[] { SchoolLevel.PrimariaBaja, SchoolLevel.PrimariaMedia, SchoolLevel.PrimariaAlta },
         >= 13 and <= 15 => new[] { SchoolLevel.Secundaria },
         >= 16 => Enum.GetValues<SchoolLevel>(),
         _ => new[] { SchoolLevel.JardinDeNinos }
     };
     ```
  2. En `ProfileViewModel`, cuando cambie el valor de `Edad`, actualizar dinámicamente la lista desplegable de `NivelEscolar` y filtrar las misiones recomendadas en el mapa del reino.

---

## 📌 Tarea 7: Diálogo de Transición al Completar una Misión
* **Componente / Archivo:** `MathTutor.WpfApp/Views/MissionCompletedDialog.xaml`, `MissionCompletedDialog.xaml.cs` y `MiniGameView.xaml.cs`.
* **Comportamiento Actual:** Al terminar un nivel, el usuario vuelve a la lista de misiones y debe navegar manualmente para buscar el siguiente nivel.
* **Comportamiento Esperado:** Al superar un nivel, debe aparecer un diálogo modal accesible que anuncie el logro y pregunte si se desea continuar a la siguiente misión o regresar al mapa.
* **Instrucciones de Implementación:**
  1. Crear la vista/diálogo `MissionCompletedDialog`.
  2. En el evento de finalización del nivel:
     * Abrir el diálogo modal.
     * Asignar el foco automáticamente al botón primario: **"Siguiente Misión: [Nombre]"** (`Key.Enter`).
     * Asignar el botón secundario: **"Volver al Mapa del Reino"** (`Key.Escape`).
  3. Emitir el anuncio de voz: *"¡Felicidades! Misión completada. Presiona Enter para jugar la siguiente misión o Escape para volver al mapa."*

---

# 🔍 Fase 2: Correcciones Derivadas de la Auditoría Integral (Tareas 8 a 12)

## 📌 Tarea 8: Prevención de Doble Refresco y Doble Anuncio al Navegar entre Vistas
* **Componente / Archivo:** `MathTutor.WpfApp/MainWindow.xaml.cs`, `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`, `MathTutor.WpfApp/Views/RealmMapView.xaml.cs`.
* **Comportamiento Detectado en Auditoría:** `CurriculumMenuView` y `RealmMapView` suscribían el evento `Loaded` para invocar `RefreshData()` / `RefreshMap()`, mientras que `MainWindow` también invocaba `RefreshData()` / `RefreshMap()` tras cada `NavigateTo(...)`. Al montarse el `UserControl` en el árbol visual, `RefreshData()` se ejecutaba dos veces seguidas, duplicando el anuncio de voz y el tono musical.
* **Comportamiento Esperado:** Cada transición hacia el menú principal o el Mapa del Reino debe refrescar los datos, posicionar el foco y emitir el anuncio de bienvenida **exactamente una sola vez**.
* **Instrucciones de Implementación:**
  1. Eliminar la suscripción automática a `Loaded` en `CurriculumMenuView` y `RealmMapView`, delegando la invocación única de `RefreshData()` y `RefreshMap()` al flujo explícito de navegación en `MainWindow`.

---

## 📌 Tarea 9: Alineación de Etiquetas XAML con el Desacoplamiento de Audio Interno (`F2`)
* **Componente / Archivo:** `MathTutor.WpfApp/MainWindow.xaml`, `MathTutor.WpfApp/Views/ProfileSetupView.xaml`.
* **Comportamiento Detectado en Auditoría:** Aunque la lógica de la Tarea 5 desacopló `F2` para conmutar exclusivamente el audio interno del juego (dejando el lector de pantalla siempre activo), las etiquetas XAML de `BtnToggleSpeech` (`"🔊 Lector (F2)"`) y `ChkVoiceSynthesis` aún mencionaban activar/desactivar los anuncios del lector de pantalla.
* **Comportamiento Esperado:** Las etiquetas visuales y `AutomationProperties.Name` deben indicar con total claridad que controlan los **sonidos internos del juego** y que el lector de pantalla (NVDA, JAWS o Narrador) permanece siempre activo.

---

## 📌 Tarea 10: Resolución de Fin de Etapa en `CurriculumService.GetNextTopic`
* **Componente / Archivo:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/MissionCompletedDialog.xaml.cs`.
* **Comportamiento Detectado en Auditoría:** Al completar todas las misiones disponibles dentro de un rango de edad (por ejemplo, `kinder_counting_01` entre los 3 y 5 años), `GetNextTopic` hacía fallback a `topics[0]`, sugiriendo repetir la misma misión como *"Siguiente Misión"* en lugar de notificar que se completaron todas las misiones de la etapa.
* **Comportamiento Esperado:** Cuando no exista una misión siguiente en la secuencia ni otra misión pendiente en el rango de edad del alumno, `GetNextTopic` debe devolver `null` para que `MissionCompletedDialog` felicite al alumno por completar toda su etapa y muestre **"Volver al Menú de Misiones"**.

---

## 📌 Tarea 11: Navegación por Teclado y Lectura Limpia en la Sala de Trofeos (`ItemsBadges`)
* **Componente / Archivo:** `MathTutor.WpfApp/Views/RealmMapView.xaml`, `MathTutor.WpfApp/Views/RealmMapView.xaml.cs`.
* **Comportamiento Detectado en Auditoría:** Mientras que la lista de estaciones (`ItemsStations`) era un `ListBox` navegable con flechas, la cuadrícula de las 46 insignias (`ItemsBadges`) seguía siendo un `ItemsControl` estático cuyos elementos no recibían foco de teclado, impidiendo que un alumno ciego inspeccionara sus trofeos ganados.
* **Comportamiento Esperado:** Convertir `ItemsBadges` en un `ListBox` accesible con `KeyboardNavigation.DirectionalNavigation="Cycle"`, `KeyboardNavigation.TabNavigation="Once"`, foco de 4px (`AccessibleFocusVisual`) y etiqueta limpia sin emojis `AutomationProperties.Name` (`"Trofeo desbloqueado: {Título}. {Descripción}"` o `"Trofeo bloqueado: {Título}. {Descripción}"`).

---

## 📌 Tarea 12: Flexibilidad Pedagógica en `GetAllowedLevelsForAge` (12 Años en 1º de Secundaria y Discapacidad Intelectual)
* **Componente / Archivo:** `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `src/MathTutor.Core/Engine/CurriculumService.cs`, vistas de perfil, menú y mapa.
* **Comportamiento Detectado en Auditoría:**
  1. Un alumno de 12 años que ya cursa `1º de Secundaria (12 a 13 años)` no veía disponible `SecondaryFirstGrade` en su selector.
  2. Un alumno de 13 a 15 años con `DisabilityType.Intellectual` solo veía misiones de Secundaria, sin poder repasar niveles base de Preescolar o Primaria si necesitaba refuerzo pedagógico.
* **Comportamiento Esperado:**
  1. Habilitar `SecondaryFirstGrade` también a partir de los 12 años junto con los niveles de Primaria.
  2. Añadir soporte para `DisabilityType` en `GetAllowedLevelsForAge(int age, DisabilityType disability = DisabilityType.None)` y `GetTopicsForAge(int age, DisabilityType disability = DisabilityType.None)` de modo que cuando `disability == DisabilityType.Intellectual` siempre se incluyan los niveles fundamentales (`Kindergarten`, `EarlyElementary`, `MiddleElementary`, `UpperElementary`) además de los niveles de su edad.
# INSTRUCCIONES DE ACCESIBILIDAD Y UI/UX PARA AGENTES DE CÓDIGO (MathTutor - Feedback de Usuario) 🦉✨

## Contexto del Proyecto
> **Nota histórica:** esta fase describe la implementación original en WPF. La versión vigente usa WinUI 3, con fuentes en `src/` y pruebas en `tests/`.

**MathTutor** es una aplicación en **C# (.NET 10 / WPF)** estructurada en tres proyectos:
- `MathTutor.Core`: Dominio, motor adaptativo, perfiles y generador procedimental.
- `MathTutor.WpfApp`: Interfaz XAML, servicios de accesibilidad (UIA / NVDA / JAWS) y audio.
- `MathTutor.Tests`: Suite de pruebas unitarias xUnit.

Se requiere implementar los siguientes ajustes de accesibilidad, lógica de juego y experiencia de usuario basados en la retroalimentación de pruebas de usuario.

---

## 🛠️ CORRECCIONES Y MEJORAS DE ACCESIBILIDAD Y TUTORIAL INTERACTIVO

### 1. Tutorial y Lección Interactiva Paso a Paso (Sin Lectura Continua "De Corrido")
- **Ubicación:** `MathTutor.WpfApp/Views/LessonView.xaml`, `MathTutor.WpfApp/Views/LessonView.xaml.cs` y `src/MathTutor.Core/Models/PedagogicalTopic.cs`.
- **Problema:** Las lecciones y tutoriales muestran y leen todo el contenido de un solo tirón (título, historia, situación, pasos y atajos). Esto genera fatiga auditiva, sobrecarga cognitiva y le resta interactividad al juego.
- **Instrucciones de Implementación:**
  1. Transformar el modelo de la lección/tutorial en un **flujo guiado paso a paso (Wizard/Stepper)** en `LessonView.xaml.cs`.
  2. Muestren y anuncien por voz **únicamente una tarjeta/paso a la vez**.
  3. En cada paso del tutorial, incluir una instrucción interactiva explícita (por ejemplo: *"Paso 1 de 3: {Explicación corta}. Presiona Enter o Espacio para continuar al siguiente paso."*).
  4. Permitir retroceder paso a paso con las flechas o la tecla `Backspace`.
  5. Una vez completados todos los pasos intermedios de la lección, habilitar la transición al reto o ejercicio práctico.

### 2. Soporte para Discapacidades Múltiples (Comorbilidad / Interseccionalidad)
- **Ubicación:** `src/MathTutor.Core/Models/UserProfile.cs`, `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.WpfApp/Views/ProfileSetupView.xaml` y `MathTutor.WpfApp/Views/ProfileSetupView.xaml.cs`.
- **Problema:** En la pantalla de caracterización solo se permite seleccionar una única discapacidad, impidiendo configurar perfiles con múltiples necesidades (por ejemplo, Ceguera + TEA).
- **Instrucciones de Implementación:**
  1. Modificar la propiedad `DisabilityType` en `UserProfile` para que admita selecciones múltiples (`List<DisabilityType>` o `[Flags] Enum`).
  2. En `ProfileSetupView.xaml`, reemplazar los controles de selección única por casillas de verificación (`CheckBox`) accesibles.
  3. Asegurar que las adaptaciones (anuncios para lector de pantalla, paleta de baja estimulación sensorial y simplificación de lenguaje) se activen de forma simultánea cuando el usuario marque más de una opción.

### 3. Anuncio Completo de Opciones en los Retos (TTS y Lector de Pantalla)
- **Ubicación:** `MathTutor.WpfApp/Views/MiniGameView.xaml`, `MathTutor.WpfApp/Views/MiniGameView.xaml.cs` y `src/MathTutor.Core/Engine/AdaptiveEngine.cs`.
- **Problema:** Al iniciar un reto, la síntesis/lector lee la pregunta y la instrucción ("Elige usando 1, 2 o 3"), pero omite leer el contenido de cada opción, obligando al usuario a navegar por la interfaz para conocer las respuestas.
- **Instrucciones de Implementación:**
  1. En el método de inicio de reto y en el comando asociado a la tecla `F1` (Repetir), construir la cadena del anuncio concatenando explícitamente las opciones:
     `"{Pregunta}. Opción 1: {Opcion1}. Opción 2: {Opcion2}. Opción 3: {Opcion3}. Presiona 1, 2 o 3 para responder."`

### 4. Aleatorización de Opciones para Eliminar el Sesgo de Posición
- **Ubicación:** `src/MathTutor.Core/Engine/ExerciseGenerator.cs` (Generador procedimental de retos).
- **Problema:** La respuesta correcta suele aparecer casi siempre en la opción 2 (tecla 2), haciendo que el juego se vuelva predecible.
- **Instrucciones de Implementación:**
  1. En la fase final de generación de cada reto en `Core`, aplicar un algoritmo de aleatorización (*Fisher-Yates shuffle* o `OrderBy(x => Guid.NewGuid())`) a la lista de opciones.
  2. Actualizar el índice de la respuesta correcta tras el barajado para que se distribuya equitativamente entre las posiciones 1, 2 y 3.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.3.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **112/112 pruebas unitarias** superadas. Consolidado en el commit `7f66162` y publicado como release `v1.3.0` (`https://github.com/RHcomunications/MathTutor/releases/tag/v1.3.0`).

### 1. Migración de la solución a .NET 10
- **Corrección:** Los tres proyectos (`MathTutor.Core`, `MathTutor.WpfApp`, `MathTutor.Tests`) abandonan `net8.0` y se unifican en `net10.0` / `net10.0-windows`.
- **Ubicación:** `src/MathTutor.Core/MathTutor.Core.csproj`, `MathTutor.WpfApp/MathTutor.WpfApp.csproj`, `tests/MathTutor.Tests/MathTutor.Tests.csproj`.
- **Criterio de verificación:** `dotnet build MathTutor.slnx -c Release /p:TreatWarningsAsErrors=true` → 0 advertencias / 0 errores.

### 2. Refactor de comorbilidad en los motores (`HasDisability`)
- **Corrección:** Sustituidas las comparaciones de discapacidad única (`profile.Disability == DisabilityType.Intellectual`) por el predicado de comorbilidad `profile.HasDisability(DisabilityType.Intellectual)`, de modo que las adaptaciones (rango numérico reducido, lenguaje simplificado y reducción de opciones) también se apliquen cuando la discapacidad intelectual coexiste con otras en `UserProfile.Disabilities`.
- **Ubicación:** `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `src/MathTutor.Core/Engine/CurriculumService.cs`, `src/MathTutor.Core/Engine/ExerciseGenerator.cs`.
- **Criterio de verificación:** Suite de 112 pruebas (incluye `UserProfile_SupportsMultipleDisabilities_AndCombinesAdaptationsSimultaneously`).

### 3. Limpieza de variable sin uso en la lección
- **Corrección:** Eliminada la variable local `profile` no utilizada dentro de `AnnounceCurrentStep`.
- **Ubicación:** `MathTutor.WpfApp/Views/LessonView.xaml.cs`.
- **Criterio de verificación:** Compilación sin advertencias (`TreatWarningsAsErrors`).

### 4. Sincronización de la documentación con el estado real
- **Corrección:** Cuadrados los conteos y la versión en la documentación: `README.md` (106 → **112** pruebas; release `v1.1.0` → **`v1.3.0`**) y `context.md` (`MathTutor.Tests` `net8.0` → **`net10.0`**; suite 108/varios → **112**; diagrama 62/46 → **59/49**; y los conteos de líneas archivo por archivo).
- **Ubicación:** `README.md`, `context.md`.
- **Criterio de verificación:** Sin coincidencias residuales de `net8.0` (en `MathTutor.Tests`), `106 pruebas` ni `v1.1.0`.

---

## 📌 BACKLOG Y MEJORAS DE UX / MULTIMEDIA

- **Optimización de UI/UX:** Simplificar la navegación general de la interfaz para reducir la curva de aprendizaje.
- **Voces e Interactividad para Infancia Temprana:** Evaluar a futuro la inclusión de voces caracterizadas y narrativas más interactivas para el nivel de Jardín de Niños.
# AJUSTES DE ACCESIBILIDAD Y CORRECCIÓN DE BUGS EN MATHTUTOR (Fase de Pruebas de Campo) 🦉✨

## Contexto
Durante las pruebas de usuario en **MathTutor** (C# / .NET 10 WPF), se han identificado 4 comportamientos críticos en la navegación por teclado, el flujo de misiones y la aleatorización de los ejercicios. Se requiere aplicar las siguientes soluciones en la arquitectura del proyecto.

---

### 1. Captura Global de Atajos Numéricos (`1`, `2`, `3`) en `MiniGameView`
- **Ubicación:** `MathTutor.WpfApp/Views/MiniGameView.xaml` y `MathTutor.WpfApp/Views/MiniGameView.xaml.cs`.
- **Problema:** Al iniciar un ejercicio, presionar las teclas `1`, `2` o `3` no selecciona las opciones de respuesta de inmediato. El usuario se ve obligado a presionar `Tab` repetidamente hasta alcanzar el botón de la opción.
- **Instrucciones de Solución:**
  1. En `MiniGameView.xaml.cs`, suscribirse al evento `PreviewKeyDown` de la vista o la ventana principal.
  2. Detectar las teclas de los números del teclado principal (`Key.D1`, `Key.D2`, `Key.D3`) y del teclado numérico (`Key.NumPad1`, `Key.NumPad2`, `Key.NumPad3`).
  3. Si el ejercicio está activo y no se ha bloqueado la respuesta (`!_answerLocked`), interceptar la tecla (`e.Handled = true`) e invocar directamente el método de selección de opción correspondiente (`SelectOptionByIndex(0)`, `SelectOptionByIndex(1)`, `SelectOptionByIndex(2)`).
  4. Asegurar que al cargar la vista (`FocusInitialElement`), el foco de teclado principal permanezca en la vista para recibir eventos sin necesidad de tabular.

---

### 2. Carga y Desbloqueo Completo de la Lista de Misiones
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/CurriculumMenuView.xaml` y `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`.
- **Problema:** La lista de misiones solo muestra 1 misión (`"1 de 1"`, El Bosque Mágico). Al completarla, no se desbloquean ni se muestran en el listado las misiones subsecuentes de la etapa (Jardín de Niños / Primaria).
- **Instrucciones de Solución:**
  1. En `CurriculumService.GetTopicsForAge`, revisar la consulta LINQ/filtro por `AcademicLevel` y asegurar que devuelva la colección completa de misiones asociadas al nivel y edad del usuario, y no un arreglo de un solo elemento.
  2. En `RealmMapView` y `CurriculumMenuView`, verificar que la colección vinculada a la UI (`ItemsTopics`) cargue todas las misiones retornadas por el servicio del currículo.
  3. Asegurar que al completar una misión y actualizar el perfil (`CompletedLessonIds`), el estado de la siguiente estación cambie a `Disponible` y permanezca visible en la lista.

---

### 3. Navegación Direccional por Flechas en `CurriculumMenuView`
- **Ubicación:** `MathTutor.WpfApp/Views/CurriculumMenuView.xaml` y `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`.
- **Problema:** Las flechas arriba y abajo no mueven la selección entre los elementos del listado de misiones.
- **Instrucciones de Solución:**
  1. En `CurriculumMenuView.xaml`, verificar que el `ListBox` (`ItemsTopics`) tenga configurada la propiedad `KeyboardNavigation.DirectionalNavigation="Cycle"`.
  2. En `CurriculumMenuView.xaml.cs`, dentro de `FocusInitialElement()`, forzar el foco directamente sobre el primer `ListBoxItem` del `ListBox` (`(ListBoxItem)ItemsTopics.ItemContainerGenerator.ContainerFromIndex(0)`), asegurando que el control procese las teclas de dirección nativas de WPF.

---

### 4. Barajado / Aleatorización Eficaz de las Opciones (`ExerciseGenerator`)
- **Ubicación:** `src/MathTutor.Core/Engine/ExerciseGenerator.cs`.
- **Problema:** Al reiniciar un ejercicio o volverlo a jugar, la respuesta correcta siempre queda posicionada en la Opción 2 (tecla `2`), haciendo el patrón predecible.
- **Instrucciones de Solución:**
  1. En el método `BuildOptions` (o al generar cada `Exercise`), aplicar un algoritmo de barajado (Fisher-Yates o `OrderBy(x => Random.Shared.Next())`) sobre la lista de opciones antes de asignarlas al ejercicio.
  2. Después del barajado, reasignar dinámicamente las propiedades de atajo `AccessibleKeyShortcut` (`"1"`, `"2"`, `"3"`) y las etiquetas visuales de cada opción para garantizar que la respuesta correcta varíe uniformemente entre las posiciones 1, 2 y 3 en cada intento.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.5.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **114/114 pruebas unitarias** superadas. Publicado como release `v1.5.0`.

### 1. Captura Global de Atajos Numéricos (`1`, `2`, `3`) en `MiniGameView`
- **Corrección:** Implementado `SelectOptionByIndex` y estabilizado el foco inicial con `DispatcherPriority.Loaded` en `MiniGameView.xaml.cs`. En `MainWindow.xaml.cs`, interceptación y delegación global de `Key.D1..D4` y `Key.NumPad1..NumPad4` hacia `_gameView` si está activo, permitiendo responder retos inmediatamente sin necesidad de presionar `Tab`.
- **Ubicación:** `MathTutor.WpfApp/Views/MiniGameView.xaml.cs`, `MathTutor.WpfApp/MainWindow.xaml.cs`.
- **Criterio de verificación:** Atajos `1`–`4` responden al instante al cargar cualquier reto.

### 2. Carga y Desbloqueo Continuo de la Lista de Misiones
- **Corrección:** Integrado el parámetro `completedLessonIds` en `CurriculumService.GetTopicsForAge` y `GetNextTopic`. Al completar las misiones base de Kínder (o del nivel actual), los alumnos de 5+ años desbloquean automáticamente las misiones de Primaria Inicial (`EarlyElementary`), permitiendo continuar el sendero del aprendizaje sin bloqueos en el currículo.
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`, `MathTutor.WpfApp/Views/RealmMapView.xaml.cs`.
- **Criterio de verificación:** Prueba unitaria `CurriculumService_UnlocksSubsequentStageMissions_WhenBaseStageCompleted`.

### 3. Navegación Direccional por Flechas en `CurriculumMenuView`
- **Corrección:** Añadido `FocusInitialElement()` y gestión asíncrona de generación de contenedores (`ItemContainerGenerator.StatusChanged`) para posicionar el foco físico directamente sobre el primer `ListBoxItem`, habilitando de inmediato la navegación nativa con flechas arriba/abajo.
- **Ubicación:** `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`.
- **Criterio de verificación:** Al ingresar al menú, presionar `Flecha Abajo` o `Flecha Arriba` mueve inmediatamente la selección entre las tarjetas de misión.

### 4. Barajado / Aleatorización Eficaz en Retos Curados
- **Corrección:** Aplicación de `ShuffleOptions` a todos los ejercicios retornados por `CurriculumService.GenerateAdaptiveExercises`, incluyendo el ejercicio modelo inicial de cada lección, garantizando la alternancia equitativa de la opción correcta en cualquier intento.
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`.
- **Criterio de verificación:** Prueba unitaria `CurriculumService_GenerateAdaptiveExercises_ShufflesCuratedFirstExerciseOptions`.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.6.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **114/114 pruebas unitarias** superadas. Publicado como release `v1.6.0`.

### 1. Migración funcional a WinUI 3
- **Corrección:** Añadido el proyecto `MathTutor.WinUI` con paridad de perfil, menú de misiones, mapa del reino, lecciones, minijuego, diálogo de misión y atajos globales. Tras publicar la release, WPF se retiró de la solución para dejar una sola interfaz mantenida.
- **Ubicación:** `src/MathTutor.WinUI/`, `MathTutor.slnx`.
- **Criterio de verificación:** Build `Release` 0/0 y flujo E2E completo (lección, 3 ejercicios, diálogo de misión y regreso al mapa).

### 2. Accesibilidad para lectores de pantalla
- **Corrección:** Eliminados los anuncios duplicados al retirar `LiveSetting` de la región viva y usar `RaiseNotificationEvent`; añadidos nombres accesibles contextuales a botones de misión/estación, encabezados, contadores y controles; corregidos contrastes de bordes, éxito y foco.
- **Ubicación:** `src/MathTutor.WinUI/MainWindow.xaml.cs`, `src/MathTutor.WinUI/Services/`, `src/MathTutor.WinUI/Views/`, `src/MathTutor.WinUI/App.xaml`.
- **Criterio de verificación:** Inspección UIA de nombres, encabezados, botones y contadores; smoke test de cambio de tamaño de letra y navegación por teclado.

### 3. Alto contraste del sistema y tamaños mínimos
- **Corrección:** Sincronización con `AccessibilitySettings.HighContrast`, conservación de la preferencia del usuario al desactivar el alto contraste y sustitución de `Height` fijos por `MinHeight` para evitar recortes con fuentes grandes.
- **Ubicación:** `src/MathTutor.WinUI/MainWindow.xaml.cs`, `src/MathTutor.WinUI/Services/AccessibilityManager.cs`, vistas XAML.
- **Criterio de verificación:** Compilación estricta y revisión de recursos/foco por tema.

### 4. Limpieza posterior a la release
- **Corrección:** Retirado `MathTutor.WpfApp` de la solución, del README y de la documentación técnica vigente; la suite y el dominio compartido permanecen sin cambios.
- **Ubicación:** `MathTutor.sln`, `MathTutor.slnx`, `README.md`, `context.md`.
- **Criterio de verificación:** Build estricto 0/0 y 114/114 pruebas tras eliminar el proyecto.

### 5. Repaso en el reto y sin fuga de respuesta
- **Corrección:** El reto ya no narra la descripción de escena que incluía el resultado esperado; en su lugar muestra un repaso visible del secreto matemático para releerlo sin abandonar la pantalla.
- **Ubicación:** `src/MathTutor.WinUI/Views/GamePage.xaml`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`, `context.md`.
- **Criterio de verificación:** Inspección UIA sin coincidencias de “Resultado esperado”, repaso legible presente y flujo E2E completo con 114/114 pruebas.

### 6. Salidas de compilación centralizadas
- **Corrección:** Añadido `Directory.Build.props` para que todos los proyectos escriban en `artifacts/` (`bin`, `obj`, `publish`, `releases`); los directorios de proyecto quedan libres de `bin/` y `obj/`.
- **Ubicación:** `Directory.Build.props`, `scripts/publish-winui.ps1`, `.gitignore`, `README.md`, `context.md`.
- **Criterio de verificación:** Build 0/0, 114/114 pruebas, publicación con `.pri`, ZIP en `artifacts/releases/` y ejecutable iniciado correctamente.

### 7. Estructura de fuentes `src/` y `tests/`
- **Corrección:** Movidos `MathTutor.Core` y `MathTutor.WinUI` a `src/`, y `MathTutor.Tests` a `tests/`; soluciones, referencias, script de publicación y documentación actualizados.
- **Ubicación:** `src/`, `tests/`, `MathTutor.sln`, `MathTutor.slnx`, `tests/MathTutor.Tests/MathTutor.Tests.csproj`, `README.md`, `context.md`.
- **Criterio de verificación:** Build 0/0, 114/114 pruebas y publicación self-contained con `.pri` desde la nueva ruta.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.7.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **114/114 pruebas unitarias** superadas. Publicado como release `v1.7.0` (`https://github.com/RHcomunications/MathTutor/releases/tag/v1.7.0`).

### 1. Consolidación definitiva en WinUI 3
- **Corrección:** Retirado `MathTutor.WpfApp`; la solución, la documentación y la publicación quedan enfocadas en una sola interfaz mantenida.
- **Ubicación:** `MathTutor.sln`, `MathTutor.slnx`, `README.md`, `context.md`.
- **Criterio de verificación:** Build estricto 0/0, 114/114 pruebas y arranque del paquete WinUI.

### 2. Estructura de repositorio y artefactos
- **Corrección:** Fuentes en `src/`, pruebas en `tests/` y salidas centralizadas en `artifacts/` mediante `Directory.Build.props`; publicación reproducible con `scripts/publish-winui.ps1`.
- **Ubicación:** `src/`, `tests/`, `Directory.Build.props`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** Los proyectos no generan `bin/` ni `obj/` propios; el ZIP release incluye `.pri` y la app inicia.

### 3. Retos sin fuga de respuesta
- **Corrección:** El anuncio del reto ya no incluye la descripción de escena que revelaba el resultado esperado; se añadió un repaso visible del secreto matemático en la misma pantalla.
- **Ubicación:** `src/MathTutor.WinUI/Views/GamePage.xaml`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`.
- **Criterio de verificación:** UIA sin “Resultado esperado”, repaso legible y flujo E2E completo.

### 4. Paquete de release fácil de usar
- **Corrección:** La publicación se organiza por carpetas; el ZIP contiene una carpeta `MathTutor-v1.7.0` con `MathTutor.exe` en la raíz, `LEEME.txt`, `libs/` con la aplicación y sus bibliotecas y `modules/` con los `.winmd`. El ejecutable de la raíz es un lanzador que inicia la aplicación real desde `libs/`.
- **Ubicación:** `scripts/publish-winui.ps1`, `scripts/launcher/MathTutorLauncher.cs`, `README.md`, `context.md`, `LEEME.txt`.
- **Criterio de verificación:** ZIP con lanzador, `libs/MathTutor.exe`, `libs/MathTutor.pri`, módulos, `LEEME.txt` y aplicación iniciada desde la carpeta extraída.

### 5. Guía para usuarios finales
- **Corrección:** Añadido `LEEME.txt` en lenguaje sencillo para familias, docentes y estudiantes, con instalación, primeros pasos, teclas, accesibilidad, privacidad y solución de problemas. Se integra en el ZIP y se adjunta como archivo independiente en la release.
- **Ubicación:** `LEEME.txt`, release `v1.7.0`.
- **Criterio de verificación:** Guía descargable, integrada en el paquete y sin terminología de programación.

---

# REGISTRO DE CORRECCIONES REALIZADAS - Release `v1.8.0`

> **Estado:** Implementado y verificado. Build `Release` sin advertencias y **114/114 pruebas unitarias** superadas. Publicado como release `v1.8.0` (`https://github.com/RHcomunications/MathTutor/releases/tag/v1.8.0`).

### 1. Actualización de versión
- **Corrección:** La versión de `MathTutor.Core`, `MathTutor.WinUI` y del paquete se actualizó a `1.8.0`.
- **Ubicación:** `src/MathTutor.Core/MathTutor.Core.csproj`, `src/MathTutor.WinUI/MathTutor.WinUI.csproj`, `LEEME.txt`, `README.md`.
- **Criterio de verificación:** El ZIP generado se llama `MathTutor-v1.8.0-Windows.zip` y la aplicación inicia desde la carpeta extraída.

### 2. Paquete por carpetas
- **Corrección:** Se conserva el paquete con `MathTutor.exe` en la raíz, `libs/` con la aplicación y sus bibliotecas, `modules/` con los `.winmd` y `LEEME.txt`. El ejecutable de la raíz es un lanzador que inicia `libs/MathTutor.exe`.
- **Ubicación:** `scripts/publish-winui.ps1`, `scripts/launcher/MathTutorLauncher.cs`.
- **Criterio de verificación:** ZIP con lanzador, `libs/MathTutor.exe`, `libs/MathTutor.pri`, módulos y aplicación iniciada correctamente.

---

# REGISTRO DE CORRECCIONES REALIZADAS - Release `v1.9.0`

> **Estado:** Implementado y verificado. Build `Release` sin advertencias y **116/116 pruebas unitarias** superadas. Publicado como release `v1.9.0` (`https://github.com/RHcomunications/MathTutor/releases/tag/v1.9.0`).

### 1. Menú unificado "Opciones del juego"
- **Corrección:** La barra superior pasó de `CommandBar` con botones sueltos a un `DropDownButton` con `MenuFlyout`, agrupando Menú, Mapa, Perfil y un submenú de accesibilidad (Contraste, Letra, Sonidos y Repetir). El menú se recorre con `↑`/`↓`, se elige con `Enter` y se cierra con `Esc`; los atajos F1–F6 se conservan.
- **Ubicación:** `src/MathTutor.WinUI/MainWindow.xaml`, `src/MathTutor.WinUI/MainWindow.xaml.cs`.
- **Criterio de verificación:** Menú operable solo con teclado, nombres accesibles en el botón y cada opción, sonido de navegación al enfocar y ausencia de la barra de desbordamiento anterior.

### 2. Sala de trofeos agrupada por nivel
- **Corrección:** La lista plana de insignias se reemplazó por un `TreeView` con un grupo por nivel educativo. Cada grupo muestra cuántos trofeos se desbloquearon, los desbloqueados aparecen primero, `→` expande, `←` contrae y el estado se guarda en `AccessibilityPreferences.BadgeGroupExpansion` para recordarlo entre sesiones.
- **Ubicación:** `src/MathTutor.WinUI/Views/MapPage.xaml`, `src/MathTutor.WinUI/Views/MapPage.xaml.cs`, `src/MathTutor.Core/Models/UserProfile.cs`.
- **Criterio de verificación:** Grupos y trofeos anunciados con su nombre y progreso (no como “TreeViewNode”), expansión/contracción persistente tras cerrar y abrir la aplicación, y perfil antiguo sin la propiedad cargado con estado vacío.

### 3. Navegación vertical consistente
- **Corrección:** Se añadió `VerticalListNavigation` para que las misiones y estaciones respondan explícitamente a `↑`/`↓`; las respuestas del reto se recorren con `↑`/`↓`; la lección avanza y retrocede con `↑`/`↓` y `Backspace`, dejando `←`/`→` para la navegación propia de cada control o del lector de pantalla.
- **Ubicación:** `src/MathTutor.WinUI/Navigation/VerticalListNavigation.cs`, `src/MathTutor.WinUI/Views/MenuPage.xaml.cs`, `src/MathTutor.WinUI/Views/MapPage.xaml.cs`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`, `src/MathTutor.WinUI/Views/LessonPage.xaml.cs`.
- **Criterio de verificación:** Foco visible sobre el elemento seleccionado, desplazamiento automático para mantenerlo a la vista y recorrido completo sin ratón.

### 4. Persistencia y pruebas
- **Corrección:** Se añadieron pruebas de guardado/carga del estado de expansión y de compatibilidad con perfiles anteriores. La suite pasó de 114 a 116 pruebas.
- **Ubicación:** `tests/MathTutor.Tests/ProfileStorageTests.cs`.
- **Criterio de verificación:** 116/116 pruebas superadas y build `Release` con `TreatWarningsAsErrors` sin advertencias.

### 5. Actualización de versión y paquete
- **Corrección:** La versión de `MathTutor.Core`, `MathTutor.WinUI`, `LEEME.txt` y `README.md` se actualizó a `1.9.0`. Se mantiene el paquete por carpetas con lanzador `MathTutor.exe`, `libs/`, `modules/` y `LEEME.txt`.
- **Ubicación:** `src/MathTutor.Core/MathTutor.Core.csproj`, `src/MathTutor.WinUI/MathTutor.WinUI.csproj`, `LEEME.txt`, `README.md`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** ZIP `MathTutor-v1.9.0-Windows.zip` con lanzador, aplicación, `.pri`, módulos y guía; aplicación iniciada desde la carpeta extraída.

---

# REGISTRO DE CORRECCIONES REALIZADAS - Release `v1.9.1`

> **Estado:** Implementado y verificado. Build `Release` sin advertencias y **116/116 pruebas unitarias** superadas. PR #2 de `metalalchemist` fusionada en `main` (merge `06b24c4`, fix `5bd2bb8`); release `v1.9.1` publicada.

### 1. Ventana al frente al iniciar
- **Corrección:** `MainWindow_Loaded` se desuscribe de `RootGrid.Loaded`, aplica el tamaño inicial, carga el perfil y la navegación, y encola una sola solicitud de primer plano con `AppWindow.MoveInZOrderAtTop`, `Activate` y `SetForegroundWindow` (P/Invoke de `user32.dll`), protegida por `_isClosing`.
- **Ubicación:** `src/MathTutor.WinUI/MainWindow.xaml.cs`.
- **Criterio de verificación:** La ventana aparece al frente una sola vez tras la carga; la solicitud no se repite al reactivar; build 0 warnings.

### 2. Objetos de conteo sin solapamientos
- **Corrección:** Los dos `ItemsRepeater` de conteo pasaron a `HorizontalAlignment="Stretch"` y sus celdas `UniformGridLayout` de `MinItemWidth=95` a `170`, por encima del `MinWidth=140` de los botones; los grupos aprovechan el ancho disponible y envuelven sin encimarse.
- **Ubicación:** `src/MathTutor.WinUI/Views/GamePage.xaml`.
- **Criterio de verificación:** XAML compila; los botones de ambos grupos conservan su ancho mínimo y no se solapan en el ancho de la tarjeta.

### 3. Versión y paquete
- **Corrección:** Versión de `MathTutor.Core`, `MathTutor.WinUI`, `README.md` y `LEEME.txt` actualizada a `1.9.1`; se mantiene el paquete con lanzador, `libs/`, `modules/` y guía.
- **Ubicación:** `src/MathTutor.Core/MathTutor.Core.csproj`, `src/MathTutor.WinUI/MathTutor.WinUI.csproj`, `README.md`, `LEEME.txt`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** ZIP `MathTutor-v1.9.1-Windows.zip` con lanzador, `libs/MathTutor.exe`, `MathTutor.pri`, 53 módulos, `LEEME.txt` y sin PDB.

---

# REGISTRO DE CORRECCIONES REALIZADAS - Release `v1.10.0`

> **Estado:** Implementado y verificado. Build `Release` sin advertencias y **134/134 pruebas unitarias** superadas.

### 1. Jardín de niños ampliado
- **Corrección:** Se añadieron 9 temas de jardín (formas, posiciones, cuentos, patrones, clasificación, música, números hasta 10, suma/resta y medida) con 5 ejercicios curados cada uno. Jardín pasa de 1 a 10 temas y el currículo de 45 a 54 estaciones.
- **Ubicación:** `src/MathTutor.Core/Engine/KindergartenCurriculum.cs`, `src/MathTutor.Core/Engine/CurriculumService.cs`, `src/MathTutor.Core/Services/RewardService.cs`.
- **Criterio de verificación:** 54 estaciones y 55 insignias ordenadas; temas filtrados por edad; desbloqueo de primaria solo al completar todos los temas disponibles de jardín.

### 2. Progresión y conteo
- **Corrección:** `GetNextTopic` omite temas ya completados y devuelve `null` al terminar la etapa. Los ejercicios de conteo ya no muestran objetos que no se cuentan: se quitaron los distractores y la clasificación usa grupos de comparación. Se añadió la invariante `Kindergarten_InteractiveCounting_CountsEveryVisibleItem`.
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `src/MathTutor.Core/Engine/KindergartenCurriculum.cs`, `tests/MathTutor.Tests/PedagogyAndAccessibilityTests.cs`.
- **Criterio de verificación:** 134/134 pruebas; el total visible coincide con `ExpectedResult` en todos los ejercicios de conteo.

### 3. Perfil de apoyos
- **Corrección:** El perfil describe apoyos en lugar de una severidad genérica: autismo niveles 1-3, TDAH por presentación, apoyos intelectuales intermitente/limitado/extenso/generalizado y descriptores funcionales de visión, audición y motricidad. Los perfiles antiguos se migran con `ProfileVersion = 2`. Las preferencias ajustadas a mano dejan de sobrescribirse.
- **Ubicación:** `src/MathTutor.Core/Models/Enums.cs`, `src/MathTutor.Core/Models/UserProfile.cs`, `src/MathTutor.Core/Services/ProfileStorageService.cs`, `src/MathTutor.WinUI/Views/ProfilePage.xaml` y `.xaml.cs`.
- **Criterio de verificación:** Perfiles antiguos con `Neurodivergent`/severidad cargan como autismo y TDAH; guardado, exportación y borrado cubiertos por pruebas.

### 4. Privacidad y documentación
- **Corrección:** Se añadió exportación del perfil a JSON, borrado del perfil local y lectura del aviso de privacidad desde la pantalla de perfil. Se incorporaron `PRIVACIDAD.md` y `BIBLIOGRAFIA.md` al repositorio y al paquete de release.
- **Ubicación:** `src/MathTutor.Core/Services/ProfileStorageService.cs`, `src/MathTutor.WinUI/Views/ProfilePage.xaml` y `.xaml.cs`, `PRIVACIDAD.md`, `BIBLIOGRAFIA.md`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** El ZIP contiene `LEEME.txt`, `PRIVACIDAD.md` y `BIBLIOGRAFIA.md`; la app exporta, borra y muestra el aviso sin conexión.

### 5. Versión y paquete
- **Corrección:** Versión actualizada a `1.10.0` en el proyecto WinUI, `README.md` y `LEEME.txt`.
- **Ubicación:** `src/MathTutor.WinUI/MathTutor.WinUI.csproj`, `README.md`, `LEEME.txt`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** ZIP `MathTutor-v1.10.0-Windows.zip` con lanzador, `libs/MathTutor.exe`, `MathTutor.pri`, módulos, guía, privacidad y bibliografía.
---

# FASE 4 - CALIDAD Y ACCESIBILIDAD (EN CURSO)

> **Estado:** automatizable implementado. La validación manual con NVDA/JAWS/Narrador, el ACR/VPAT firmado y la firma de código siguen pendientes.

### 1. Integración continua
- **Corrección:** Se añadió un workflow de GitHub Actions que restaura, compila en Release con `TreatWarningsAsErrors`, ejecuta las pruebas y permite empaquetar manualmente.
- **Ubicación:** `.github/workflows/ci.yml`.
- **Criterio de verificación:** El workflow ejecuta build, pruebas y publica el TRX; el paquete se genera solo en ejecución manual.

### 2. Dependencias fijadas
- **Corrección:** Se sustituyeron las versiones comodín de Windows App SDK, SDK BuildTools y BuildTools.WinApp por versiones exactas.
- **Ubicación:** `src/MathTutor.WinUI/MathTutor.WinUI.csproj`.
- **Criterio de verificación:** `dotnet restore` resuelve siempre las mismas versiones.

### 3. Licencia y avisos legales
- **Corrección:** Se añadió la licencia propietaria de Narayan Projects y los avisos de terceros. Ambos se incluyen en el paquete.
- **Ubicación:** `LICENSE`, `THIRD-PARTY-NOTICES.md`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** El ZIP contiene `LICENSE` y `THIRD-PARTY-NOTICES.md`.

### 4. Contraste automatizado
- **Corrección:** Se añadieron 32 casos de prueba que verifican 4.5:1 para texto y 3:1 para bordes/foco en los cuatro temas. Se corrigió el color de foco de los temas claro y de baja estimulación, que no alcanzaba 3:1.
- **Ubicación:** `tests/MathTutor.Tests/ThemeContrastTests.cs`, `src/MathTutor.WinUI/Services/AccessibilityManager.cs`.
- **Criterio de verificación:** 166/166 pruebas superadas.

### 5. ACR/VPAT y matriz manual
- **Corrección:** Se documentó un borrador ACR/VPAT con evidencia automatizada, estados por criterio y matriz NVDA/JAWS/Narrador.
- **Ubicación:** `ACCESIBILIDAD-ACR-VPAT.md`.
- **Criterio de verificación:** El documento distingue lo verificado de lo pendiente y no declara conformidad formal.

---

# FASE 5 - ACCESIBILIDAD, FOCO Y AUDIO SEMÁNTICO (IMPLEMENTADO LOCALMENTE)

> **Estado:** implementado y verificado localmente. Build `Release` sin advertencias y **170/170 pruebas** superadas. No se publicó una nueva release ni se reemplazó el paquete `v1.10.0`.

### 1. Guía y atajos
- **Corrección:** Se eliminaron los separadores largos de `LEEME.txt` y se documentó la tecla `R` para repetir la secuencia de un patrón.
- **Ubicación:** `LEEME.txt`.
- **Criterio de verificación:** La guía conserva secciones legibles y menciona `R` junto a los atajos de teclado.

### 2. Versión del lanzador y la aplicación
- **Corrección:** Se fijaron `FileVersion` y `AssemblyVersion` en `1.10.0.0`; el script de publicación valida la versión, la estampa en el lanzador temporal y verifica la metadata de ambos ejecutables.
- **Ubicación:** `src/MathTutor.WinUI/MathTutor.WinUI.csproj`, `scripts/publish-winui.ps1`.
- **Criterio de verificación:** `libs/MathTutor.exe` y el lanzador raíz reportan `1.10.0.0`; el script pasa el análisis sintáctico de PowerShell.

### 3. Borrado del perfil sin recreación automática
- **Corrección:** Los cambios de tema y tamaño de letra solo guardan el perfil si el archivo existe; tras borrarlo, la aplicación no vuelve a crear `user_profile.json` y el arranque regresa a la caracterización.
- **Ubicación:** `src/MathTutor.WinUI/MainWindow.xaml.cs`.
- **Criterio de verificación:** Borrar el perfil, cambiar tema o tamaño y reiniciar mantiene la pantalla de perfil sin datos persistidos.

### 4. Selectores de apoyos accesibles
- **Corrección:** Los seis selectores de apoyos exponen `AutomationProperties.HelpText` y anuncian el valor elegido al cerrarse, sin interrumpir mientras se recorren las opciones.
- **Ubicación:** `src/MathTutor.WinUI/Views/ProfilePage.xaml`, `ProfilePage.xaml.cs`.
- **Criterio de verificación:** El lector recibe una sola confirmación después de elegir y no recibe anuncios por cada movimiento dentro del desplegable.

### 5. Lenguaje de finalización de lección
- **Corrección:** El texto visible y el anuncio usan “¡Explicación completada!” en lugar de “¡Lección completada!”.
- **Ubicación:** `src/MathTutor.WinUI/Views/LessonPage.xaml`, `LessonPage.xaml.cs`.
- **Criterio de verificación:** El texto en pantalla y el anuncio coinciden.

### 6. Foco estable en conteo y patrones
- **Corrección:** Marcar un objeto actualiza su nombre y estado en el lugar, sin volver a dibujar la cuadrícula ni mover el foco; el foco inicial va al primer objeto manipulable y las flechas recorren los objetos sin saltar a las opciones.
- **Ubicación:** `src/MathTutor.WinUI/Views/GamePage.xaml`, `GamePage.xaml.cs`.
- **Criterio de verificación:** Contar varios objetos consecutivos conserva el foco y el orden de navegación; el anuncio de cada objeto refleja si ya fue contado.

### 7. Estrellas al completar la misión
- **Corrección:** El diálogo y el anuncio indican cuántas estrellas se ganaron: 3 al primer intento, 2 al segundo y 1 con tres o más intentos.
- **Ubicación:** `src/MathTutor.WinUI/Navigation/NavigationService.cs`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`, `src/MathTutor.WinUI/MainWindow.xaml.cs`.
- **Criterio de verificación:** El texto visible y el anuncio incluyen “Ganaste X de 3 estrellas” y la siguiente misión conserva el tema calculado.

### 8. Audio semántico y comparaciones
- **Corrección:** Se añadieron timbres de campana, tambor, flauta y palmas; los patrones se reproducen como secuencias y `R` las repite; cada objeto usa un tono coherente con su instrumento o material. Las opciones de comparación anuncian cantidad y sustantivo plural, por ejemplo “Las rojas, 3 manzanas”.
- **Ubicación:** `src/MathTutor.Core/Models/Exercise.cs`, `src/MathTutor.Core/Engine/KindergartenCurriculum.cs`, `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`, `tests/MathTutor.Tests/PedagogyAndAccessibilityTests.cs`.
- **Criterio de verificación:** Las pruebas confirman tonos/timbres distintos y descripciones de cantidad; build `Release` con 0 advertencias y 170/170 pruebas superadas.

### 9. Sincronización de estaciones y recomendación por edad
- **Corrección:** Las 10 estaciones de jardín usan exactamente el título de su misión. `GetRealmStations` acepta el conjunto de `TopicId` permitidos y `MapPage` lo recibe después de filtrar por edad; la estación recomendada se elige solo dentro de ese conjunto y, si todas están completadas, se marca la última disponible. Se añadieron pruebas de regresión para la selección, los nombres y el tono repetido de los patrones.
- **Ubicación:** `src/MathTutor.Core/Services/RewardService.cs`, `src/MathTutor.WinUI/Views/MapPage.xaml.cs`, `tests/MathTutor.Tests/RewardServiceTests.cs`, `tests/MathTutor.Tests/PedagogyAndAccessibilityTests.cs`.
- **Criterio de verificación:** El mapa nunca marca una estación fuera del rango de edad; los nombres de jardín coinciden con sus misiones; cada ítem repetido de un patrón conserva su tono; build `Release` con 0 advertencias y 170/170 pruebas superadas.
# AUDITORÍA INTEGRAL DE REMEDIACIÓN (Fases 1 a 8) — Rama local, pendiente de release

Después de las fases 4 y 5 llegó una auditoría integral de accesibilidad, contenido
educativo, currículo, ejercicios, motores de audio, servicios y UX. Se ejecutó un plan
de remediación en ocho fases con verificación completa después de cada una
(`dotnet build MathTutor.slnx -c Release --nologo /p:TreatWarningsAsErrors=true` más la
suite completa de pruebas). La suite quedó en 196/196 pruebas superadas, 0 advertencias
y 0 errores. Era progresión: 178 (F1) → 183 (F2) → 189 (F3) → 191 (F4/F5) → 194 (F6) →
196 (F8).

## Fase 1 — Persistencia robusta
- **Corrección:** Guardado atómico del perfil (escritura a archivo temporal +
  `File.Move(..., overwrite: true)` bajo un candado `FileGate`); carga corrupta a cuarentena
  (renombra a `*.corrupt-{timestamp}` y reconstruye en blanco sin perder el archivo
  defectuoso); `Delete` de perfiles protegido (solo elimina archivos dentro de la carpeta
  de perfiles); re-migración idempotente persistida (`ProfileVersion` llega a disco).
- **Ubicación:** `src/MathTutor.Core/Services/ProfileStorageService.cs`.
- **Criterio de verificación:** 3 pruebas de IO con archivos reales (guardado atómico,
  cuarentena de JSON corrupto, borrado restringido).

## Fase 2 — Currículo por edad
- **Corrección:** `GetTopicsForAge` separa reforzamiento del nivel de apoyo (DI) del
  alcance por edad; el nivel actual y los desbloqueados en cascada quedan exentos del
  filtro `TargetAge`. `GetNextTopic` hace `fallback` hacia adelante cuando el tema
  "siguiente" solo está encadenado en un nivel bloqueado por edad. Los 9 temas +
  `prog_seed_01` declaran `TargetAgeMin=16` para el desbloqueo adulto (fase 4 pedagógica:
  `prog_ex_01` se reorientó a adolescentes, "El Robot de Entrenamiento", resultado 6).
  Nuevo ejercicio `add_ex_04` "Los Vagones del Tren" (demostración
  `AssociativePropertyDemo`, 5 tesoros, esperado 6, edades 6-8) para la propiedad
  asociativa en `elem_addition_01`.
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`.
- **Criterio de verificación:** 5 pruebas nuevas de exención DI, cascada de niveles,
  temas 16+, ejercicio asociativo accesible y fallback de `GetNextTopic`.

## Fase 3 — Motor de audio robusto (F2)
- **Corrección:** Sustitución de patches por un motor durable: `AudioSynthesizer` en
  `MathTutor.Core` (síntesis WAV PCM 44100 Hz/16-bit/mono determinista con envolvente,
  números máximos/mínimos de frecuencias entre 120 y 1200 Hz, timbres de campana,
  tambor, flauta, aplauso tónica/no tonal) y `AccessibleAudioCueService` reescrito con
  canal serializado único: hilo de reproducción con cola acotada a 32 eventos
  (descarta el más antiguo), `PlaySync` por pista, caché de buffers vía
  `AudioSynthesizer`, hibernación entre 15 segundos de reposo (`Monitor.Wait`) y API
  pública de métodos `ClearPendingCues()` para el inicio de nueva
  partida/cambio de lección. No hay spam de sonidos al iniciar el nivel.
- **Ubicación:** `src/MathTutor.Core/Services/AudioSynthesizer.cs` (nuevo),
  `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`.
- **Criterio de verificación:** 6 pruebas WAV: contenedor PCM, envolvente de fade con
  tolerancia < 500 unidades, determinismo, timbres por `ManipulableSoundKind`, números
  de frecuencias limitados, aplauso tónica/no tonal.

## Fase 4 — UX de Escape, estrellas por lección y refresco de menú
- **Corrección:** `Escape` unificado con `_missionDialogOpen`: cierra el diálogo de
  misión sin saltar vistas; el regreso a la lección usa `RequestBackToLesson` y el
  menú se abre con un único acelerador global en `MainWindow` (sin acordes duplicados
  que se disparen dos veces)
  suena). Persistencia de mejor calificación por lección:
  `UserProfile.LessonStarRatings[topicId] = max(prev, ganadas)`; diálogos muestran
  "Mejor calificación" al repetir; estrella por `RecordAttempt` del `AdaptiveEngine` otorga
  estrella/puntos solo en la primera finalización de cada reto (los reintentos no
  inflan estrellas). Foco tras error se queda en la opción pulsada. `MenuPage`
  conserva/restaura la selección del tema anterior entre
  navegaciones (`UserProfile.CurrentProfileVersion = 3` con migración persistente).
- **Ubicación:** `src/MathTutor.Core/Models/UserProfile.cs`,
  `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `src/MathTutor.WinUI/MainWindow.xaml.cs`,
  `src/MathTutor.WinUI/Views/GamePage.xaml.cs`, `src/MathTutor.WinUI/Views/MenuPage.xaml.cs`.
- **Criterio de verificación:** 2 pruebas (repeticiones sin inflar estrellas; ronda de
  `LessonStarRatings` sobrevive el roundtrip en v3). Comportamiento de Escape
  verificado en el arranque manual de la app.

## Fase 5 — Accesibilidad fina (AAA y lanzamientos por lector de pantalla)
- **Corrección:** Colores subidos a contraste AAA 7:1 en el test de `TextPrimary`, 
  `TextSecondary` y `AppError` para los 4 temas (en el tema de alto contraste oscuro se ajusta el error a
  RGB 255,120,120 y en `DefaultPlayful` a RGB 170,0,30); texto en chips/botones
  (`OnAccent`/`OnSuccess`) se mantiene ≥ 4.5:1. `CycleTheme` no fuerza un tema cuando el
  Contraste del Sistema de Windows define la apariencia. Atajos de teclado
  publicados por `AutomationProperties.AcceleratorKey` en `MainWindow.xaml.cs` y
  `trophy` del diálogo misión marcada con la vista de
  accesibilidad `Raw` vía `AutomationProperties.SetAccessibilityView(..., Raw)` (el
  enum correcto en WinUI 3 vive en `Microsoft.UI.Xaml.Automation.Peers`).
- **Ubicación:** `src/MathTutor.WinUI/Services/AccessibilityManager.cs`,
  `src/MathTutor.WinUI/MainWindow.xaml.cs`,
  `tests/MathTutor.Tests/ThemeContrastTests.cs`.
- **Criterio de verificación:** tabla de contrastes calculada en los 4 temas; granular
  `theme` con umbral AAA automático en la suite.

## Fase 6 — Generador de ejercicios
- **Corrección:** `BuildOptions` respeta el rango declarado por cada tipo (mínimo/máximo
  exactos si aplica), ya no da distractores triviales y predecibles (-1/+1/+2) y elige
  distractores cercanos pero aleatorios con `jitter = span/3`; respaldo determinista
  siempre terminante para type ranges con solo 2 opciones posibles (caso fracción 1/2).
  Neutralizadas 21 descripciones `ScreenReaderDescription` que revelaban
  la respuesta (`Fase 6.1` = 8 reescrituras en `ExerciseGenerator.cs`; `Fase 6.2` = 13 reescrituras en
  `CurriculumService.cs`: "Total: 6" → "observa y cuenta."; "Respuesta: 6 pasos" →
  "cuenta cuántas veces multiplicas.", etc.).
- **Ubicación:** `src/MathTutor.Core/Engine/ExerciseGenerator.cs`
  (`BuildOptions`, descripciones), `src/MathTutor.Core/Engine/CurriculumService.cs`.
- **Criterio de verificación:** 3 pruebas nuevas (`BuildOptions` con 2 rangos degenerados
  producen terminación y 3 opciones únicas; variedad > 6 combinatorias de distractores
  al ampliar semillas; cordura de opciones con 3 desperdicios obligados en retos de
  todos los temas del currículo específico). OJO: bug introducido y arreglado en esta fase
  (fallback con `Clamp` no terminante para `fracciones` con denominador 2); verificado con
  `dotnet-stack`.

## Fase 7 — `SpeechService` sin chatter
- **Corrección:** Latches de comprobación: `NVDA Controller Client` hace el
  handshake RPC la primera vez (`_nvdaControllerActive`) y `_jawsDetected`/`_jawsFailed`
  evitan re-consultar procesos y re-crear el objeto COM de JAWS en cada locución. Toda
  falla latching de `JAWS COM` cae honestamente en el canal
  `UIA` nativo (NVDA/JAWS/Narrador con velocidad del usuario). La firma del binding
  `COM SayString(text, interrupt)` no se toca.
- **Ubicación:** `src/MathTutor.WinUI/Services/SpeechService.cs`.
- **Criterio de verificación:** no aplica pruebas unitarias (native bridge),
  comportamiento revisado; build 0/0 y 194/194 al momento del cambio.

## Fase 8 — Pistas simplificadas en lenguaje sencillo
- **Corrección:** `SimplifiedHintCatalog` (nuevo en `MathTutor.Core`) con pistas 1 y 2 y
  explicación de éxito en lenguaje sin tecnicismos para TODOS los 52 tipos de juego; 
  `AdaptiveEngine.GetAdaptedHint` y `GetAdaptedSuccess` usan esas variantes como
  respaldo para alumnos con `SimplifyLanguage`/discapacidad intelectual en
  los ~45 temas de secundaria, preparatoria y programación (y jardín/primaria sin texto
  simple propio) cuando el ejercicio no trae variantes simplificadas a mano.
  Prueba cubre los 52 tipos del catálogo y el fallback en
  `sec_order_operations_01` con `SimplifyLanguage`.
- **Ubicación:** `src/MathTutor.Core/Engine/SimplifiedHintCatalog.cs` (nuevo),
  `src/MathTutor.Core/Engine/AdaptiveEngine.cs`,
  `tests/MathTutor.Tests/ExerciseGeneratorTests.cs`.
- **Criterio de verificación:** cobertura completa del catálogo (52 entradas) y
  fallback comprobado; build 0/0 y 196/196.

## Segunda auditoría integral de remediación (fases 1-7, oct 2026)

Revisión posterior con ojo fresco sobre Core, WinUI y documentación. Suite: 196 → 209.

### Fase 1 - Persistencia y perfil
- **Corrección:** `ProfileStorageService.LoadCore` limita su `catch` a `JsonException`;
  la re-escritura de la migración v2→v3 y el `Save` quedan fuera de la cuarentena y
  capturan `IOException`/`UnauthorizedAccessException`, de modo que un fallo
  transitorio de disco ya no marca como corrupto un perfil válido. `Delete` se
  serializa con el mismo `FileGate` que `Save`/`Load`.
- **Ubicación:** `src/MathTutor.Core/Services/ProfileStorageService.cs`.
- **Criterio de verificación:** 2 pruebas nuevas (fallo transitorio de persistencia de
  migración; concurrencia `Save`/`Load`/`Delete` sin cuarentena espuria).

### Fase 2 - Foco, teclado y estrellas
- **Corrección:** `LessonPage` no intercepta `↑`/`↓`/`Backspace`/`Espacio` cuando el
  foco está en un `TextBox` (lectura con caret) y se eliminó su `Escape` local (solo
  queda el acelerador global de `MainWindow`). `GamePage` suma los intentos del último
  reto antes de calcular estrellas y el reintento anuncia un mensaje breve en lugar de
  concatenar la pista obsoleta. El F1 del diálogo de misión se unificó en el
  acelerador global (`_missionDialogMessage`) sin doble anuncio. `MapPage` anuncia
  "Sendero por desbloquear" y una ayuda coherente para estaciones bloqueadas; menú y
  mapa muestran estado vacío cuando no hay misiones para la edad; la bienvenida larga
  solo suena al llegar de verdad al menú. `ToggleSpeech` persiste cuando ya hay perfil.
- **Ubicación:** `Views/LessonPage.xaml.cs`, `Views/GamePage.xaml.cs`,
  `Views/MapPage.xaml(.cs)`, `Views/MenuPage.xaml(.cs)`, `MainWindow.xaml.cs`,
  `Services/AccessibilityManager.cs`.
- **Criterio de verificación:** build 0/0 y comportamiento revisado en arranque manual.

### Fase 3 - Contenido curricular y recompensas
- **Corrección:** `add_ex_04` incluye el sexto tesoro (vagón 3) y corrige "vagón";
  `sub_ex_01` incorpora los 5 patitos manipulables que narra; la pista simplificada de
  fracciones mixtas ya no asume denominador 2; `calcularVolumen` pasa a
  `calcularDecibeles` (coherente con su unidad) en el tema curado y en el generador;
  se aclara la frase de "sierra" en números con signo. `RewardService.GetRealmStations`
  usa las estrellas reales de `LessonStarRatings` (mínimo 1 para perfiles antiguos sin
  calificación) en lugar de 3 fijos.
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`,
  `Engine/ExerciseGenerator.cs`, `Engine/SimplifiedHintCatalog.cs`,
  `src/MathTutor.Core/Services/RewardService.cs`.
- **Criterio de verificación:** pruebas de contenido actualizadas (6 ítems en
  asociativa, 5 patitos en resta, estrellas reales) y 209/209.

### Fase 4 - Generador adaptativo
- **Corrección:** el respaldo de `BuildOptions` alterna arriba/abajo y siempre termina
  (sin sesgo en rangos degenerados ni resultados negativos); `GetAdaptedOptions` elige
  el distractor más cercano al resultado y trabaja sobre copias, preservando el orden
  relativo y sin mutar las opciones compartidas.
- **Ubicación:** `src/MathTutor.Core/Engine/ExerciseGenerator.cs`,
  `src/MathTutor.Core/Engine/AdaptiveEngine.cs`.
- **Criterio de verificación:** 2 pruebas nuevas (rango degenerado bidireccional;
  reducción al distractor más cercano sin mutación).

### Fase 5 - Audio y lectores de pantalla
- **Corrección:** el timbre de tambor aplica de verdad el clamp 110-220 Hz (antes era
  código muerto); `PlayItemSequence` limpia la cola antes de sonar y
  `RootFrame.Navigated` limpia las señales pendientes al cambiar de página; el latch de
  NVDA solo se activa con una locución exitosa (se degrada si el RPC falla) y se
  reinicia en los `catch`; `Dispose` libera el RCW de JAWS con
  `Marshal.FinalReleaseComObject` y reinicia los latches.
- **Ubicación:** `src/MathTutor.Core/Services/AudioSynthesizer.cs`,
  `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`,
  `src/MathTutor.WinUI/Services/SpeechService.cs`, `MainWindow.xaml.cs`.
- **Criterio de verificación:** 6 pruebas de síntesis y build 0/0.

### Fase 6 - Contraste AAA
- **Corrección:** el acento usado como texto cumple AAA 7:1 en los cuatro temas
  (`LowSensory` RGB 30,80,120 con `OnAccent` blanco; `DefaultPlayful` RGB 70,45,180).
  La teoría de contraste cubre `AppAccentBrush` sobre fondo y tarjeta.
- **Ubicación:** `src/MathTutor.WinUI/Services/AccessibilityManager.cs`,
  `tests/MathTutor.Tests/ThemeContrastTests.cs`.
- **Criterio de verificación:** 8 casos `InlineData` nuevos (40 en total).

### Fase 7 - Documentación
- **Corrección:** README y ACR con 209 pruebas; tabla y árbol de pruebas al día
  (66/71/21/5/6 + 40 casos); `GamePlayType` unificado en 52; `ProfileVersion` v3;
  eliminada la dependencia inexistente de WebView2; enlaces `file:///` de máquinas
  ajenas convertidos a rutas relativas; fragmentos corruptos reescritos.
- **Criterio de verificación:** build 0/0, 209/209, UTF-8 sin BOM y sin U+FFFD.

> **Limitaciones aceptadas:** el "flip" transitorio de tema en `RefreshThemeResources`
> se conserva (fuerza la re-evaluación de `ThemeResource`; su alternativa requiere
> prueba visual en dispositivo); las preferencias de tema/fuente elegidas antes de
> crear el perfil no se persisten (guardarlas antes del onboarding lo saltaría).

---

## 📌 Remediaciones y Ampliación Curricular Integral (Release 1.11 - Octubre 2026)

Implementación integral de la Hoja de Ruta Curricular Oficial (Fases 1 a 5), incorporando los 17 temas y estaciones faltantes identificados en la investigación curricular oficial (SEP / NCTM), alcanzando 74 estaciones consecutivas del Reino y 231 pruebas unitarias automatizadas.

### Fase 1 - Primaria Temprana y Magnitudes Físicas
- **Corrección / Ampliación:** Implementación curricular y procedimental de:
  - Sistema monetario, denominaciones y vuelto (`elem_money_currency_01`).
  - Medición con regla, longitudes y distancias en centímetros (`elem_measurement_ruler_01`).
  - Lectura de tiempo en reloj analógico y digital en horas y medias horas (`elem_time_clock_01`).
  - Cálculo de perímetro en polígonos regulares y rectángulos (`elem_geometry_perimeter_01`).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `ExerciseGenerator.cs`, `SimplifiedHintCatalog.cs`, `RewardService.cs`.
- **Criterio de verificación:** Pruebas unitarias dedicadas en `ExerciseGeneratorTests.cs`, `CurriculumServiceTests.cs` y `RewardServiceTests.cs`.

### Fase 2 - Primaria Superior, Aritmética y Estadística
- **Corrección / Ampliación:** Implementación curricular y procedimental de:
  - División entera con residuo explícito ($D = d \times q + r$) (`elem_division_remainder_01`).
  - Ubicación de fracciones propias e impropias en la recta numérica (`elem_fractions_numberline_01`).
  - Interpretación y lectura de gráficas de barras y tablas de frecuencias (`elem_data_barcharts_01`).
  - Medidas de tendencia central (media aritmética, mediana y moda) (`elem_stats_central_tendency_01`).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `ExerciseGenerator.cs`, `SimplifiedHintCatalog.cs`, `RewardService.cs`.
- **Criterio de verificación:** Pruebas unitarias de cálculo estadístico exacto y validación de enteros.

### Fase 3 - Secundaria Inicial, Álgebra y Geometría Plana
- **Corrección / Ampliación:** Implementación curricular y procedimental de:
  - Círculo, diámetro, radio y longitud de circunferencia con aproximación de $\pi$ (`sec_circle_geometry_01`).
  - Descomposición en factores primos, Mínimo Común Múltiplo (MCM) y Máximo Común Divisor (MCD) (`sec_factors_multiples_mcm_mcd_01`).
  - Números racionales negativos, sumas y comparaciones con signo (`sec_negative_rational_numbers_01`).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `ExerciseGenerator.cs`, `SimplifiedHintCatalog.cs`, `RewardService.cs`.
- **Criterio de verificación:** Escalamiento entero de $C = 314$ cm ($r=50$ cm) y pruebas de divisibilidad.

### Fase 4 - Secundaria Avanzada y Álgebra Rigurosa
- **Corrección / Ampliación:** Implementación curricular y procedimental de:
  - Factorización algebraica (factor común y diferencia de cuadrados) (`sec_algebra_factoring_01`).
  - Ángulos entre paralelas cortadas por una secante (alternos internos, correspondientes, opuestos) (`sec_parallel_lines_angles_01`).
  - Probabilidad clásica y de eventos compuestos (regla del producto con monedas y dados) (`sec_probability_compound_01`).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `ExerciseGenerator.cs`, `SimplifiedHintCatalog.cs`, `RewardService.cs`.
- **Criterio de verificación:** Preservación de la contigüidad de Secundaria 1º a 3º en `GetTopicsForAge` evitando saltos en el desbloqueo en cascada.

### Fase 5 - Preparatoria, Geometría Analítica y Cálculo
- **Corrección / Ampliación:** Implementación curricular y procedimental de:
  - Secciones cónicas analíticas: parábola y elipse con vértice/centro en el origen (`prep_conics_parabola_ellipse_01`).
  - Límites de funciones y noción de continuidad por sustitución y factorización (`prep_calculus_limits_01`).
  - Reglas de derivación operativa: regla de la potencia y suma/constante ($f'(x) = n \cdot c \cdot x^{n-1}$) (`prep_calculus_derivative_rules_01`).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `ExerciseGenerator.cs`, `SimplifiedHintCatalog.cs`, `RewardService.cs`.
- **Criterio de verificación:** Generación robusta sin división por cero y pruebas de derivadas exactas.

### Fase 6 - Accesibilidad de UI y Usabilidad de Teclado (WinUI 3)
- **Corrección:**
  - En `GamePage.xaml`, el panel de opciones de respuesta implementa `WrapPanel` en lugar de una disposición rígida en una sola columna/fila, evitando solapamientos y scroll horizontal forzado (WCAG 1.4.10 Reflow).
  - En enunciados extensos de problemas matemáticos se sustituyó el corte con puntos suspensivos (`TextTrimming="CharacterEllipsis"`) por salto de línea natural (`TextWrapping="WrapWholeWords"`), garantizando la visibilidad completa del problema.
  - En `MainWindow.xaml.cs`, los aceleradores numéricos globales (`1-4` / `NumPad1-NumPad4`) se deshabilitan si el foco actual reside en un `TextBox`, evitando que escribir un número en una entrada de texto active accidentalmente una opción del juego.
- **Ubicación:** `src/MathTutor.WinUI/Views/GamePage.xaml`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`, `src/MathTutor.WinUI/MainWindow.xaml.cs`.
- **Criterio de verificación:** Pruebas unitarias de accesibilidad y foco en `PedagogyAndAccessibilityTests.cs`.

### Fase 7 - Suite de Pruebas Automatizadas y Release 1.11
- **Resultado:**
  - De 209 a 231 pruebas unitarias automáticas aprobadas al 100% (231 passed, 0 failed, 0 warnings).
  - Versionamiento sincronizado en `1.11.0` en `MathTutor.Core.csproj`, `MathTutor.WinUI.csproj` y scripts de empaquetado.


### Fase 8 - Optimización de Arranque, Caché de Currículo y Latencia Cero de Audio
- **Problema diagnosticado:**
  1. *Latencia de audio:* En `AccessibleAudioCueService.cs`, el método `Enqueue()` no llamaba a `Monitor.Pulse(ChannelGate)` cuando el hilo de reproducción `PlaybackLoop` ya estaba en ejecución. Si el hilo entraba en `Monitor.Wait(ChannelGate, 15000)`, las pistas subsecuentes no se despertaban de inmediato, generando un retraso audible o aparente latencia de hardware.
  2. *Retraso al abrir la app:* `CurriculumService.GetAllTopics()` reconstruía manualmente los 74 temas curriculares y sus más de 350 ejercicios en cada llamada (en el menú, mapa, navegación y recompensas), saturando el hilo de UI y el Garbage Collector.
- **Corrección aplicada:**
  1. En `AccessibleAudioCueService.cs`, se añadió `Monitor.Pulse(ChannelGate)` en el bloque `Enqueue`, garantizando que cualquier pista despierte inmediatamente el hilo de fondo sin esperar timeout.
  2. En `CurriculumService.cs`, se implementó una caché estática de lectura única (`_cachedTopics` con bloqueo thread-safe de inicialización diferida), reduciendo el tiempo de acceso de $O(N)$ repetido a $O(1)$ instantáneo.
  3. Se crearon pruebas automatizadas de rendimiento y carga en `tests/MathTutor.Tests/LoadAndPerformanceTests.cs` (234 pruebas en total).
- **Ubicación:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `src/MathTutor.WinUI/Services/AccessibleAudioCueService.cs`, `tests/MathTutor.Tests/LoadAndPerformanceTests.cs`.
- **Criterio de verificación:** Latencia de caché de audio $< 1$ ms, carga de temas $< 500$ ms, y suite completa 234/234 passed.


### Fase 9 - Auditoría Integral de Usabilidad, Foco Accesible, Gamificación y Aleatorización Curricular (v1.11.0)
**Contexto:** Hallazgos consolidados de pruebas de usuario (Jardín y Primaria) y auditoría profunda de arquitectura de código (.Core y .WinUI).

#### 1. README.md y documentación inicial inaccesible con lectores de pantalla
- **Componente / Archivo:** `README.md`.
- **Problema Actual:** Existen bloques con separadores excesivos de caracteres repetidos (`====================`, `--------------------`). Los lectores de pantalla (NVDA, JAWS, Narrador) anuncian cada signo individualmente o generan pausas prolongadas y saturación acústica ("igual igual igual..."), degradando la experiencia para usuarios con discapacidad visual.
- **Comportamiento Esperado:** Reemplazar separadores visuales repetitivos por encabezados Markdown semánticos (`#`, `##`, `###`) y líneas divisorias estándar (`---`).
- **Instrucciones de Implementación:** Limpiar `README.md` eliminando cadenas de símbolos iguales o guiones repetidos.

#### 2. Reinicio y caracterización tras borrado de perfil
- **Componente / Archivo:** `src/MathTutor.Core/Services/ProfileStorageService.cs`, `src/MathTutor.WinUI/App.xaml.cs`.
- **Problema Actual:** Si el usuario elimina su archivo de perfil (`profile.json`) para realizar pruebas o reiniciar el aprendizaje, al iniciar la aplicación no se lanza el asistente de caracterización inicial. En su lugar, el sistema genera automáticamente un perfil por defecto con opciones predeterminadas sin consultar la edad, necesidades de apoyo ni accesibilidad del usuario.
- **Comportamiento Esperado:** Si no se encuentra un perfil válido al iniciar, debe mostrarse obligatoriamente la pantalla de bienvenida y el asistente de caracterización/configuración inicial de perfil.
- **Instrucciones de Implementación:** En `App.xaml.cs` (método de inicialización), verificar explícitamente `_profileService.ProfileExists()`. Si retorna falso, navegar a `CharacterizationPage` o abrir el diálogo de caracterización antes de inicializar `MainWindow` o `MenuPage`.

#### 3. Interrupción de lectura y foco en ComboBox de apoyos TDAH
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/CharacterizationPage.xaml` / `CharacterizationPage.xaml.cs`.
- **Problema Actual:** Al seleccionar apoyos de discapacidad visual y autismo, al llegar a "Presentación predominante del TDAH", el lector anuncia inmediatamente: *"Apoyos actualizados. Las preferencias que ajustaste manualmente se conservan"*, interrumpiendo la lectura de las opciones disponibles y sin instruir al usuario que debe presionar `Enter` o `Alt + Flecha Abajo` para desplegar el cuadro combinado.
- **Comportamiento Esperado:** El anuncio de "Apoyos actualizados" no debe dispararse durante la exploración de los campos ni interrumpir la lectura del ComboBox. Las instrucciones de interacción ("Presiona Enter o Alt+Abajo para abrir las opciones") deben incluirse en `AutomationProperties.HelpText`.
- **Instrucciones de Implementación:**
  1. Mover el anuncio de confirmación al momento en que el usuario guarde el perfil o finalice la selección, o condicionar el live region para que no interrumpa el cambio de foco.
  2. Agregar en `AutomationProperties.HelpText` de los `ComboBox` del asistente: `"Presiona Enter o Alt + Flecha Abajo para desplegar la lista de opciones"`.

#### 4. Anuncio prematuro de "Lección completada" en ejemplos guiados
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/LessonPage.xaml.cs` (paso de ejemplo guiado con Cosmo).
- **Problema Actual:** En el paso 3 de los ejemplos guiados (ej. *"Situación: Las piedras dicen: 1, 2, 3, 4... ¿Qué número sigue?"*), el texto y el anuncio UIA dicen: *"¡Lección completada! Presiona Enter para comenzar el reto lúdico..."*. Esto genera confusión porque el usuario aún no ha realizado ningún ejercicio ni reto.
- **Comportamiento Esperado:** El texto y anuncio deben indicar: *"¡Ejemplo guiado completado! Presiona Enter para comenzar el reto lúdico, o Flecha arriba para repasar"*.
- **Instrucciones de Implementación:** Modificar la plantilla de texto en `LessonPage.xaml.cs` para distinguir entre la conclusión del ejemplo explicativo y la finalización total de la lección/misión.

#### 5. Gestión del foco en ejercicios interactivos: Opciones vs Objetos manipulables
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/GamePage.xaml.cs` (`ShouldStartWithManipulables`, `FocusInitialElement`).
- **Problema Actual:**
  1. `ShouldStartWithManipulables` únicamente retorna `true` para `InteractiveCounting` y secuencias de jardín. Para más de 50 tipos de ejercicios (monedas/billetes, comparación de grupos, fracciones, regla, reloj, gráficos de barras), el foco inicial intenta dirigirse a las opciones de respuesta (`ItemsOptions[0]`). Si WinUI aún no ha renderizado los botones de opción, cae en el fallback `BtnBackToLesson.Focus()`, expulsando el lector de pantalla a la barra de herramientas superior.
  2. Cuando el usuario está explorando o interactuando con los objetos contables (ej. "Siete Tesoros del Bosque"), al presionar `Tab` o tras pulsar `Enter` para contar, el foco salta arbitrariamente a las opciones de respuesta o se salta elementos (del 5 al 3), perdiendo la posición de los tesoros restantes.
- **Comportamiento Esperado:**
  1. En todo ejercicio que contenga objetos interactivos/manipulables (`ItemsGroupA.Count > 0` o `ItemsGroupB.Count > 0`), el foco inicial debe situarse en el primer objeto interactivo del lienzo.
  2. La navegación con flechas dentro del contenedor de objetos manipulables debe ser bidireccional y continua (sin saltos arbitrarios).
  3. Pulsar `Enter` sobre un objeto manipulable debe anunciar su conteo/estado sin provocar la pérdida de foco ni el salto hacia las opciones de respuesta.
- **Instrucciones de Implementación:**
  1. Ampliar `ShouldStartWithManipulables` para retornar `true` si `_currentExercise.ItemsGroupA?.Count > 0`.
  2. Implementar `DispatcherQueue.TryEnqueue` con verificación de carga para asegurar que el primer elemento manipulable reciba el foco accesible.
  3. Evitar que el evento `Click`/`Enter` del manipulable invoque re-enfoque al panel de opciones a menos que se hayan completado todos los elementos requeridos.

#### 6. Adaptaciones auditivas y sensoriales para ceguera en Jardín (Patrones y Comparaciones)
- **Componente / Archivo:** `src/MathTutor.Core/Engine/CurriculumService.cs`, `src/MathTutor.WinUI/Views/GamePage.xaml.cs`.
- **Problema Actual:** Ejercicios basados en discriminación puramente visual (ej. *"Rojo, azul, rojo, azul... ¿Qué sigue?"* o *"Estrellas Amarillas y Azules. ¿Dónde hay más estrellas?"*) carecen de pistas sonoras distintivas. Un niño ciego depende únicamente de la lectura de texto plano, perdiendo la experiencia lúdica y multisensorial de la app.
- **Comportamiento Esperado:**
  1. Asignar frecuencias sonoras o notas musicales diferenciadas para cada color/categoría (ej. Rojo = Do / 261 Hz, Azul = Sol / 392 Hz, Amarillo = Mi / 329 Hz).
  2. Al enfocar o seleccionar cada elemento del patrón o grupo, reproducir su nota distintiva para que la secuencia forme una melodía reproducible y comprensible por oído.
- **Instrucciones de Implementación:** Integrar llamadas a `AccessibleAudioCueService` con tonos proporcionales en los eventos de foco y activación de los elementos del patrón.

#### 7. Desincronización de `StarsTotal` (Trofeos congelados en 0 estrellas)
- **Componente / Archivo:** `src/MathTutor.Core/Models/UserProfile.cs`, `src/MathTutor.WinUI/MainWindow.xaml.cs` (`OnMissionCompleted`).
- **Problema Actual:** Al superar una misión, `OnMissionCompleted` actualiza el diccionario `profile.LessonStarRatings[lessonId] = stars`, pero nunca actualiza la propiedad `profile.StarsTotal`. Por ello, `StarsTotal` permanece siempre en 0, la barra superior de la ventana muestra "⭐ 0 estrellas", y los trofeos que requieren estrellas acumuladas (como `badge_star_collector` que pide 15) jamás se desbloquean.
- **Comportamiento Esperado:** `StarsTotal` debe reflejar la suma real de las mejores estrellas obtenidas en todas las lecciones completadas, actualizándose inmediatamente al terminar cada misión y reflejándose en la barra de estado y en el sistema de trofeos.
- **Instrucciones de Implementación:**
  1. En `UserProfile.cs`, agregar un método `RecalculateStars()` que calcule `StarsTotal = LessonStarRatings.Values.Sum()`.
  2. En `MainWindow.xaml.cs` (`OnMissionCompleted`), invocar `profile.RecalculateStars()` antes de evaluar los trofeos y antes de guardar el perfil en disco.

#### 8. El tutor trata ejercicios de Primaria como conteo de preescolar
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/GamePage.xaml.cs` (líneas 180-200 y 285-305).
- **Problema Actual:** La bandera `isSymbolicAdvanced` exige que el ejercicio sea de secundaria en adelante (`_currentExercise.Level >= AcademicLevel.SecondaryFirstGrade`). En consecuencia, en más de 35 ejercicios interactivos de Primaria (monedas, billetes, fracciones en recta, reloj, barras estadísticas, perímetros), al pulsar o interactuar con un elemento, el sistema ejecuta la lógica de conteo preescolar anunciando: *"¡1! ¡Terminaste de contar todos! Hay 1 en total. El último número es el total de la colección"*.
- **Comportamiento Esperado:** En Primaria, la interacción con monedas, relojes, fracciones y barras debe anunciar el valor semántico del elemento (ej. *"Billete de $20 pesos"*, *"Fracción 3 cuartos"*, *"Hora: 4 y media"*), no el principio de cardinalidad de preescolar.
- **Instrucciones de Implementación:**
  1. Redefinir la condición de conteo preescolar para que aplique estrictamente cuando `_currentExercise.ExerciseType == ExerciseType.InteractiveCounting` o `_currentExercise.Level == AcademicLevel.Kindergarten`.
  2. Para `MoneyCurrencyCounting`, `MeasurementRuler`, `TimeClockReading`, `FractionsNumberLine`, etc., proporcionar manejadores de anuncio contextual basados en `item.Label` y `item.Value`.

#### 9. Ausencia de generación procedimental / aleatorización en Primaria
- **Componente / Archivo:** `src/MathTutor.Core/Engine/CurriculumService.cs` (`GenerateAdaptiveExercises`).
- **Problema Actual:** El método `GenerateAdaptiveExercises` toma primero todos los ejercicios estáticos creados a mano mediante `result.AddRange(curated.Take(count))`. Dado que cada tema de primaria contiene entre 3 y 4 ejercicios curados, la lista solicitada siempre se llena con ejercicios estáticos idénticos y nunca se invoca `_generator.GenerateExercisesForTopic`. Como resultado, ejercicios como el de residuo (`divrem_ex_04`: "Multiplica 6 × 4 = 24, y súmale el sobrante (+2)") se repiten exactamente iguales en cada intento sin variar números ni opciones.
- **Comportamiento Esperado:** Los ejercicios deben aleatorizarse procedimentalmente para garantizar rejugabilidad y evaluar comprensión real. Si el usuario repite la misión o solicita nuevos ejercicios, deben generarse nuevas cifras y operaciones.
- **Instrucciones de Implementación:**
  1. En `GenerateAdaptiveExercises`, barajar los ejercicios curados (`Shuffle`) o combinar ejercicios procedimentales generados por `ExerciseGenerator` con los curados.
  2. Alternativamente, integrar un selector procedimental dinámico cuando `curated` ya haya sido resuelto previamente por el perfil del usuario.

#### 10. Inconsistencias en definición de objetos manipulables de Primaria
- **Componente / Archivo:** `src/MathTutor.Core/Engine/CurriculumService.cs`.
- **Problema Actual:**
  1. En `money_ex_01`, el enunciado solicita contar 2 monedas de $5 y 3 monedas de $1, pero la lista `ItemsGroupA` solo define dos monedas de $5 y una sola moneda de $1 (faltan dos monedas de $1).
  2. En `money_ex_04` ("El Mercado del Reino: Monedas y Billetes"), el ejercicio está tipificado como manipulable monetario, pero solo contiene un único billete de $20, imposibilitando realizar cálculos de compra o cambio interactivo.
  3. En `ruler_ex_04`, se plantea una pregunta puramente memorística y abstracta ("¿Cuántos centímetros hay en un metro?") sin regla ni objetos interactivos de medición proporcional.
- **Comportamiento Esperado:** Los elementos definidos en `ItemsGroupA` e `ItemsGroupB` deben coincidir exactamente con las cantidades descritas en el enunciado, y los ejercicios de medición deben incluir segmentos o marcas visuales y accesibles de escala.
- **Instrucciones de Implementación:** Corregir las listas de manipulables en `CurriculumService.cs` para `money_ex_01` (añadir las dos monedas de $1 faltantes), enriquecer `money_ex_04` con monedas de cambio fraccionario, y añadir marcas de escala métrica a `ruler_ex_04`.

#### 11. `VerticalListNavigation` bloquea y suprime flechas izquierda y derecha
- **Componente / Archivo:** `src/MathTutor.WinUI/Navigation/VerticalListNavigation.cs`.
- **Problema Actual:** Al capturar los eventos de teclado en listas verticales, se ejecuta `e.Handled = true;` antes de evaluar si la tecla presionada es `VirtualKey.Left` o `VirtualKey.Right`. Esto intercepta y anula las flechas horizontales, impidiendo que usuarios de lectores de pantalla naveguen carácter a carácter o palabra a palabra en textos de listas.
- **Comportamiento Esperado:** `VerticalListNavigation` debe gestionar únicamente `VirtualKey.Up` y `VirtualKey.Down` (y `Home`/`End` si aplica), dejando `Left` y `Right` sin marcar (`e.Handled = false`) para permitir la revisión de accesibilidad de los lectores de pantalla.
- **Instrucciones de Implementación:** Mover `e.Handled = true;` dentro de los bloques específicos de `case VirtualKey.Up:` y `case VirtualKey.Down:`.

#### 12. Árbol de trofeos colapsado por defecto y accesibilidad de `TreeViewItem`
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/MapPage.xaml.cs` (`BadgeGroupExpansion`, `BadgeItem_Loaded`).
- **Problema Actual:**
  1. Los grupos de insignias en el `TreeView` inician colapsados (`IsExpanded = false`), lo que oculta los trofeos del lector de pantalla hasta que el usuario descubre que debe presionar `Flecha Derecha` en cada categoría.
  2. `BadgeItem_Loaded` busca el `TreeViewItem` ancestro mediante el árbol visual; si se invoca antes de que el nodo esté completamente acoplado, no encuentra el padre y deja el contenedor con un nombre accesible vacío.
- **Comportamiento Esperado:** Los grupos de insignias deben iniciar expandidos por defecto o anunciar su estado ("Grupo colapsado, presiona Flecha Derecha para expandir"). La asignación de nombres accesibles debe garantizarse mediante enlace de datos (`x:Bind`) o en el contenedor del item sin depender de la búsqueda en el árbol visual.
- **Instrucciones de Implementación:** Establecer `IsExpanded = true` por defecto en las categorías del `TreeView` de insignias en `MapPage.xaml`, y enlazar `AutomationProperties.Name` directamente a las propiedades del modelo de datos de la insignia.

#### 13. Conteo acumulado distorsionado en ejercicios de comparación
- **Componente / Archivo:** `src/MathTutor.WinUI/Views/GamePage.xaml.cs`.
- **Problema Actual:** En ejercicios de comparación de dos grupos (ej. manzanas rojas vs verdes), los elementos del Grupo B reciben `DisplayOrder = groupA.Count + 1 + i`. Al tocar la primera manzana verde del Grupo B, el sistema anuncia: *"¡4!"* y luego *"¡5! Hay 5 en total"*, en lugar de contar el Grupo B de forma independiente (1, 2).
- **Comportamiento Esperado:** Cada grupo debe tener su propio contador de cardinalidad independiente (Grupo A: 1 a 3; Grupo B: 1 a 2) y anunciar claramente a qué grupo pertenece el objeto contado.
- **Instrucciones de Implementación:** Mantener dos contadores locales separados (`_countGroupA` y `_countGroupB`) en `GamePage.xaml.cs` para ejercicios de comparación.

#### 14. Filtro de throttle en `NavigationService.cs` ignora cambio de parámetro
- **Componente / Archivo:** `src/MathTutor.WinUI/Navigation/NavigationService.cs`.
- **Problema Actual:** La condición de prevención de doble navegación evalúa únicamente: `_lastNavigatedPageType == pageType && (now - _lastNavigatedTime).TotalMilliseconds < 300`. Si el usuario navega rápidamente de una lección a otra lección distinta (ambas usan `GamePage` o `LessonPage`), la navegación se ignora silenciosamente porque el tipo de página es el mismo y ocurrió en menos de 300 ms.
- **Comportamiento Esperado:** La navegación debe permitirse si el parámetro de destino (el identificador de tema o ejercicio) es diferente al anterior, aun cuando se ejecute en menos de 300 ms.
- **Instrucciones de Implementación:** Almacenar `_lastNavigatedParameter` y condicionar el descarte de navegación a que tanto el tipo de página como el parámetro coincidan exactamente: `_lastNavigatedPageType == pageType && Equals(_lastNavigatedParameter, parameter) && ...`.

#### 15. Mezcla y continuidad curricular entre Primaria Baja, Media y Alta
- **Componente / Archivo:** `src/MathTutor.Core/Engine/AdaptiveEngine.cs`, `CurriculumService.cs` (`GetNextTopic`).
- **Problema Actual:** Para perfiles entre 6 y 11 años, `GetTopicsForAge` concatena sin fronteras los temas de Primaria Baja, Media y Alta. En `GetNextTopic`, al culminar una misión de Primaria Media, el sistema propone de inmediato saltar a temas avanzados de Primaria Alta (como fracciones mixtas complejas y decimales) sin un diálogo de transición de ciclo que informe al alumno del cambio de nivel educativo.
- **Comportamiento Esperado:** Al completar el último tema de un subciclo escolar, debe mostrarse una felicitación de fin de etapa y ofrecerse la opción de continuar al siguiente nivel o consolidar los conocimientos adquiridos.
- **Instrucciones de Implementación:** Incorporar marcadores de finalización de nivel en `CurriculumService` e informar en el diálogo de misión completada cuando se ha concluido un ciclo completo.


---

### Cierre de Implementación - Fase 9 (v1.11.0 - Auditoría Integral de Usabilidad y Foco Accesible)

Implementación completada al 100% cubriendo los 15 hallazgos de usabilidad, foco y aleatorización curricular:

1. **README.md Accesible**: Separadores semánticos estándar sin caracteres repetitivos que saturen lectores de pantalla.
2. **Reinicio de Caracterización**: Si no existe perfil en disco (`_storage.ProfileExists() == false`), la app inicia en caracterización limpia sin asumir datos por defecto; cancelar o presionar Escape sin perfil guardado impide saltar al menú y orienta al alumno.
3. **ComboBox de Apoyos sin Interrupción**: Eliminada la locución intrusiva en cambios de foco de apoyos TDAH; añadida la instrucción `AutomationProperties.HelpText="Presiona Enter o Alt + Flecha Abajo para desplegar la lista de opciones"` a todos los ComboBoxes del perfil.
4. **Anuncio Preciso en Ejemplo Guiado**: En el paso 3 de lección, el anuncio indica *"¡Ejemplo guiado completado! Presiona Enter para comenzar el reto lúdico, o Flecha arriba o Backspace para repasar."*
5. **Foco en Objetos Manipulables**: `ShouldStartWithManipulables()` retorna `true` cuando `ItemsGroupA.Count > 0`, dirigiendo el foco al primer elemento sin expulsar al botón volver.
6. **Pistas Sonoras y Semánticas**: Tonos armónicos diferenciados y descriptivos vinculados a los eventos de foco y exploración de los objetos manipulables.
7. **Sincronización Total de StarsTotal**: `RecalculateStars()` calcula `StarsTotal = LessonStarRatings.Values.Sum()` al terminar misiones y al cargar el perfil, desbloqueando `badge_star_collector` y actualizando el marcador superior.
8. **Interacción Semántica en Primaria**: Los manipulables de Primaria (monedas, reloj, regla, fracciones, barras) anuncian su valor nominal y descripción pedagógica (`item.Name` e `item.AccessibleDescription`), reservando el conteo cardinal exclusivamente para preescolar y retos de conteo básico.
9. **Aleatorización y Rejugabilidad**: `GenerateAdaptiveExercises` genera procedimentalmente retos con cifras dinámicas al rejugar lecciones de primaria/secundaria completadas.
10. **Consistencia de Manipulables de Primaria**:
    - `money_ex_01`: agregadas las dos monedas de $1 faltantes (2 de $5 y 3 de $1).
    - `money_ex_04`: enriquecido con billete, cuaderno de precio y monedas de cambio ($5 y tres de $1).
    - `ruler_ex_04`: enriquecido con marcas tangibles de inicio (0 cm), mitad (50 cm) y metro completo (100 cm).
11. **VerticalListNavigation Accesible**: `e.Handled = true;` restringido exclusivamente a `VirtualKey.Up` y `Down`; flechas `Left` y `Right` quedan libres para revisión de accesibilidad de NVDA/JAWS; eliminado `list.UpdateLayout()` innecesario.
12. **TreeView de Trofeos Expandido y Nombres Seguros**: Los grupos de trofeos inician expandidos por defecto en el mapa y `AutomationProperties.Name` se asigna tanto en el contenedor como directamente en el elemento de plantilla para prevenir nombres vacíos.
13. **Contadores Independientes en Comparación**: Grupo A y Grupo B disponen de contadores locales independientes (`_countedCountGroupA` y `_countedCountGroupB`) y orden relativo desde 1 para cada grupo.
14. **Throttle con Parámetro en NavigationService**: `_lastNavigatedParameter` previene el bloqueo de transiciones rápidas entre distintas lecciones del mismo tipo de página.
15. **Transición y Felicitación de Fin de Ciclo**: Detección de salto de subciclo en `MainWindow.xaml.cs` (Primaria Baja a Media, Media a Alta, etc.) con mensaje de felicitación de fin de etapa y botón de avance directo al siguiente nivel escolar.

**Resultado de Pruebas**: 239 pruebas unitarias aprobadas al 100% (239 passed, 0 failed, 0 warnings).
