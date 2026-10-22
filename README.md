# Tutor de Matemáticas Accesible (MathTutor) 🦉✨
**100% Accesible | Conforme a WCAG 2.2 / DUA | Basado en el Juego | Adaptado a México y Latinoamérica**

Desarrollado en **C# (.NET 10 / WinUI 3)** para el aprendizaje matemático significativo, inclusivo y sin barreras, abarcando desde **Jardín de Niños (3 a 5 años)**, **Primaria Completa (1º a 6º grado)**, **Secundaria Completa (1º a 3º grado)** y **Educación Media Superior / Preparatoria (1º a 3º grado)** hasta **Pensamiento Computacional y Programación Inclusiva**.

---

## 🌟 Características Principales

### 1. ♿ 100% Accesible y Conforme a WCAG 2.2 (Nivel AA y AAA)
- **Perceptible**:
  - **Integración Nativa con el Lector de Pantalla del Sistema (NVDA, JAWS y Narrador de Windows)**: Sin voces robóticas SAPI5 que interfieran o hablen lento; el tutor envía todos los anuncios directamente al lector de pantalla activo del usuario mediante **NVDA Controller Client**, **JAWS COM API (`FreedomScientific.JawsApi`)** y **Regiones en Vivo Asertivas de Microsoft UI Automation (`LiveRegionChanged`)**, respetando al 100% el sintetizador, la velocidad de lectura y el teclado del usuario.
  - **Exploración Línea por Línea y Carácter por Carácter**: Las tarjetas de lección, fórmulas algebraicas, ejemplos guiados y bloques de código son navegables directamente con las flechas del teclado para una inspección detallada con el cursor del lector de pantalla.
  - **Paletas de Alto Contraste**:
    - *Colorido Lúdico* (estándar infantil verificado > 4.5:1).
    - *Alto Contraste Oscuro* (Fondo negro con texto en amarillo y blanco brillante > 15:1).
    - *Alto Contraste Claro* (Fondo blanco puro con texto negro absoluto > 21:1).
    - *Baja Estimulación Sensorial* (Tonos pastel suaves anti-fatiga, ideal para TEA y TDAH).
  - **Escala de Fuentes Dinámica**: Estándar (18pt), Grande (23pt), Muy Grande (28pt) y Gigante (34pt).
  - **Audio Procedural en Memoria**: Generador acústico PCM WAV en tiempo real con frecuencias musicales para el conteo armónico (Do, Re, Mi, Fa, Sol, La, Si, Do) y acordes de éxito, sin depender de archivos externos.

- **Operable**:
  - **100% Navegable por Teclado**: Todo el flujo (crear perfil, seleccionar misiones, explorar el mapa del reino, jugar, responder retos y pedir pistas) se opera sin ratón.
  - **Atajos Directos de Respuesta**: Las opciones se responden instantáneamente con las teclas numéricas `1`, `2`, `3` o `Enter`.
  - **Foco Visible Destacado**: Borde visual de 4px de alto contraste que indica en todo momento la posición activa en la pantalla.
  - **Sin Límites de Tiempo**: Aprendizaje respetuoso del ritmo de cada alumno (Pauta WCAG 2.2).

- **Comprensible, Adaptable y Culturalmente Cercano**:
  - **Caracterización Inicial del Usuario**: Se configura el tipo de discapacidad (**Visual, Auditiva, Motriz, Intelectual/Cognitiva, Neurodivergente, Sin discapacidad**) y su severidad (**Leve, Moderada, Severa**).
  - **Adaptación Cultural Latinoamericana (`RegionalContext`)**: Configurable por país (**México, Colombia, Argentina, Perú, Chile**), adaptando dinámicamente monedas (`pesos` / `soles`, `centavos` / `céntimos`) y vocabulario cotidiano (`tiendita`, `tienda de barrio`, `kiosco`, `bodega`, `almacén`).
  - **Adaptación Automática del Lenguaje**: Si se indica discapacidad intelectual o edad temprana, el motor adaptativo simplifica el vocabulario, acorta los enunciados y reduce la cantidad de distractores/opciones sin sesgo de posición para evitar la sobrecarga cognitiva.
  - **Ejemplos Guiados Paso a Paso y Retroalimentación Diagnóstica**: Cada una de las 45 misiones incluye un ejemplo resuelto paso a paso en la pantalla de lección y retroalimentación diagnóstica inmediata cuando el alumno comete un error, explicando si su respuesta fue mayor o menor que la esperada y qué principio revisar.
  - **Repaso Sin Abandonar el Reto**: La pantalla del ejercicio mantiene visible el secreto matemático para releerlo, y el anuncio del reto no revela el resultado esperado.
  - **Sala de Trofeos Agrupada por Nivel**: Las insignias se organizan por nivel educativo, muestran el progreso de cada grupo, se expanden y contraen con las flechas y recuerdan su estado entre sesiones.

