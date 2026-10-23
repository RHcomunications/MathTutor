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
  - **Sonido con Significado**: Los patrones se reproducen como secuencias completas y cada objeto usa un timbre coherente con su instrumento o material (campana, tambor, flauta, palmas), reforzando el concepto sin sustituir al lector de pantalla.

- **Operable**:
  - **100% Navegable por Teclado**: Todo el flujo (crear perfil, seleccionar misiones, explorar el mapa del reino, jugar, responder retos y pedir pistas) se opera sin ratón.
  - **Atajos Directos de Respuesta**: Las opciones se responden instantáneamente con las teclas numéricas `1`, `2`, `3` o `Enter`.
  - **Foco Visible Destacado**: Borde visual de 4px de alto contraste que indica en todo momento la posición activa en la pantalla.
  - **Conteo sin Saltos de Foco**: Al marcar un objeto, su estado se actualiza en el lugar sin volver a dibujar la cuadrícula ni mover el foco; las flechas recorren los objetos y las opciones de forma predecible.
  - **Ventana al Frente al Iniciar**: Al abrir el programa, la ventana solicita el primer plano una sola vez después de cargar el perfil y la navegación inicial.
  - **Sin Límites de Tiempo**: Aprendizaje respetuoso del ritmo de cada alumno (Pauta WCAG 2.2).

- **Comprensible, Adaptable y Culturalmente Cercano**:
  - **Caracterización Inicial del Usuario**: Se pueden indicar una o varias condiciones (**Visual, Auditiva, Motriz, Intelectual/Cognitiva, Autismo, TDAH**) y describir los apoyos que necesita el alumno, sin obligar a declarar un diagnóstico ni usar una severidad genérica. El perfil distingue niveles de apoyo autista 1-3, presentación del TDAH, apoyos intelectuales y descriptores funcionales de visión, audición y motricidad.
  - **Adaptación Cultural Latinoamericana (`RegionalContext`)**: Configurable por país (**México, Colombia, Argentina, Perú, Chile**), adaptando dinámicamente monedas (`pesos` / `soles`, `centavos` / `céntimos`) y vocabulario cotidiano (`tiendita`, `tienda de barrio`, `kiosco`, `bodega`, `almacén`).
  - **Adaptación Automática del Lenguaje**: Si se indica discapacidad intelectual o edad temprana, el motor adaptativo simplifica el vocabulario, acorta los enunciados y reduce la cantidad de distractores/opciones sin sesgo de posición para evitar la sobrecarga cognitiva.
  - **Ejemplos Guiados Paso a Paso y Retroalimentación Diagnóstica**: Cada una de las 54 misiones incluye un ejemplo resuelto paso a paso en la pantalla de lección y retroalimentación diagnóstica inmediata cuando el alumno comete un error, explicando si su respuesta fue mayor o menor que la esperada y qué principio revisar.
  - **Repaso Sin Abandonar el Reto**: La pantalla del ejercicio mantiene visible el secreto matemático para releerlo, y el anuncio del reto no revela el resultado esperado.
  - **Sala de Trofeos Agrupada por Nivel**: Las insignias se organizan por nivel educativo, muestran el progreso de cada grupo, se expanden y contraen con las flechas y recuerdan su estado entre sesiones.
  - **Cierre de Misión con Estrellas**: El diálogo final indica cuántas estrellas se ganaron según los intentos y ofrece continuar con la siguiente misión o volver al mapa.
  - **Conteo sin Solapamientos**: Los objetos de conteo usan celdas de 170 px y se distribuyen en el ancho disponible para que los botones no se encimen.

- **Robusto**:
  - Arquitectura modular limpia en tres proyectos: `MathTutor.Core` (dominio, currículo de 54 estaciones y 16 objetivos, generador procedimental, motor adaptativo con filtrado por edad y recompensas), `MathTutor.WinUI` (servicios de accesibilidad, audio en memoria e interfaz WinUI 3) y `MathTutor.Tests` (suite automatizada de 168 pruebas unitarias xUnit).

---

## 🎮 Enfoque Pedagógico y Curricular (54 Estaciones / 16 Objetivos)

