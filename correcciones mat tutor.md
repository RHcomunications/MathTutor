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