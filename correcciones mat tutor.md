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
  misión sin saltar vistas; "regreso a la lección" sin Escape en `GamePage` vale con
  `RequestBackToLesson`; "el menú" sucumbe con dedup de acordes (solo `MainWindow`
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
  `LessonStarRatings` trabajo en roundtrip en v3). Comportamiento Escape
  verificado en el arranque manual de la app.

## Fase 5 — Accesibilidad fina (AAA y lanzamientos por lector de pantalla)
- **Corrección:** Colores subidos a contraste AAA 7:1 en el test de `TextPrimary`, 
  `TextSecondary` y `AppError` para los 4 temas (en `HC-oscuro` se ajusta el error a
  RGB 255,120,120 y en `DefaultPlayful` a RGB 170,0,30); texto en chips/botones
  (`OnAccent`/`OnSuccess`) se mantiene ≥ 4.5:1. `CycleTheme` evita mintir cuando el
  Contraste del Sistema de Windows define el appreciate/tema. Atajos de teclado
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
  exactos si aplica), ya no da distractores viscosity loyalty triviales (-1/+1/+2) y elige
  distractores cercanos pero aleatorios con `jitter = span/3`; respaldo determinista
  siempre terminante para type ranges con solo 2 opciones posibles (caso fracción 1/2).
  Neutralizadas 21 descripciones `ScreenReaderDescription` que revelaban
  la respuesta (`Fase 6.1` = 8撰写 en `ExerciseGenerator.cs`; `Fase 6.2` = 13 reescritas en
  `CurriculumService.cs`: "Total: 6"→ "observa y cuenta…"; "Respuesta: 6 pasos"→
  "cuenta cuántas veces multiplicas…", etc.).
- **Ubicación:** `src/MathTutor.Core/Engine/ExerciseGenerator.cs`
  (`BuildOptions`, descripciones), `src/MathTutor.Core/Engine/CurriculumService.cs`.
- **Criterio de verificación:** 3 pruebas nuevas (`BuildOptions` con 2 rangos degenerados
  producen terminación y 3 opciones únicas; variedad > 6 combinatorias de distractores
  al ampliar semillas; cordura de opciones con 3 desperdicios obligados en retos de
  todos los temas de curriculum;pecifica). OJO: bug introducido y arreglado en esta fase
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
  explicación de éxito en lenguaje sin tecnicismos para TODOS los 61 tipos de juego; 
  `AdaptiveEngine.GetAdaptedHint` y `GetAdaptedSuccess` usan esas variantes como
  respaldo para alumnos con `SimplifyLanguage`/discapacidad intelectual en
  los ~45 temas de secundaria, preparatoria y programación (y jardín/primaria sin texto
  simple propio) cuando el ejercicio no trae variantes simplificadas a mano.
  Prueba cubre los 61 tipos del catálogo y el fallback en
  `sec_order_operations_01` con `SimplifyLanguage`.
- **Ubicación:** `src/MathTutor.Core/Engine/SimplifiedHintCatalog.cs` (nuevo),
  `src/MathTutor.Core/Engine/AdaptiveEngine.cs`,
  `tests/MathTutor.Tests/ExerciseGeneratorTests.cs`.
- **Criterio de verificación:** cobertura completa del catálogo (61 entradas) y
  fallback comprobado; build 0/0 y 196/196.