### 1. Jardín de Niños (3 a 5 años — Objetivo 1): Conteo, Formas y Primeras Nociones
1. **Conteo** (3 a 5 años): **Correspondencia uno a uno, Orden estable, Cardinalidad, Abstracción e Irrelevancia del orden** guiados por **Cosmo el Búho Sabio**.
2. **Formas y posiciones** (3 a 4 años): círculos, cuadrados, triángulos, tamaño, arriba/abajo y dentro/fuera.
3. **Cuentos, patrones, clasificación y música** (3 a 5 años): contar dentro de historias, continuar secuencias, agrupar por color, forma o tamaño, y contar sonidos.
4. **Números hasta 10, suma/resta y medida** (5 a 6 años): contar hasta diez, juntar y quitar, comparar longitudes y pesos, y ordenar del primero al último.
5. **Progresión por edad**: el menú filtra los temas según la edad y desbloquea el siguiente nivel solo cuando el alumno completa todos los temas de jardín disponibles para su edad.

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

### 5. Pensamiento Computacional y Programación Inclusiva (Objetivos 13 a 16 — Estaciones 45 a 54)
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
| **F6** | Abrir el Mapa del Reino y Medallero de Insignias (54 estaciones) |
| **Flechas (↑ ↓)** | Recorrer las misiones, las estaciones del mapa, los objetos de conteo y las respuestas de opción múltiple; avanzar o retroceder entre pasos de la lección |
| **R** | Repetir la secuencia sonora del patrón actual |
| **Escape** | Volver al menú principal de aventuras (o cerrar el diálogo al terminar una misión) |
| **1, 2, 3** | Responder directamente la opción correspondiente en el minijuego |
| **Enter / Espacio** | Iniciar misión seleccionada o avanzar a la siguiente misión tras completar un nivel |

---

## 🔒 Privacidad y documentación pedagógica

- [Aviso de privacidad para familias](PRIVACIDAD.md): qué datos guarda la app, dónde se almacenan y cómo exportar o borrar el perfil.
- [Bibliografía y criterios del modelo de apoyos](BIBLIOGRAFIA.md): referencias sobre autismo, TDAH, discapacidad intelectual, funcionamiento y accesibilidad.
- El perfil se guarda solo en `%LocalAppData%\MathTutor\user_profile.json`; la aplicación no usa telemetría ni envía datos a internet.

## 🔧 Calidad, licencia y accesibilidad

- [CI](.github/workflows/ci.yml): compila en Release con advertencias como errores, ejecuta las 168 pruebas y permite empaquetar manualmente.
- [Licencia propietaria](LICENSE) de Narayan Projects.
- [Avisos de terceros](THIRD-PARTY-NOTICES.md).
- [Borrador ACR/VPAT y matriz de lectores](ACCESIBILIDAD-ACR-VPAT.md).
- Los paquetes NuGet están fijados a versiones exactas para compilaciones reproducibles.

---

## 🚀 Release `v1.10.0`

Desde la terminal en esta carpeta:

```bash
# Compilar la solución en modo Release
dotnet build MathTutor.slnx -c Release

# Ejecutar las 168 pruebas unitarias automatizadas
dotnet test tests/MathTutor.Tests/MathTutor.Tests.csproj -c Release

# Iniciar la aplicación WinUI 3
dotnet run --project src/MathTutor.WinUI/MathTutor.WinUI.csproj -c Release

# Publicar y empaquetar WinUI (genera artifacts/releases/MathTutor-v1.10.0-Windows.zip)
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/publish-winui.ps1
```

Las compilaciones y publicaciones se concentran en `artifacts/`; los proyectos ya no generan carpetas `bin/` ni `obj/` propias. La release se entrega como una carpeta `MathTutor-v1.10.0` con `MathTutor.exe` en la raíz, `LEEME.txt`, una carpeta `libs/` con la aplicación y sus bibliotecas, y una carpeta `modules/` con los módulos `.winmd`. El ejecutable de la raíz es un lanzador que inicia la aplicación real desde `libs/`.

```text
src/MathTutor.Core/    Dominio, currículo y motor adaptativo
src/MathTutor.WinUI/   Aplicación WinUI 3
tests/MathTutor.Tests/ Suite xUnit
scripts/               Publicación y empaquetado
LEEME.txt              Guía para familias, docentes y estudiantes
artifacts/             Salidas de compilación y releases (ignorado por Git)
```

