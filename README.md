# Tutor de Matemáticas Accesible (MathTutor) 🦉✨
**100% Accesible | Conforme a WCAG 2.2 / DUA | Basado en el Juego**

Desarrollado en **C# (.NET 10 / WPF)** para el aprendizaje matemático significativo, inclusivo y sin barreras, abarcando desde **Jardín de Niños (3 a 5 años)** y **Primaria Baja (6 a 8 años)** hasta grados superiores y semillas de pensamiento computacional y programación.

---

## 🌟 Características Principales

### 1. ♿ 100% Accesible y Conforme a WCAG 2.2 (Nivel AA y AAA)
- **Perceptible**:
  - **Síntesis de Voz Integrada (`System.Speech`)**: Lee automáticamente instrucciones, preguntas, opciones, diálogos y retroalimentación sin requerir configuraciones adicionales.
  - **Compatibilidad con Lectores Externos de Pantalla**: Diseñado con **Microsoft UI Automation (`AutomationProperties.Name`, `HelpText`, `LiveSetting = "Polite" / "Assertive"`)**, compatible con **NVDA, JAWS y el Narrador de Windows**.
  - **Paletas de Alto Contraste**:
    - *Colorido Lúdico* (estándar infantil verificado > 4.5:1).
    - *Alto Contraste Oscuro* (Fondo negro con texto en amarillo y blanco brillante > 15:1).
    - *Alto Contraste Claro* (Fondo blanco puro con texto negro absoluto > 21:1).
    - *Baja Estimulación Sensorial* (Tonos pastel suaves anti-fatiga, ideal para TEA y TDAH).
  - **Escala de Fuentes Dinámica**: Estándar (18pt), Grande (23pt), Muy Grande (28pt) y Gigante (34pt).
  - **Audio Procedural en Memoria**: Generador acústico PCM WAV en tiempo real con frecuencias musicales para el conteo armónico (Do, Re, Mi, Fa, Sol, La, Si, Do) y acordes de éxito, sin depender de archivos externos.

- **Operable**:
  - **100% Navegable por Teclado**: Todo el flujo (crear perfil, seleccionar misiones, jugar, responder retos y pedir pistas) se opera sin ratón.
  - **Atajos Directos de Respuesta**: Las opciones se responden instantáneamente con las teclas numéricas `1`, `2`, `3` o `Enter`.
  - **Foco Visible Destacado**: Borde visual de 4px de alto contraste que indica en todo momento la posición activa en la pantalla.
  - **Sin Límites de Tiempo**: Aprendizaje respetuoso del ritmo de cada alumno (Pauta WCAG 2.2).

- **Comprensible y Adaptable**:
  - **Caracterización Inicial del Usuario**: Se configura el tipo de discapacidad (**Visual, Auditiva, Motriz, Intelectual/Cognitiva, Neurodivergente, Sin discapacidad**) y su severidad (**Leve, Moderada, Severa**).
  - **Adaptación Automática del Lenguaje**: Si se indica discapacidad intelectual o edad temprana, el motor adaptativo simplifica el vocabulario, acorta los enunciados y reduce la cantidad de distractores/opciones para evitar la sobrecarga cognitiva.

- **Robusto**:
  - Arquitectura modular limpia en dos capas: `MathTutor.Core` (dominio, pedagogía, lógica adaptativa) y `MathTutor.WpfApp` (servicios de accesibilidad, interfaz XAML).

---

## 🎮 Enfoque Pedagógico y Curricular

### 1. Jardín de Niños (3 a 5 años): Los 5 Principios del Conteo (Gelman y Gallistel)
Guiado por **Cosmo el Búho Sabio**:
1. **Correspondencia uno a uno (biunívoca)**: A cada objeto le corresponde una y sólo una palabra numérica. (Repartir manzanas a los conejitos hambrientos con notas musicales ascendentes).
2. **Orden estable**: La serie numérica siempre tiene la misma secuencia fija e invariable ($1, 2, 3, 4, 5...$). (El camino de piedras musicales del río).
3. **Cardinalidad**: El último número dicho al contar una colección representa el total absoluto de objetos.
4. **Abstracción**: Cualquier grupo de objetos reales o imaginarios puede contarse (estrellas, campanas, juguetes).
5. **Irrelevancia del orden**: El resultado del conteo es idéntico sin importar el orden o dirección en que se cuenten los elementos.

### 2. Primaria Baja (6 a 8 años): Fundamentos y Propiedades de las Operaciones
1. **Fundamentos de la Suma (Adición)**: Juntar colecciones, unir grupos y avanzar en la recta numérica.
2. **Propiedad Conmutativa**: $a + b = b + a$ (*"El orden de los amigos no cambia la fiesta"*).
3. **Propiedad Asociativa**: $(a + b) + c = a + (b + c)$ (*"Agrupar en diferentes vagones da el mismo tren"*).
4. **Elemento Neutro de la Suma**: $a + 0 = a$ (*"El Cero es como una burbuja transparente: no cambia tu cantidad"*).
5. **Fundamentos de la Resta (Sustracción)**: Quitar, separar y retroceder en el camino numérico.
6. **Relación Inversa entre Suma y Resta**: Si $4 + 3 = 7$, entonces $7 - 3 = 4$ (*"La máquina del tiempo matemática"*).

### 3. Semilla hacia Niveles Superiores y Programación
- **Suma Repetida y Producto**: $2 + 2 + 2 = 3 \times 2 = 6$.
- **Pensamiento Computacional (Bucles / Loops)**: *"Repetir 3 veces: dar 2 pasos"*. Conecta la multiplicación con los bucles de código de las computadoras.

---

## ⌨️ Atajos de Teclado Globales de Accesibilidad

| Tecla | Acción |
|---|---|
| **F1** | Repetir en voz alta la última instrucción o explicación |
| **F2** | Activar / Desactivar la síntesis de voz (TTS) |
| **F3** | Cambiar cíclicamente el esquema de contraste (Lúdico, Alto Contraste Oscuro, Claro, Baja Estimulación) |
| **F4** | Cambiar cíclicamente el tamaño de letra (Estándar, Grande, Muy Grande, Gigante) |
| **F5** | Abrir la pantalla de caracterización y perfil del alumno |
| **Escape** | Volver al menú principal de aventuras |
| **1, 2, 3** | Responder directamente la opción correspondiente en el minijuego |
| **Enter / Espacio** | Activar botón seleccionado o avanzar al siguiente reto tras acertar |

---

## 🚀 Cómo Ejecutar el Proyecto

Desde la terminal en esta carpeta:

```bash
# Compilar la solución
dotnet build

# Ejecutar las pruebas unitarias automatizadas
dotnet test

# Iniciar la aplicación interactiva
dotnet run --project MathTutor.WpfApp/MathTutor.WpfApp.csproj
```