- **Robusto**:
  - Arquitectura modular limpia en tres proyectos: `MathTutor.Core` (dominio, currículo de 45 estaciones y 16 objetivos, generador procedimental, motor adaptativo con filtrado por edad y recompensas), `MathTutor.WinUI` (servicios de accesibilidad, audio en memoria e interfaz WinUI 3) y `MathTutor.Tests` (suite automatizada de 116 pruebas unitarias xUnit).

---

## 🎮 Enfoque Pedagógico y Curricular (45 Estaciones / 16 Objetivos)

### 1. Jardín de Niños (3 a 5 años — Objetivo 1): Los 5 Principios del Conteo (Gelman y Gallistel)
1. **Correspondencia uno a uno, Orden estable, Cardinalidad, Abstracción e Irrelevancia del orden** guiados por **Cosmo el Búho Sabio**.

### 2. Primaria Baja, Media y Alta (1º a 6º Grado — 6 a 12 años — Objetivos 2 a 6)
- **Primaria Baja (1º y 2º)**: Adición (propiedades conmutativa, asociativa y elemento neutro $0$), Sustracción y relación inversa, Multiplicación con arreglos rectangulares.
- **Primaria Media (3º y 4º)**: División por reparto equitativo, Fracciones propias y equivalentes, Multiplicación distributiva por descomposición en bloques.
- **Primaria Alta (5º y 6º)**: Fracciones mixtas, Decimales con moneda regional, Potencias (cuadrados y cubos), Proporcionalidad directa, Balanza de ecuaciones pre-álgebra, Raíz cuadrada exacta y Coordenadas cartesianas $(X, Y)$ en el primer cuadrante.

### 3. Educación Secundaria (1º a 3º Grado — 12 a 15 años — Objetivos 7 a 9)
- **1º de Secundaria**: Números enteros con signo (saldos y deudas en la tiendita), Jerarquía de operaciones y paréntesis (PEMDAS), Ecuaciones lineales $ax + b = c$, y Porcentajes y descuentos comerciales.
- **2º de Secundaria**: Leyes de los exponentes, Polinomios y reducción de términos semejantes, Sistemas de ecuaciones lineales $2 \times 2$, y Teorema de Pitágoras ($a^2 + b^2 = c^2$).
- **3º de Secundaria**: Ecuaciones cuadráticas ($ax^2 + c = d$), Semejanza de triángulos y Teorema de Tales, Razones trigonométricas ($\text{sen}, \cos, \tan$), y Funciones cuadráticas y parábolas ($y = ax^2 + c$).

