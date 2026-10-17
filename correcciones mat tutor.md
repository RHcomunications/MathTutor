# 🛠️ Especificación Técnica para Agentes de Código (Antigravity / OpenCode)
**Proyecto:** MathTutor (C# .NET 10 / WPF)  
**Objetivo:** Corrección de errores de accesibilidad WCAG 2.2 / DUA, usabilidad de teclado y reglas de negocio adaptativas.

---

## 📌 Tarea 1: Foco Inicial Automático y Anuncio de Entrada al Iniciar
* **Componente / Archivo:** `MathTutor.WpfApp/Views/MainWindow.xaml`, `MainWindow.xaml.cs` (o vista principal inicial).
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
         AccessibilityService.Announce("Bienvenido a MathTutor. Presiona Tab para navegar por las opciones.");
     };
     ```
  2. Verificar que `AutomationProperties.Name` del primer elemento lea claramente la instrucción de inicio.

---

## 📌 Tarea 2: Navegación Direccional (Flechas) y Maquetación de la Lista de Misiones
* **Componente / Archivo:** `MathTutor.WpfApp/Views/MissionMapView.xaml`, `MissionMapViewModel.cs`.
* **Comportamiento Actual:** Las misiones se muestran en grupos pequeños de 2 o 3, requiriendo presionar la tecla `Tab` repetidamente. Las teclas de flecha no permiten cambiar de misión de manera fluida.
* **Comportamiento Esperado:** El listado de misiones debe comportarse como una lista accesible con navegación nativa por teclas de dirección (`Flecha Arriba` / `Flecha Abajo` / `Flecha Izquierda` / `Flecha Derecha`).
* **Instrucciones de Implementación:**
  1. Utilizar un `ListBox` o `ListView` WPF con `KeyboardNavigation.DirectionalNavigation="Cycle"` en lugar de contenedores estáticos o `ScrollViewer` aislados.
  2. Definir `ItemContainerStyle` para que la tarjeta seleccionada reciba el borde de foco de 4px (`FocusVisualStyle`).
  3. Asegurar que `IsSynchronizedWithCurrentItem="True"` para que mover el foco entre items con las flechas seleccione e informe la misión activa automáticamente.

---

## 📌 Tarea 3: Estandarización de Etiquetas Accesibles (`AutomationProperties`)
* **Componente / Archivo:** `MathTutor.WpfApp/Controls/MissionCardControl.xaml`, plantillas de botones de misión.
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
* **Componente / Archivo:** `MathTutor.WpfApp/ViewModels/QuizViewModel.cs`, `AccessibilityService.cs`.
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
* **Componente / Archivo:** `MathTutor.WpfApp/Services/AccessibilityService.cs`, `TTSManager.cs`.
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
* **Componente / Archivo:** `MathTutor.Core/Services/AdaptiveEngine.cs`, `MathTutor.WpfApp/ViewModels/ProfileViewModel.cs`.
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
* **Componente / Archivo:** `MathTutor.WpfApp/Views/MissionCompletedDialog.xaml`, `QuizViewModel.cs`.
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