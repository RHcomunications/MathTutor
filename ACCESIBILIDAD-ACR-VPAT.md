# Accesibilidad: borrador ACR/VPAT y matriz de validación

> **Estado:** borrador técnico de trabajo. No es un ACR/VPAT firmado ni una certificación formal. Las filas marcadas como pendientes requieren validación manual con lectores de pantalla y, para una declaración pública, una auditoría externa.

## 1. Alcance

- Producto: MathTutor Accesible para Windows 10/11 (WinUI 3).
- Nivel objetivo: WCAG 2.2 nivel AA, con refuerzos AAA en contraste y tamaño de texto.
- Tecnologías de apoyo previstas: NVDA, JAWS y Narrador de Windows.
- Fecha de este borrador: octubre de 2026.

## 2. Evidencia automatizada

| Control | Estado | Evidencia |
|---|---|---|
| Compilación Release sin advertencias | Cumple | `dotnet build MathTutor.slnx -c Release /p:TreatWarningsAsErrors=true` |
| Pruebas unitarias | Cumple | 209 pruebas, incluidas las de contraste AAA de los cuatro temas |
| Contraste de texto y bordes | Cumple | `ThemeContrastTests` verifica 4.5:1 para texto y 3:1 para bordes/foco |
| CI de build, pruebas y paquete | Cumple | `.github/workflows/ci.yml` |
| Dependencias fijadas | Cumple | Versiones exactas en los `.csproj` |
| Avisos de terceros | Cumple | `THIRD-PARTY-NOTICES.md` |
| Firma de código | Pendiente | Requiere certificado de firma de código |

## 3. Borrador ACR/VPAT (WCAG 2.2 AA)

| Criterio | Nivel | Estado | Evidencia o pendiente |
|---|---|---|---|
| 1.1.1 Contenido no textual | A | Parcial | Iconos y objetos tienen descripciones accesibles; falta revisión manual de todos los recursos. |
| 1.3.1 Información y relaciones | A | Parcial | Controles WinUI con nombres y agrupaciones; falta validación con árbol de accesibilidad. |
| 1.3.2 Secuencia significativa | A | Parcial | Orden de lectura definido en XAML; falta validación con lectores. |
| 1.4.1 Uso del color | A | Cumple | Formas, texto y retroalimentación no dependen solo del color. |
| 1.4.3 Contraste mínimo | AA | Cumple | Prueba automática de los cuatro temas. |
| 1.4.4 Cambio de tamaño del texto | AA | Cumple | Cuatro tamaños hasta 34–36 pt. |
| 1.4.6 Contraste mejorado | AAA | Parcial | Temas de alto contraste; no se declara AAA global. |
| 1.4.10 Reflujo | AA | Parcial | Ventana mínima 800×600; falta probar al 400 % de zoom. |
| 1.4.11 Contraste de elementos no textuales | AA | Cumple | Bordes y foco verificados a 3:1. |
| 1.4.12 Espaciado del texto | AA | Parcial | Falta probar el espaciado ampliado del sistema sin pérdida de contenido. |
| 2.1.1 Teclado | A | Cumple | Flujo completo con teclado y atajos F1–F6, flechas, Enter y 1–3. |
| 2.1.2 Sin trampas de teclado | A | Parcial | Falta validación manual de todos los diálogos. |
| 2.1.4 Atajos de teclado con caracteres | A | Parcial | Atajos 1–3 activos durante el juego; falta opción para desactivarlos o confirmar que solo aplican con el juego enfocado. |
| 2.2.1 Tiempo ajustable | A | Cumple | Sin límites de tiempo. |
| 2.3.1 Tres destellos o menos | A | Cumple | Sin animaciones de destello. |
| 2.4.1 Evitar bloques | A | Parcial | Navegación por regiones; falta validación con lector. |
| 2.4.2 Título de página | A | Cumple | Título de ventana y encabezados por pantalla. |
| 2.4.3 Orden del foco | A | Parcial | Orden lógico implementado; falta validación manual. |
| 2.4.4 Propósito de los enlaces | A | No aplica | Aplicación de escritorio sin enlaces web. |
| 2.4.6 Encabezados y etiquetas | AA | Cumple | Encabezados y nombres accesibles en las vistas. |
| 2.4.7 Foco visible | AA | Cumple | Borde de foco de 4 px y contraste verificado. |
| 2.5.1 Gestos de puntero | A | Cumple | Todas las acciones tienen alternativa por teclado. |
| 2.5.3 Etiqueta en el nombre | A | Parcial | Nombres accesibles revisados; falta barrido completo. |
| 3.1.1 Idioma de la página | A | Parcial | Textos en español; falta declarar idioma en todos los controles dinámicos. |
| 3.2.1 Al recibir el foco | A | Cumple | No cambia el contexto solo al enfocar. |
| 3.2.2 Al introducir datos | A | Cumple | Los cambios requieren acción explícita. |
| 3.3.1 Identificación de errores | A | Cumple | Retroalimentación diagnóstica y mensajes de validación. |
| 3.3.2 Etiquetas o instrucciones | A | Cumple | Instrucciones estándar y simplificadas. |
| 3.3.3 Sugerencia ante errores | AA | Cumple | Pistas y explicación del paso a revisar. |
| 4.1.2 Nombre, función y valor | A | Parcial | Controles WinUI nativos; falta inspección con Accessibility Insights. |
| 4.1.3 Mensajes de estado | AA | Parcial | Regiones vivas y anuncios UIA; falta validación con NVDA/JAWS/Narrador. |

## 4. Matriz manual de lectores de pantalla

Ejecutar en Windows 11 actualizado, con la app instalada desde el ZIP oficial.

| Flujo | NVDA | JAWS | Narrador | Observaciones |
|---|---|---|---|---|
| Inicio y primer foco | Pendiente | Pendiente | Pendiente | Verificar anuncio de bienvenida y foco inicial. |
| Crear perfil y apoyos | Pendiente | Pendiente | Pendiente | Verificar combos, casillas y lectura de ayudas. |
| Menú de misiones | Pendiente | Pendiente | Pendiente | Verificar nombre, progreso y estado completado. |
| Lección y secreto matemático | Pendiente | Pendiente | Pendiente | Verificar navegación por pasos y lectura repetible. |
| Juego y opciones 1–3 | Pendiente | Pendiente | Pendiente | Verificar anuncio de respuesta, pistas y errores. |
| Mapa y estaciones | Pendiente | Pendiente | Pendiente | Verificar orden y estado de estación actual. |
| Sala de trofeos | Pendiente | Pendiente | Pendiente | Verificar grupos, expansión y progreso. |
| Exportar y borrar perfil | Pendiente | Pendiente | Pendiente | Verificar diálogo de confirmación y anuncios. |
| Diálogo de misión completada | Pendiente | Pendiente | Pendiente | Verificar foco atrapado y retorno. |

## 5. Pendientes para cerrar Fase 4

- Ejecutar y registrar la matriz NVDA/JAWS/Narrador.
- Revisar el árbol de accesibilidad con Accessibility Insights for Windows.
- Probar zoom de texto del sistema y reflujo al 400 %.
- Obtener certificado y configurar firma de código.
- Revisar licencias y avisos con el titular antes de una distribución pública.
- Decidir si se contrata una auditoría externa para un ACR/VPAT firmado.
