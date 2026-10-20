# 🛠️ Especificación Técnica para Agentes de Código (Antigravity / OpenCode)
**Proyecto:** MathTutor (C# .NET 10 / WPF)  
**Objetivo:** Corrección de errores de accesibilidad WCAG 2.2 / DUA, usabilidad de teclado y reglas de negocio adaptativas.

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
* **Componente / Archivo:** `MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.WpfApp/Views/ProfileSetupView.xaml.cs`.
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
* **Componente / Archivo:** `MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/MissionCompletedDialog.xaml.cs`.
* **Comportamiento Detectado en Auditoría:** Al completar todas las misiones disponibles dentro de un rango de edad (por ejemplo, `kinder_counting_01` entre los 3 y 5 años), `GetNextTopic` hacía fallback a `topics[0]`, sugiriendo repetir la misma misión como *"Siguiente Misión"* en lugar de notificar que se completaron todas las misiones de la etapa.
* **Comportamiento Esperado:** Cuando no exista una misión siguiente en la secuencia ni otra misión pendiente en el rango de edad del alumno, `GetNextTopic` debe devolver `null` para que `MissionCompletedDialog` felicite al alumno por completar toda su etapa y muestre **"Volver al Menú de Misiones"**.

---

## 📌 Tarea 11: Navegación por Teclado y Lectura Limpia en la Sala de Trofeos (`ItemsBadges`)
* **Componente / Archivo:** `MathTutor.WpfApp/Views/RealmMapView.xaml`, `MathTutor.WpfApp/Views/RealmMapView.xaml.cs`.
* **Comportamiento Detectado en Auditoría:** Mientras que la lista de estaciones (`ItemsStations`) era un `ListBox` navegable con flechas, la cuadrícula de las 46 insignias (`ItemsBadges`) seguía siendo un `ItemsControl` estático cuyos elementos no recibían foco de teclado, impidiendo que un alumno ciego inspeccionara sus trofeos ganados.
* **Comportamiento Esperado:** Convertir `ItemsBadges` en un `ListBox` accesible con `KeyboardNavigation.DirectionalNavigation="Cycle"`, `KeyboardNavigation.TabNavigation="Once"`, foco de 4px (`AccessibleFocusVisual`) y etiqueta limpia sin emojis `AutomationProperties.Name` (`"Trofeo desbloqueado: {Título}. {Descripción}"` o `"Trofeo bloqueado: {Título}. {Descripción}"`).

---

## 📌 Tarea 12: Flexibilidad Pedagógica en `GetAllowedLevelsForAge` (12 Años en 1º de Secundaria y Discapacidad Intelectual)
* **Componente / Archivo:** `MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.Core/Engine/CurriculumService.cs`, vistas de perfil, menú y mapa.
* **Comportamiento Detectado en Auditoría:**
  1. Un alumno de 12 años que ya cursa `1º de Secundaria (12 a 13 años)` no veía disponible `SecondaryFirstGrade` en su selector.
  2. Un alumno de 13 a 15 años con `DisabilityType.Intellectual` solo veía misiones de Secundaria, sin poder repasar niveles base de Preescolar o Primaria si necesitaba refuerzo pedagógico.
* **Comportamiento Esperado:**
  1. Habilitar `SecondaryFirstGrade` también a partir de los 12 años junto con los niveles de Primaria.
  2. Añadir soporte para `DisabilityType` en `GetAllowedLevelsForAge(int age, DisabilityType disability = DisabilityType.None)` y `GetTopicsForAge(int age, DisabilityType disability = DisabilityType.None)` de modo que cuando `disability == DisabilityType.Intellectual` siempre se incluyan los niveles fundamentales (`Kindergarten`, `EarlyElementary`, `MiddleElementary`, `UpperElementary`) además de los niveles de su edad.
# INSTRUCCIONES DE ACCESIBILIDAD Y UI/UX PARA AGENTES DE CÓDIGO (MathTutor - Feedback de Usuario) 🦉✨

## Contexto del Proyecto
**MathTutor** es una aplicación en **C# (.NET 10 / WPF)** estructurada en tres proyectos:
- `MathTutor.Core`: Dominio, motor adaptativo, perfiles y generador procedimental.
- `MathTutor.WpfApp`: Interfaz XAML, servicios de accesibilidad (UIA / NVDA / JAWS) y audio.
- `MathTutor.Tests`: Suite de pruebas unitarias xUnit.

Se requiere implementar los siguientes ajustes de accesibilidad, lógica de juego y experiencia de usuario basados en la retroalimentación de pruebas de usuario.

---

## 🛠️ CORRECCIONES Y MEJORAS DE ACCESIBILIDAD Y TUTORIAL INTERACTIVO

### 1. Tutorial y Lección Interactiva Paso a Paso (Sin Lectura Continua "De Corrido")
- **Ubicación:** `MathTutor.WpfApp/Views/LessonView.xaml`, `MathTutor.WpfApp/Views/LessonView.xaml.cs` y `MathTutor.Core/Models/PedagogicalTopic.cs`.
- **Problema:** Las lecciones y tutoriales muestran y leen todo el contenido de un solo tirón (título, historia, situación, pasos y atajos). Esto genera fatiga auditiva, sobrecarga cognitiva y le resta interactividad al juego.
- **Instrucciones de Implementación:**
  1. Transformar el modelo de la lección/tutorial en un **flujo guiado paso a paso (Wizard/Stepper)** en `LessonView.xaml.cs`.
  2. Muestren y anuncien por voz **únicamente una tarjeta/paso a la vez**.
  3. En cada paso del tutorial, incluir una instrucción interactiva explícita (por ejemplo: *"Paso 1 de 3: {Explicación corta}. Presiona Enter o Espacio para continuar al siguiente paso."*).
  4. Permitir retroceder paso a paso con las flechas o la tecla `Backspace`.
  5. Una vez completados todos los pasos intermedios de la lección, habilitar la transición al reto o ejercicio práctico.

### 2. Soporte para Discapacidades Múltiples (Comorbilidad / Interseccionalidad)
- **Ubicación:** `MathTutor.Core/Models/UserProfile.cs`, `MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.WpfApp/Views/ProfileSetupView.xaml` y `MathTutor.WpfApp/Views/ProfileSetupView.xaml.cs`.
- **Problema:** En la pantalla de caracterización solo se permite seleccionar una única discapacidad, impidiendo configurar perfiles con múltiples necesidades (por ejemplo, Ceguera + TEA).
- **Instrucciones de Implementación:**
  1. Modificar la propiedad `DisabilityType` en `UserProfile` para que admita selecciones múltiples (`List<DisabilityType>` o `[Flags] Enum`).
  2. En `ProfileSetupView.xaml`, reemplazar los controles de selección única por casillas de verificación (`CheckBox`) accesibles.
  3. Asegurar que las adaptaciones (anuncios para lector de pantalla, paleta de baja estimulación sensorial y simplificación de lenguaje) se activen de forma simultánea cuando el usuario marque más de una opción.

### 3. Anuncio Completo de Opciones en los Retos (TTS y Lector de Pantalla)
- **Ubicación:** `MathTutor.WpfApp/Views/MiniGameView.xaml`, `MathTutor.WpfApp/Views/MiniGameView.xaml.cs` y `MathTutor.Core/Engine/AdaptiveEngine.cs`.
- **Problema:** Al iniciar un reto, la síntesis/lector lee la pregunta y la instrucción ("Elige usando 1, 2 o 3"), pero omite leer el contenido de cada opción, obligando al usuario a navegar por la interfaz para conocer las respuestas.
- **Instrucciones de Implementación:**
  1. En el método de inicio de reto y en el comando asociado a la tecla `F1` (Repetir), construir la cadena del anuncio concatenando explícitamente las opciones:
     `"{Pregunta}. Opción 1: {Opcion1}. Opción 2: {Opcion2}. Opción 3: {Opcion3}. Presiona 1, 2 o 3 para responder."`

### 4. Aleatorización de Opciones para Eliminar el Sesgo de Posición
- **Ubicación:** `MathTutor.Core/Engine/ExerciseGenerator.cs` (Generador procedimental de retos).
- **Problema:** La respuesta correcta suele aparecer casi siempre en la opción 2 (tecla 2), haciendo que el juego se vuelva predecible.
- **Instrucciones de Implementación:**
  1. En la fase final de generación de cada reto en `Core`, aplicar un algoritmo de aleatorización (*Fisher-Yates shuffle* o `OrderBy(x => Guid.NewGuid())`) a la lista de opciones.
  2. Actualizar el índice de la respuesta correcta tras el barajado para que se distribuya equitativamente entre las posiciones 1, 2 y 3.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.3.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **112/112 pruebas unitarias** superadas. Consolidado en el commit `7f66162` y publicado como release `v1.3.0` (`https://github.com/RHcomunications/MathTutor/releases/tag/v1.3.0`).

### 1. Migración de la solución a .NET 10
- **Corrección:** Los tres proyectos (`MathTutor.Core`, `MathTutor.WpfApp`, `MathTutor.Tests`) abandonan `net8.0` y se unifican en `net10.0` / `net10.0-windows`.
- **Ubicación:** `MathTutor.Core/MathTutor.Core.csproj`, `MathTutor.WpfApp/MathTutor.WpfApp.csproj`, `MathTutor.Tests/MathTutor.Tests.csproj`.
- **Criterio de verificación:** `dotnet build MathTutor.slnx -c Release /p:TreatWarningsAsErrors=true` → 0 advertencias / 0 errores.

### 2. Refactor de comorbilidad en los motores (`HasDisability`)
- **Corrección:** Sustituidas las comparaciones de discapacidad única (`profile.Disability == DisabilityType.Intellectual`) por el predicado de comorbilidad `profile.HasDisability(DisabilityType.Intellectual)`, de modo que las adaptaciones (rango numérico reducido, lenguaje simplificado y reducción de opciones) también se apliquen cuando la discapacidad intelectual coexiste con otras en `UserProfile.Disabilities`.
- **Ubicación:** `MathTutor.Core/Engine/AdaptiveEngine.cs`, `MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.Core/Engine/ExerciseGenerator.cs`.
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
- **Ubicación:** `MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/CurriculumMenuView.xaml` y `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`.
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
- **Ubicación:** `MathTutor.Core/Engine/ExerciseGenerator.cs`.
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
- **Ubicación:** `MathTutor.Core/Engine/CurriculumService.cs`, `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`, `MathTutor.WpfApp/Views/RealmMapView.xaml.cs`.
- **Criterio de verificación:** Prueba unitaria `CurriculumService_UnlocksSubsequentStageMissions_WhenBaseStageCompleted`.

### 3. Navegación Direccional por Flechas en `CurriculumMenuView`
- **Corrección:** Añadido `FocusInitialElement()` y gestión asíncrona de generación de contenedores (`ItemContainerGenerator.StatusChanged`) para posicionar el foco físico directamente sobre el primer `ListBoxItem`, habilitando de inmediato la navegación nativa con flechas arriba/abajo.
- **Ubicación:** `MathTutor.WpfApp/Views/CurriculumMenuView.xaml.cs`.
- **Criterio de verificación:** Al ingresar al menú, presionar `Flecha Abajo` o `Flecha Arriba` mueve inmediatamente la selección entre las tarjetas de misión.

### 4. Barajado / Aleatorización Eficaz en Retos Curados
- **Corrección:** Aplicación de `ShuffleOptions` a todos los ejercicios retornados por `CurriculumService.GenerateAdaptiveExercises`, incluyendo el ejercicio modelo inicial de cada lección, garantizando la alternancia equitativa de la opción correcta en cualquier intento.
- **Ubicación:** `MathTutor.Core/Engine/CurriculumService.cs`.
- **Criterio de verificación:** Prueba unitaria `CurriculumService_GenerateAdaptiveExercises_ShufflesCuratedFirstExerciseOptions`.

---

# ✅ REGISTRO DE CORRECCIONES REALIZADAS — Release `v1.6.0`

> **Estado:** Implementado y verificado. Build `Release` con `TreatWarningsAsErrors` sin advertencias y **114/114 pruebas unitarias** superadas. Publicado como release `v1.6.0`.

### 1. Migración funcional a WinUI 3
- **Corrección:** Añadido el proyecto `MathTutor.WinUI` con paridad de perfil, menú de misiones, mapa del reino, lecciones, minijuego, diálogo de misión y atajos globales. La solución conserva WPF y permite compilar ambas interfaces.
- **Ubicación:** `MathTutor.WinUI/`, `MathTutor.slnx`.
- **Criterio de verificación:** Build `Release` 0/0 y flujo E2E completo (lección, 3 ejercicios, diálogo de misión y regreso al mapa).

### 2. Accesibilidad para lectores de pantalla
- **Corrección:** Eliminados los anuncios duplicados al retirar `LiveSetting` de la región viva y usar `RaiseNotificationEvent`; añadidos nombres accesibles contextuales a botones de misión/estación, encabezados, contadores y controles; corregidos contrastes de bordes, éxito y foco.
- **Ubicación:** `MathTutor.WinUI/MainWindow.xaml.cs`, `MathTutor.WinUI/Services/`, `MathTutor.WinUI/Views/`, `MathTutor.WinUI/App.xaml`.
- **Criterio de verificación:** Inspección UIA de nombres, encabezados, botones y contadores; smoke test de cambio de tamaño de letra y navegación por teclado.

### 3. Alto contraste del sistema y tamaños mínimos
- **Corrección:** Sincronización con `AccessibilitySettings.HighContrast`, conservación de la preferencia del usuario al desactivar el alto contraste y sustitución de `Height` fijos por `MinHeight` para evitar recortes con fuentes grandes.
- **Ubicación:** `MathTutor.WinUI/MainWindow.xaml.cs`, `MathTutor.WinUI/Services/AccessibilityManager.cs`, vistas XAML.
- **Criterio de verificación:** Compilación estricta y revisión de recursos/foco por tema.