### 4. Educación Media Superior / Preparatoria (1º a 3º Grado — 15 a 18 años — Objetivos 10 a 12)
- **1º de Preparatoria**: Distancia entre dos puntos en el plano analítico, Pendiente de la recta ($m = \frac{y_2 - y_1}{x_2 - x_1}$), y Ecuación pendiente-ordenada al origen ($y = mx + b$).
- **2º de Preparatoria**: Secciones cónicas y ecuación de la circunferencia ($x^2 + y^2 = r^2$), Funciones periódicas y ondas senoidales ($y = A\operatorname{sen}(x)$), y Funciones exponenciales y logaritmo base 2 ($\log_2(n)$).
- **3º de Preparatoria**: Cálculo diferencial y derivada como razón de cambio instantánea ($f'(t)$), Cálculo integral y área bajo la curva ($\int_a^b f(t)\,dt$), y Probabilidad y estadística descriptiva para ciencia de datos ($\bar{x}$).

### 5. Pensamiento Computacional y Programación Inclusiva (Objetivos 13 a 16 — Estaciones 36 a 45)
- **Semilla de Programación (Objetivo 13)**: Suma repetida y bucles de código (`REPETIR N VECES`).
- **Módulo 1 de Programación (Objetivo 14)**: Variables, asignación y estado en memoria (`x = x + a`), Condicionales `SI / SI NO` (`if / else`), y Lógica Booleana con compuertas `Y (AND)`, `O (OR)` y `NO (NOT)`.
- **Módulo 2 de Programación (Objetivo 15)**: Ciclos `PARA` (`for`) con variables acumuladoras, Arreglos (`Arrays`) e indexación basada en cero (`[0, 1, 2...]`), y Funciones modulares con parámetros y valor de retorno (`return`).
- **Módulo 3 de Programación (Objetivo 16)**: Algoritmos paso a paso de búsqueda del máximo/mínimo, Física y coordenadas 2D en videojuegos accesibles ($x = x_0 + v \cdot t$), y Programación de Inteligencia Artificial Asistiva con filtrado estadístico de sensores.

---

## ⌨️ Atajos de Teclado Globales de Accesibilidad

El botón **Opciones del juego** abre un menú vertical: usa **↑ / ↓** para recorrerlo y **Enter** para elegir. Dentro de **Opciones de accesibilidad** encontrarás Contraste, Letra, Sonidos y Repetir. **Escape** cierra el menú; los atajos F1–F6 siguen disponibles.

| Tecla | Acción |
|---|---|
| **F1** | Repetir en voz alta la última instrucción o explicación |
| **F2** | Activar / Desactivar los sonidos internos del juego (el lector de pantalla permanece siempre activo) |
| **F3** | Cambiar cíclicamente el esquema de contraste (Lúdico, Alto Contraste Oscuro, Claro, Baja Estimulación) |
| **F4** | Cambiar cíclicamente el tamaño de letra (Estándar, Grande, Muy Grande, Gigante) |
| **F5** | Abrir la pantalla de caracterización y perfil del alumno |
| **F6** | Abrir el Mapa del Reino y Medallero de Insignias (45 estaciones) |
| **Flechas (↑ ↓)** | Recorrer las misiones, las estaciones del mapa y las respuestas de opción múltiple; avanzar o retroceder entre pasos de la lección |
| **Escape** | Volver al menú principal de aventuras (o cerrar el diálogo al terminar una misión) |
| **1, 2, 3** | Responder directamente la opción correspondiente en el minijuego |
| **Enter / Espacio** | Iniciar misión seleccionada o avanzar a la siguiente misión tras completar un nivel |

---

## 🚀 Cómo Ejecutar el Proyecto (Release `v1.9.0`)

Desde la terminal en esta carpeta:

```bash
# Compilar la solución en modo Release
dotnet build MathTutor.slnx -c Release

# Ejecutar las 116 pruebas unitarias automatizadas
dotnet test tests/MathTutor.Tests/MathTutor.Tests.csproj -c Release

# Iniciar la aplicación WinUI 3
dotnet run --project src/MathTutor.WinUI/MathTutor.WinUI.csproj -c Release

# Publicar y empaquetar WinUI (genera artifacts/releases/MathTutor-v1.9.0-Windows.zip)
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/publish-winui.ps1
```

Las compilaciones y publicaciones se concentran en `artifacts/`; los proyectos ya no generan carpetas `bin/` ni `obj/` propias. La release se entrega como una carpeta `MathTutor-v1.9.0` con `MathTutor.exe` en la raíz, `LEEME.txt`, una carpeta `libs/` con la aplicación y sus bibliotecas, y una carpeta `modules/` con los módulos `.winmd`. El ejecutable de la raíz es un lanzador que inicia la aplicación real desde `libs/`.

```text
src/MathTutor.Core/    Dominio, currículo y motor adaptativo
src/MathTutor.WinUI/   Aplicación WinUI 3
tests/MathTutor.Tests/ Suite xUnit
scripts/               Publicación y empaquetado
LEEME.txt              Guía para familias, docentes y estudiantes
artifacts/             Salidas de compilación y releases (ignorado por Git)
```

