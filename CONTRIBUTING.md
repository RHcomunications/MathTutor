# Guía de Contribución a MathTutor Accesible (CONTRIBUTING) 🦉✨

¡Gracias por tu interés en contribuir a **MathTutor Accesible**! Este proyecto tiene como misión brindar una experiencia de aprendizaje matemático lúdico, riguroso y 100% inclusivo para estudiantes desde preescolar hasta preparatoria y programación inclusiva, sin barreras físicas, sensoriales o cognitivas.

Apreciamos profundamente las contribuciones de desarrolladores, docentes, diseñadores pedagógicos, especialistas en educación especial y accesibilidad (WCAG/DUA), así como de usuarios y evaluadores de tecnologías de asistencia (NVDA, JAWS, Narrador, conmutadores y teclados accesibles).

---

## 📋 Tabla de Contenidos

1. [Código de Conducta y Principios de Inclusión](#-código-de-conducta-y-principios-de-inclusión)
2. [Criterios Innegociables del Proyecto](#-criterios-innegociables-del-proyecto)
3. [Cómo Empezar](#-cómo-empezar)
4. [Flujo de Trabajo con Git y Ramas](#-flujo-de-trabajo-con-git-y-ramas)
5. [Estándares de Código y Calidad](#-estándares-de-código-y-calidad)
6. [Pruebas Automatizadas y Validación](#-pruebas-automatizadas-y-validación)
7. [Reporte de Errores y Sugerencias](#-reporte-de-errores-y-sugerencias)
8. [Privacidad y Seguridad](#-privacidad-y-seguridad)

---

## 🤝 Código de Conducta y Principios de Inclusión

1. **Respeto a la Neurodiversidad**: Nos basamos en un modelo social y funcional de apoyos (TEA, TDAH, discapacidad intelectual, motriz, auditiva o visual). Jamás empleamos términos patologizantes, etiquetas capacitistas ni asumimos limitaciones en el potencial de aprendizaje del estudiante.
2. **Lenguaje Respetuoso y Amigable**: Toda comunicación en incidencias, revisiones de código y textos didácticos debe ser empática, constructiva y en un español neutro, claro y accesible para infancias y juventudes.
3. **Inclusión Real, no Simulada**: La accesibilidad no es un agregado opcional; es la arquitectura fundamental del proyecto.

---

## 🎯 Criterios Innegociables del Proyecto

Antes de proponer una contribución, asegúrate de que cumpla con los siguientes principios:

- **100% Operable por Teclado**: Todo el flujo (onboarding, mapa, lecciones, retos interactivos y configuración) debe poder completarse exclusivamente con teclado, sin ratón.
- **Cero Trampas de Foco**: No deben capturarse flechas de navegación si impiden el recorrido del cursor virtual del lector de pantalla (como en `VerticalListNavigation`).
- **Compatibilidad con Lectores de Pantalla**: Los textos, botones y manipulables deben tener nombres y descripciones semánticas accesibles compatibles con **NVDA**, **JAWS** y **Narrador de Windows**.
- **Contraste WCAG 2.2 Nivel AAA**:
  - Relación $\ge 7:1$ para texto regular sobre fondo.
  - Relación $\ge 4.5:1$ para texto grande y elementos de acento.
  - Borde de foco visible $\ge 3:1$ de 4px de grosor.
- **Rigor Matemático y Pedagógico**:
  - Cálculos y representaciones exactas (usar enteros escalados en geometría y cálculo para evitar imprecisiones de punto flotante).
  - Distractores pedagógicos bien fundamentados (errores conceptuales típicos, nunca valores absurdos o repetidos).
- **Compilación Limpia (0 Advertencias / 0 Errores)**: El repositorio compila con advertencias tratadas como errores en modo Release.

---

## 🛠️ Cómo Empezar

### Requisitos del Sistema
- **Sistema Operativo**: Windows 10 (versión 1809 o superior) o Windows 11.
- **SDK**: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) o superior.
- **Entorno de Desarrollo**: Visual Studio 2022 (v17.12+) con la carga de trabajo *Desarrollo para el escritorio con .NET* y componentes de *Windows App SDK*, o VS Code / Antigravity con extensiones de C#.
- **Lector de Pantalla para Pruebas**: Se recomienda tener instalado [NVDA](https://www.nvaccess.org/) (gratuito) para verificar anuncios y navegación.

### Configuración Local

1. Clona el repositorio:
   ```bash
   git clone https://github.com/RHcomunications/MathTutor.git
   cd MathTutor
   ```

2. Restaura y compila la solución en modo Release:
   ```bash
   dotnet build MathTutor.slnx -c Release
   ```

3. Ejecuta la suite de pruebas unitarias:
   ```bash
   dotnet test tests/MathTutor.Tests/MathTutor.Tests.csproj -c Release
   ```

4. Inicia la aplicación para desarrollo:
   ```bash
   dotnet run --project src/MathTutor.WinUI/MathTutor.WinUI.csproj -c Release
   ```

---

## 🌿 Flujo de Trabajo con Git y Ramas

1. **Crea una Rama Descriptiva**:
   - `feat/nombre-de-la-mejora` para nuevas características o estaciones.
   - `fix/descripcion-del-error` para correcciones de bugs o accesibilidad.
   - `docs/nombre-del-documento` para documentación y guías.

2. **Commits Semánticos (Conventional Commits)**:
   Usa mensajes claros en español con prefijos estándar:
   - `feat(curriculum): Añadir manipulables dinámicos para fracciones`
   - `fix(accessibility): Corregir anuncio en lector de pantalla al cambiar de estación`
   - `test(core): Agregar pruebas para cálculo de MCD y MCM`
   - `docs(readme): Actualizar instrucciones de instalación`

3. **Solicitud de Extracción (Pull Request)**:
   - Explica con claridad qué problema resuelve o qué valor pedagógico aporta.
   - Indica qué pruebas manuales realizaste (teclado, NVDA, temas de contraste).
   - Verifica que todas las pruebas automatizadas pasen exitosamente.

---

## 💻 Estándares de Código y Calidad

- **C# Moderno (.NET 10)**: Usar tipos de referencia anulables (`Nullable enable`), concordancia de patrones y sintaxis idiomática clara.
- **Separación de Responsabilidades**:
  - `MathTutor.Core`: Lógica de dominio puro, modelos, currículo, generador adaptativo, recompensas y gamificación. **No debe depender de APIs de WinUI ni de Windows Runtime**.
  - `MathTutor.WinUI`: Capa de presentación WinUI 3, servicios de audio sintetizado, temas XAML y puente de automatización accesible.
  - `MathTutor.Tests`: Pruebas unitarias xUnit que garantizan la cobertura integral de temas y accesibilidad.
- **XAML Limpio y Accesible**:
  - Toda etiqueta interactiva debe poseer `AutomationProperties.Name` o `AutomationProperties.HelpText`.
  - Los controles autocontenidos sin contenido hijo deben cerrarse explícitamente con `/>`.
  - Evitar márgenes fijos o tamaños rígidos que rompan el reflujo multilínea cuando el estudiante configure tamaños de fuente gigantes.

---

## 🧪 Pruebas Automatizadas y Validación

El proyecto cuenta con una batería de más de 230 pruebas automáticas en `tests/MathTutor.Tests`:
- Validación exhaustiva de las 74 estaciones curriculares y generadores procedimentales.
- Verificación matemática de soluciones y distractores.
- Validación de contratos de accesibilidad y ratios de contraste de color para todos los temas.
- Pruebas de rendimiento de latencia de audio y carga de catálogo.

Al crear o modificar ejercicios o lógica del motor, es obligatorio agregar o actualizar las pruebas correspondientes en `ExerciseGeneratorTests.cs`, `PedagogyAndAccessibilityTests.cs` o el archivo pertinente.

---

## 🐛 Reporte de Errores y Sugerencias

Si encuentras un error o tienes una idea para enriquecer la experiencia educativa:
1. Revisa las [Issues existentes](https://github.com/RHcomunications/MathTutor/issues) para evitar duplicados.
2. Abre un nuevo Issue detallando:
   - **Comportamiento observado vs. Comportamiento esperado**.
   - **Pasos exactos para reproducirlo**.
   - **Navegación utilizada**: Teclado exclusivo, ratón, lector de pantalla (indicando versión de NVDA/JAWS/Narrador).
   - **Tema y configuración del perfil** (edad, nivel, apoyos activos).

---

## 🔒 Privacidad y Seguridad

- MathTutor es una aplicación **completamente fuera de línea (offline-first)**.
- **No se aceptarán contribuciones que incorporen telemetría, rastreadores, librerías de analítica externa o dependencias que envíen datos de los estudiantes a través de internet.**
- Todos los datos del perfil y progreso deben residir exclusivamente en el almacenamiento local del equipo (`%LocalAppData%\MathTutor\user_profile.json`).

---

¡Gracias por formar parte de este esfuerzo por hacer que las matemáticas sean accesibles, divertidas y enriquecedoras para todos los estudiantes! 🚀✨
